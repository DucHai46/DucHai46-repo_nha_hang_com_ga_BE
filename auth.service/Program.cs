using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using auth.service.Context;
using MongoDbGenericRepository.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Microsoft.AspNetCore.Identity; // Added for UserManager
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";


BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var mongoDbConfig = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new MongoDbSettings
    {
        ConnectionString = "mongodb://admin:admin123@3.24.10.252:27017",
        DatabaseName = "Users"
    },
    IdentityOptionsAction = options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
    }
};

// Sử dụng CustomMongoDbContext
var mongoDbContext = new CustomMongoDbContext(
    mongoDbConfig.MongoDbSettings.ConnectionString,
    mongoDbConfig.MongoDbSettings.DatabaseName
);

builder.Services.ConfigureMongoDbIdentity<MongoUser, MongoIdentityRole<Guid>, Guid>(
    mongoDbConfig,
    mongoDbContext
).AddRoles<MongoIdentityRole<Guid>>() // Thêm dòng này
  .AddRoleManager<RoleManager<MongoIdentityRole<Guid>>>(); // Và dòng này

// Add this authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Auth Service API", Version = "v1" });

    // Cấu hình JWT Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Thêm yêu cầu bắt buộc phải có token
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();
// Tạo vai trò mặc định
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<MongoIdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MongoUser>>();
    
    // Tạo vai trò Admin nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new MongoIdentityRole<Guid>("Admin"));
    }
    
    // Tạo vai trò User nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new MongoIdentityRole<Guid>("User"));
    }
    
    // Tạo vai trò Manager nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Manager"))
    {
        await roleManager.CreateAsync(new MongoIdentityRole<Guid>("Manager"));
    }
    
    // Tạo tài khoản Admin mặc định nếu chưa có
    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new MongoUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            FullName = "Administrator"
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "AuthService v1"));
}

app.UseCors(myAllowSpecificOrigins);

app.UseHttpsRedirection();

// Add this line before UseAuthorization
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

[CollectionName("Users")]
public class MongoUser : MongoIdentityUser<Guid>
{
    public string FullName { get; set; } // Add custom fields as needed
}

