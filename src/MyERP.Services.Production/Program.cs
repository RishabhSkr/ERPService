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
using MyERP.Services.Production.Services.MRP;
using MyERP.Services.Production.Repositories.Process;
using MyERP.Services.Production.Repositories.WorkCenter;
using MyERP.Services.Production.Repositories.Equipment;
using MyERP.Services.Production.Repositories.ProcessRoute;
using MyERP.Services.Production.Repositories.WorkOrder;
using MyERP.Services.Production.Services.Process;
using MyERP.Services.Production.Services.WorkCenter;
using MyERP.Services.Production.Services.Equipment;
using MyERP.Services.Production.Services.ProcessRoute;
using MyERP.Services.Production.Services.WorkOrder;
using MyERP.Services.Production.Authorization;
using Microsoft.AspNetCore.Authorization;
using MyERP.Services.Production.HttpHandlers;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1. DATABASE CONFIGURATION
// ============================================================================
// 📝 Industry Practice: Always use configuration, never hardcode
builder.Services.AddDbContext<ProductionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
var inventoryServiceUrl = builder.Configuration["Services:InventoryServiceUrl"] 
    ?? "http://localhost:5004";
builder.Services.AddTransient<JwtDelegatingHandler>();
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(inventoryServiceUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<JwtDelegatingHandler>();

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
builder.Services.AddScoped<IMRPService, MRPService>();

// Work Orders & Routing — Repositories
builder.Services.AddScoped<IProcessRepository, ProcessRepository>();
builder.Services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IProcessRouteRepository, ProcessRouteRepository>();
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

// Work Orders & Routing — Services
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<IWorkCenterService, WorkCenterService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IProcessRouteService, ProcessRouteService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();

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
    x.AddConsumer<StockReleasedConsumer>();   // SAGA return: decrement QuantityReserved on WO cancel
    x.AddConsumer<SalesOrderCancelledConsumer>()
    .Endpoint(e => e.Name = "sales-order-cancelled");
    
    // Configure RabbitMQ — URI-based config for CloudAMQP compatibility
    var rabbitUri = builder.Configuration["RabbitMQ:Uri"]
        ?? "amqp://guest:guest@localhost:5672/";
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitUri));

        // Retry policy — handles cold starts & transient failures
        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10)
        ));

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

// 🐳 Auto Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductionDbContext>();
    db.Database.Migrate();
}

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

// Health check endpoint — keeps Render service awake via external pinger
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "production", time = DateTime.UtcNow }));

// ============================================================================
// SET PORT AND RUN
// ============================================================================
// 📝 Port allocation: 
//    5001=Identity, 5002=Sales, 5003=SalesTutorial, 5004=Inventory, 5006=Production
// Local dev mein custom port, Docker mein default 8080 use hota hai
if (app.Environment.IsDevelopment())
{
    app.Urls.Add("http://localhost:5006");
}

app.Run();
