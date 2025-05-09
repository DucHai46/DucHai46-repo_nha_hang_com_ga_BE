using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt; 
using Microsoft.IdentityModel.Tokens; 
using Microsoft.AspNetCore.Authorization;
using AspNetCore.Identity.MongoDbCore.Models; 

namespace auth.service.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly UserManager<MongoUser> _userManager;
    private readonly RoleManager<MongoIdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<MongoUser> userManager, 
        RoleManager<MongoIdentityRole<Guid>> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            // Kiểm tra tồn tại
            if (await _userManager.FindByNameAsync(model.Username) != null)
                return BadRequest("Username đã tồn tại");

            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest("Email đã tồn tại");

            // Role mặc định
            string roleToAssign = "User";
            if (!string.IsNullOrEmpty(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                    return BadRequest($"Vai trò '{model.Role}' không tồn tại");
                roleToAssign = model.Role;
            }

            // Tạo user
            var user = new MongoUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName ?? model.Username
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // ❗ Wait until MongoDB writes are complete (retry loop)
            MongoUser? createdUser = null;
            for (int i = 0; i < 5; i++)
            {
                createdUser = await _userManager.FindByNameAsync(user.UserName);
                if (createdUser != null)
                    break;
                await Task.Delay(100); // wait 100ms between retries
            }

            if (createdUser == null)
                return StatusCode(500, "Không thể lấy user sau khi tạo");

            // ❗ Gán role với bản user mới từ DB
            var roleResult = await _userManager.AddToRoleAsync(createdUser, roleToAssign);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(createdUser);
                return BadRequest(roleResult.Errors);
            }

            return Ok(new { UserId = createdUser.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        // Thêm vai trò vào claims
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!string.IsNullOrEmpty(user.Email)) // Added null check for user.Email
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(1000),
            signingCredentials: creds
        );

        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("roles")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới có thể tạo vai trò mới
    public async Task<IActionResult> CreateRole([FromBody] RoleModel model)
    {
        if (string.IsNullOrEmpty(model.Name))
            return BadRequest("Tên vai trò không được để trống");

        // Kiểm tra xem vai trò đã tồn tại chưa
        if (await _roleManager.RoleExistsAsync(model.Name))
            return BadRequest($"Vai trò '{model.Name}' đã tồn tại");

        // Tạo vai trò mới
        var role = new MongoIdentityRole<Guid>(model.Name);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Name = model.Name });
    }

    [HttpPost("user/{userId}/roles")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới có thể gán vai trò
    public async Task<IActionResult> AddUserToRole(string userId, [FromBody] RoleModel model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"Không tìm thấy người dùng với ID: {userId}");

        if (!await _roleManager.RoleExistsAsync(model.Name))
            return BadRequest($"Vai trò '{model.Name}' không tồn tại");

        var result = await _userManager.AddToRoleAsync(user, model.Name);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { UserId = userId, Role = model.Name });
    }

    [HttpGet("user/{userId}/roles")]
    [Authorize] // Người dùng đã đăng nhập có thể xem vai trò
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"Không tìm thấy người dùng với ID: {userId}");

        // Kiểm tra quyền: chỉ Admin hoặc chính người dùng đó mới có thể xem vai trò
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized();

        if (currentUser.Id.ToString() != userId && !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            return Forbid();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new { UserId = userId, Roles = roles });
    }
}

public class RegisterModel
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? FullName { get; set; }
}

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RoleModel
{
    public string Name { get; set; } = string.Empty;
}
