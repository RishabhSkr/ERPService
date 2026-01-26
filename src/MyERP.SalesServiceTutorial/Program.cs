using Microsoft.EntityFrameworkCore;
using MyERP.SalesServiceTutorial.Data;
using MyERP.SalesServiceTutorial.Services.Customers;
using MyERP.SalesServiceTutorial.Repositories.Customers;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyERP.SalesServiceTutorial.Validators;
using MyERP.SalesServiceTutorial.Middleware;
using MyERP.SalesServiceTutorial.Clients;
using MyERP.SalesServiceTutorial.Events.Publishers;
using MyERP.SalesServiceTutorial.Events.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using MyERP.SalesServiceTutorial.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<SalesDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

builder.Services.AddScoped<ICustomersRepository,CustomerRepository>();
builder.Services.AddScoped<ICustomerService,CustomerService>();
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["InventoryService:BaseUrl"]!);
});

builder.Services.AddScoped<IEventPublisher, EventPublisher>();
builder.Services.AddHostedService<SalesOrderEventConsumer>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DynamicPermission", policy =>
    {
        policy.Requirements.Add(new PermissionRequirement());
    });
});


// // Swagger
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new() { Title = "MyERP Sales Service", Version = "v1" });
// });

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

