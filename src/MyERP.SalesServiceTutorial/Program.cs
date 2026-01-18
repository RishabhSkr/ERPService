using Microsoft.EntityFrameworkCore;
using MyERP.SalesServiceTutorial.Data;
using MyERP.SalesServiceTutorial.Services.Customers;
using MyERP.SalesServiceTutorial.Repositories.Customers;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyERP.SalesServiceTutorial.Validators;
using MyERP.SalesServiceTutorial.Middleware;

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
app.MapControllers();

app.Run();

