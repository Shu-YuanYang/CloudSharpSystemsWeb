using AuxiliaryClassLibrary.ExceptionHelper;
using CustomMiddleWares;
using DBConnectionLibrary;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add controller service
// Also use Pascal Case:
// Reference: https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-7.0 See section "Configure System.Text.Json-based formatters"
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Use forwarded headers
// Reference: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-5.0
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add service for global exception handling middleware:
builder.Services.AddTransient<ExceptionMiddleware>();

// Link Entity Framework Core DB Context to establish reference to the App database:
builder.Services.AddDbContext<AppDBMainContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseAppMainConnectionString")));

// Add CORS method
// Reference: https://www.yogihosting.com/aspnet-core-enable-cors/
builder.Services.AddCors();



var app = builder.Build();


// Use forwarded headers
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Shows UseCors with CorsPolicyBuilder.
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

app.UseMiddleware<ReverseProxyMiddleware>();

// use global exception handling middleware:
app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

app.MapControllers();

app.Run();
