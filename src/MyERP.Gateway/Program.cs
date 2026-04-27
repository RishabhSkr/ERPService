using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using System.Text;

// Enable full error details (ONLY for debugging!)
IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);



// 1. JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"]!;
Console.WriteLine($"🔑 Gateway JWT Key (first 10 chars): {jwtKey.Substring(0, Math.Min(10, jwtKey.Length))}...");
Console.WriteLine($"🔑 Gateway JWT Key Length: {jwtKey.Length}");

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
    
    // Log token validation errors for debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ Auth Failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"✅ Token validated for: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

// 2. Authorization with Default policy
builder.Services.AddAuthorization(options =>
{
    // Default policy - requires authenticated user
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// 3. Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    
// 8. CORS (for frontend access)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");

// 4. Use Authentication & Authorization BEFORE YARP
app.UseAuthentication();
app.UseAuthorization();

// 5. Map reverse proxy routes
app.MapReverseProxy();



app.Run();