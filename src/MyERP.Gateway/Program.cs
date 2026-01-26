var builder = WebApplication.CreateBuilder(args);
// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
// Map reverse proxy routes
app.MapReverseProxy();
app.Run();