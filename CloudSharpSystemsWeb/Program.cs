using APIConnector.Model;
using AuxiliaryClassLibrary.ExceptionHelper;
using CloudSharpSystemsWeb.CustomMiddleWares;
using DBConnectionLibrary;
using DBConnectionLibrary.DBQueryContexts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
// Add json for query validator:
builder.Configuration.AddJsonFile("dynamic-query-config.json", optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile("gcp_service_account_secrets.json", optional: true, reloadOnChange: false);
builder.Configuration
    .AddJsonFile("gcp_credentials_client_secrets.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"gcp_credentials_client_secrets.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);


// Add services to the container.
// Shu-Yuan Yang 20240513 added log in options
//builder.Services.AddAuthentication().AddGoogle(googleOptions =>
//{
//    googleOptions.ClientId = "466344244859-uidehqsrjah6ae0dse457vrdqbc0eovf.apps.googleusercontent.com"; // configuration["Authentication:Google:ClientId"];
//    googleOptions.ClientSecret = "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"; //configuration["Authentication:Google:ClientSecret"];
//});



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
/*builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("3.132.169.204")); // <<-- Notice the full format of the IP address.
});*/

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add service for global exception handling middleware:
builder.Services.AddTransient<ExceptionMiddleware>();


// Add Configuration service so the controllers can get settings from the appsettings file
//builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Link Entity Framework Core DB Context to establish reference to the App database and storage:
builder.Services.Configure<GCPServiceAccountSecretKeyObject>(builder.Configuration); // get storage service account credential config
builder.Services.Configure<GCPOAuth2ClientSecretKeyObject>(builder.Configuration); // get Google credentials client secret config
builder.Services.Configure<DynamicQueryConfig>(builder.Configuration); // get queryable fields config
builder.Services.AddDbContext<AppDBMainContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseAppMainConnectionString")));

// Link Mongo DB Context to establish reference to the App Mongo database and:
builder.Services.AddSingleton<AppDBMongoContext, AppDBMongoContext>(s => new AppDBMongoContext(builder.Configuration.GetConnectionString("DatabaseMongoConnectionString")!));


// Add CORS method
// Reference: https://www.yogihosting.com/aspnet-core-enable-cors/
builder.Services.AddCors();

// Allow Amazon AWS Lambda Hosting:
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);


// 20240513 Enable Session Management:
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});




var app = builder.Build();





var forwardingOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardingOptions.KnownNetworks.Clear(); // Loopback by default, this should be temporary
forwardingOptions.KnownProxies.Clear(); // Update to include
// Use forwarded headers
app.UseForwardedHeaders(forwardingOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
});

app.UseAuthorization();

app.UseSession();

// Shu-Yuan Yang 20240513 redirect middleware:
app.UseMiddleware<RedirectMiddleWare>();

// use global exception handling middleware:
app.UseMiddleware<ExceptionMiddleware>();


app.MapControllers();

app.Run();
