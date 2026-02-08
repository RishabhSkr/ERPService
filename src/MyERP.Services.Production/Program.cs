/*
 * ============================================================================
 * MyERP Production Service - Program.cs
 * ============================================================================
 * 
 * 📚 INDUSTRY vs NOOB Approach:
 * 
 * ❌ NOOB WAY:
 *    - Add services randomly, no organization
 *    - Hardcode connection strings
 *    - No comments, no sections
 *    - Copy-paste from tutorials without understanding
 * 
 * ✅ INDUSTRY WAY (What we do here):
 *    1. Organize services in numbered sections (easy to navigate)
 *    2. Configuration from appsettings.json (no hardcoding)
 *    3. Clear separation: Auth > DI > Middleware pipeline
 *    4. Comments explaining WHY, not just WHAT
 *    5. Extension methods for complex registrations (keeps Program.cs clean)
 * 
 * ============================================================================
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Middleware;
using MyERP.Services.Production.Repositories.BOM;
using MyERP.Services.Production.Repositories.PendingRequests;
using MyERP.Services.Production.Repositories.ProductionOrders;
using MyERP.Services.Production.Services.BOM;
using MyERP.Services.Production.Services.PendingRequests;
using MyERP.Services.Production.Services.ProductionOrders;
using MyERP.Services.Production.Events.Consumers;
using MyERP.Services.Production.Events.Publishers;
using MyERP.Services.Production.Validators;
using MyERP.Services.Production.Services.External;
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1. DATABASE CONFIGURATION
// ============================================================================
// 📝 Industry Practice: Always use configuration, never hardcode
builder.Services.AddDbContext<ProductionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var inventoryServiceUrl = builder.Configuration["Services:InventoryServiceUrl"] 
    ?? "http://localhost:5004";
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(inventoryServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ============================================================================
// 2. CONTROLLERS + API DOCUMENTATION
// ============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyERP Production Service", Version = "v1" });
});

// ============================================================================
// 3. VALIDATION (FluentValidation)
// ============================================================================
// 📝 Industry Practice: Auto-register all validators from assembly
//    Noob approach: Manually register each validator
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBOMDtoValidator>();

// ============================================================================
// 4. JWT AUTHENTICATION
// ============================================================================
// 📝 Industry Practice: Same JWT settings across all microservices
//    This allows tokens from Identity Service to work everywhere
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

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
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// ============================================================================
// 5. DEPENDENCY INJECTION - REPOSITORIES
// ============================================================================
// 📝 Industry Practice: Interface → Implementation pattern
//    Allows easy mocking for unit tests, swapping implementations
//    Noob approach: Use concrete classes directly (hard to test)

builder.Services.AddScoped<IBOMRepository, BOMRepository>();
builder.Services.AddScoped<IPendingRequestRepository, PendingRequestRepository>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();

// ============================================================================
// 6. DEPENDENCY INJECTION - SERVICES (Business Logic)
// ============================================================================
// 📝 Industry Practice: Thin controllers, fat services
//    Controllers only handle HTTP, Services contain business logic
//    This makes logic reusable across controllers, background jobs, etc.

builder.Services.AddScoped<IBOMService, BOMService>();
builder.Services.AddScoped<IPendingRequestService, PendingRequestService>();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();

// ============================================================================
// 7. EVENT PUBLISHER
// ============================================================================
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

// ============================================================================
// 8. MASSTRANSIT - RABBITMQ CONFIGURATION
// ============================================================================
//  Industry Practice: 
//    - Use MassTransit abstraction over raw RabbitMQ
//    - Automatic retry, DLQ (Dead Letter Queue), serialization
//    Noob approach: Use RabbitMQ client directly (more code, more bugs)

builder.Services.AddMassTransit(x =>
{
    // Register all consumers with explicit endpoint names
    x.AddConsumer<SalesOrderCreatedConsumer>()
        .Endpoint(e => e.Name = "sales-order-created");  // Explicit queue name
    
    x.AddConsumer<StockReservedConsumer>();
    
    // Configure RabbitMQ
    var rabbitHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:UserName"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        
        // Configure all endpoints (including our explicit "sales-order-created")
        cfg.ConfigureEndpoints(context);
        
        Console.WriteLine("🔧 [MassTransit] All endpoints configured");
    });
});






// ============================================================================
// 9. CORS CONFIGURATION
// ============================================================================
// 📝 Note: In production, restrict origins. AllowAll is for development only.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================================
// BUILD APP
// ============================================================================
var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE (Order matters!)
// ============================================================================
// 📝 Industry Practice: Middleware order is CRITICAL
//    1. Exception handling (catches all errors)
//    2. CORS
//    3. Authentication (who are you?)
//    4. Authorization (what can you do?)
//    5. Controllers

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionMiddleware>();  // Custom error handling
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ============================================================================
// SET PORT AND RUN
// ============================================================================
// 📝 Port allocation: 
//    5001=Identity, 5002=Sales, 5003=SalesTutorial, 5004=Inventory, 5006=Production
app.Urls.Add("http://localhost:5006");

app.Run();
