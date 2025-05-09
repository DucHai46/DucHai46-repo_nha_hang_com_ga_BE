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
        var user = new MongoUser
        {
            UserName = model.Username,
            Email = model.Email,
            FullName = model.Username // hoặc nhận từ client nếu cần
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Gán vai trò mặc định cho người dùng mới (nếu có)
        if (!string.IsNullOrEmpty(model.Role))
        {
            await _userManager.AddToRoleAsync(user, model.Role);
        }
        else
        {
            // Gán vai trò "User" mặc định nếu không có vai trò được chỉ định
            await _userManager.AddToRoleAsync(user, "User");
        }

        return Ok(new { UserId = user.Id });
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
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Role { get; set; } // Thêm trường Role (tùy chọn)
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RoleModel
{
    public string Name { get; set; }
}
