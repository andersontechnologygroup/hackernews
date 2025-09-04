using HackerNews.Api.Library.Configuration;
using HackerNews.Api.Library.Interfaces;
using HackerNews.Api.Library.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Runtime;
using System.Text;

// There are no unit tests for the Program.   The builders should 
// just work or you will get immediate errors and not even be able
// to run the app at all.  Therefore, let's shut down Stryker completely

// Stryker disable all

var builder = WebApplication.CreateBuilder(args);

// Load configurations from the appsettings
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Make sure the appsettings sectino exists.
if (!builder.Configuration.GetSection("HackerNewsServiceSettings").Exists()) throw new InvalidOperationException("HackerNewssServiceSettings section is missing.");

var bindHackerNewsServiceSettings = new HackerNewsServiceSettings();
builder.Configuration.Bind("HackerNewsServiceSettings", bindHackerNewsServiceSettings);

if (bindHackerNewsServiceSettings.CacheKey == null) throw new InvalidOperationException("CacheKey is not configured in appsettings.json");
if (bindHackerNewsServiceSettings.CacheTimeoutInMinutes == null) throw new InvalidOperationException("CacheTimeoutInMinutes is not configured in appsettings.json");
if (bindHackerNewsServiceSettings.NewStoriesJSONPath == null) throw new InvalidOperationException("NewStoriesJSONPath is not configured in appsettings.json");
if (bindHackerNewsServiceSettings.ItemJSONPath == null) throw new InvalidOperationException("ItemJSONPath is not configured in appsettings.json");
if (bindHackerNewsServiceSettings.NumberOfStoriesToPull == null) throw new InvalidOperationException("NumberOfStoriesToPull is not configured in appsettings.json");

// Define a CORS Policy
const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          // For development, you can allow the default Angular port.
                          // For production, you would list your specific domain(s).
                          
                          // Since we aren't doing authentication just yet, we don't really mind it being open
                          policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                      });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add MemoryCache for caching service
builder.Services.AddMemoryCache();

// Add the appsettings option so that it gets injected into controllers.
builder.Services.Configure<HackerNewsServiceSettings>(builder.Configuration.GetSection("HackerNewsServiceSettings"));

// Configure a typed HttpClient for our HackerNewsService
var hackerNewsApiBaseUrl = builder.Configuration.GetValue<string>("HackerNewsApi:BaseUrl");
if (string.IsNullOrEmpty(hackerNewsApiBaseUrl))
{
    throw new InvalidOperationException("Hacker News API Base URL is not configured in appsettings.json.");
}

builder.Services.AddHttpClient<IHackerNewsService, HackerNewsService>(client =>
{
    client.BaseAddress = new Uri(hackerNewsApiBaseUrl);
});

// Add JWT Authentication
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured.")))
    };
});

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// App Pipeline Config
var app = builder.Build();

// Configure the HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// The order is important here!
// Middleware is processed in the order it is added.

// Add the CORS middleware first
// We want to make sure CORS comes before anything so it will
// get rejected before we even try to process other stuff.
app.UseCors(myAllowSpecificOrigins);

// UseAuthentication must come before UseAuthorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Stryker restore all

public partial class Program { }
