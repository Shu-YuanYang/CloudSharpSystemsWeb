using APIConnector.Model;
using AuxiliaryClassLibrary.ExceptionHelper;
using CloudSharpLimitedCentral.CustomMiddleWares;
using CustomMiddleWares;
using DBConnectionLibrary;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("gcp_credentials_client_secrets.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"gcp_credentials_client_secrets.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);


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


builder.Services.Configure<GCPOAuth2ClientSecretKeyObject>(builder.Configuration); // get Google credentials client secret config
// Link Entity Framework Core DB Context to establish reference to the App database:
builder.Services.AddDbContext<AppDBMainContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseAppMainConnectionString")));

// Add CORS method
// Reference: https://www.yogihosting.com/aspnet-core-enable-cors/
builder.Services.AddCors();


// 20240705 Enable Session Management:
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


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
var javascript_origins = builder.Configuration.GetSection("javascript_origins").Get<string[]>()!;
app.UseCors(builder =>
{
    builder.WithOrigins(javascript_origins)
            //.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
});

app.UseSession();

// Shu-Yuan Yang 20240513 redirect middleware:
app.UseMiddleware<RedirectMiddleWare>();

app.UseMiddleware<ReverseProxyMiddleware>();

// use global exception handling middleware:
app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

app.MapControllers();

app.Run();
