using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyERP.Services.Sales.Data;
using MyERP.Services.Sales.Middleware;
using MyERP.Services.Sales.Repositories.Customers;
using MyERP.Services.Sales.Repositories.SalesOrders;
using MyERP.Services.Sales.Services.Customers;
using MyERP.Services.Sales.Services.SalesOrders;
using MyERP.Services.Sales.Services.External;
using MyERP.Services.Sales.Validators;
using MyERP.Services.Sales.Authorization;
using Microsoft.AspNetCore.Authorization;
using MyERP.Services.Sales.Events.Consumers;
using MassTransit;
using MyERP.Services.Sales.Events.Producers.Publishers.MassTransit;
using MyERP.Services.Sales.HttpHandlers;


var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controllers
builder.Services.AddControllers();

// 3. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerDtoValidator>();

// 4. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyERP Sales Service", Version = "v1" });
});

// 5. JWT Authentication
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


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DynamicPermission", policy =>
    {
        policy.Requirements.Add(new PermissionRequirement());
    });
});

// 6. Register Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();

// 7. Register Services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();

// 8. Register HTTP Client for Inventory Service
var inventoryServiceUrl = builder.Configuration["Services:InventoryServiceUrl"] ?? "http://localhost:5004";
builder.Services.AddTransient<JwtDelegatingHandler>();
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(inventoryServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<JwtDelegatingHandler>();


// 9. Register Event Publisher (MassTransit)
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

// Using MassTransit to publish events
// NOTE: Sales PUBLISHES events, does NOT consume them. Production service consumes.
builder.Services.AddMassTransit(x =>
{
    // NO consumers here - Sales only publishes
    // Production service will consume SalesOrderCreatedEvent
    x.AddConsumer<BatchConcludedConsumer>()
    .Endpoint(e => e.Name = "sales-batch-concluded");

    x.AddConsumer<ProductionCancelledConsumer>()
    .Endpoint(e => e.Name = "sales-production-cancelled");

    // Configure RabbitMQ — URI-based config for CloudAMQP compatibility
    var rabbitUri = builder.Configuration["RabbitMQ:Uri"]
        ?? "amqp://guest:guest@localhost:5672/";
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitUri));
        // Auto-configure endpoints (DLQ automatic!)
        cfg.ConfigureEndpoints(context);
    });
});


// 10. CORS
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

// 🐳 Auto Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    db.Database.Migrate();
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Local dev mein custom port, Docker mein default 8080 use hota hai
if (app.Environment.IsDevelopment())
{
    app.Urls.Add("http://localhost:5002");
}

app.Run();
