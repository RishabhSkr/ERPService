using Microsoft.EntityFrameworkCore;
using MyERP.Services.Identity.Data;
using MyERP.Services.Identity.Services.Auth;
using MyERP.Services.Identity.Repositories;
using MyERP.Services.Identity.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyERP.Services.Identity.Services;
using MyERP.Services.Identity.Services.Users;
using MyERP.Services.Identity.Services.Roles;
using MyERP.Services.Identity.Services.Permissions;
using MyERP.Services.Identity.Middleware;
using Microsoft.AspNetCore.Authorization;
var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection Register karein
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controllers Add karein
builder.Services.AddControllers();


// 4. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key)
    };
});

// 3. Swagger (API Testing UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
// builder.Services.AddSingleton<IAuthorizationHandler, AccessControlHandler>(); // Removed in favor of Middleware
var app = builder.Build();

// 🐳 Auto Migration — Docker mein pehli baar database auto-create hoga
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 4. Pipeline Setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Custom RBAC Middleware
app.UseMiddleware<AccessControlMiddleware>();

app.MapControllers();

app.Run();