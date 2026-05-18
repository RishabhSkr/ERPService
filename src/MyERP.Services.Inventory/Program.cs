using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyERP.Services.Inventory.Data;
using MyERP.Services.Inventory.Middleware;
using MyERP.Services.Inventory.Repositories.Categories;
using MyERP.Services.Inventory.Repositories.Units;
using MyERP.Services.Inventory.Repositories.Products;
using MyERP.Services.Inventory.Repositories.RawMaterials;
using MyERP.Services.Inventory.Repositories.Warehouses;
using MyERP.Services.Inventory.Repositories.Inventory;
using MyERP.Services.Inventory.Services.Categories;
using MyERP.Services.Inventory.Services.Units;
using MyERP.Services.Inventory.Services.Products;
using MyERP.Services.Inventory.Services.RawMaterials;
using MyERP.Services.Inventory.Services.StockMovements;
using MyERP.Services.Inventory.Services.Warehouses;
using MyERP.Services.Inventory.Validators;
using MassTransit;
using MyERP.Services.Inventory.Events.Consumers;
using MyERP.Services.Inventory.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    // Register all consumers with explicit endpoint names
    x.AddConsumer<MaterialReservationRequestedConsumer>();
    x.AddConsumer<MaterialReturnRequestedConsumer>();
    x.AddConsumer<BatchConcludedConsumer>();
    
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
        
        Console.WriteLine("🔧 [MassTransit] All endpoints Inventory Service configured");
    });
});



// 1. Database Connection
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controllers
builder.Services.AddControllers();

// 3. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();

// 4. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyERP Inventory Service", Version = "v1" });
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
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DynamicPermission", policy =>
    {
        policy.Requirements.Add(new PermissionRequirement());
    });
});
// 6. Register Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IStorageLocationTypeRepository, StorageLocationTypeRepository>();
builder.Services.AddScoped<IStorageLocationRepository, StorageLocationRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();

// 7. Register Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IStorageLocationTypeService, StorageLocationTypeService>();
builder.Services.AddScoped<IStorageLocationService, StorageLocationService>();

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

// 🐳 Auto Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
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
    app.Urls.Add("http://localhost:5004");
}

app.Run();
