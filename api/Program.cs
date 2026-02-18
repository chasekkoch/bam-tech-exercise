using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OpenSearch.Client;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Health;
using StargateAPI.Logging;
using StargateAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<StargateContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

var openSearchOptions = builder.Configuration.GetSection("OpenSearch").Get<OpenSearchOptions>() ?? new OpenSearchOptions();
builder.Services.AddSingleton(openSearchOptions);

builder.Services.AddSingleton<IOpenSearchClient>(_ =>
{
    var settings = new ConnectionSettings(new Uri(openSearchOptions.Uri));

    if (!string.IsNullOrWhiteSpace(openSearchOptions.Username))
    {
        settings = settings.BasicAuthentication(openSearchOptions.Username, openSearchOptions.Password);
    }

    return new OpenSearchClient(settings);
});

builder.Services.AddSingleton<ILogService>(sp =>
{
    if (!openSearchOptions.Enabled)
    {
        return new NoOpLogService();
    }

    var client = sp.GetRequiredService<IOpenSearchClient>();
    return new OpenSearchLogService(client, openSearchOptions);
});

builder.Services.AddTransient<RequestLoggingMiddleware>();

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

// Configure health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<StargateContext>(
        name: "database",
        tags: new[] { "ready", "db" })
    .AddCheck<OpenSearchHealthCheck>(
        name: "opensearch",
        tags: new[] { "ready", "opensearch" });

// Register OpenSearchHealthCheck
builder.Services.AddSingleton<OpenSearchHealthCheck>(sp =>
{
    var client = sp.GetRequiredService<IOpenSearchClient>();
    return new OpenSearchHealthCheck(client, openSearchOptions.Enabled);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Note: HTTPS redirection disabled for container-to-container communication
// External HTTPS is handled by the nginx reverse proxy in production

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database - apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
    try
    {
        // Apply any pending migrations (includes seed data via HasData)
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log but don't fail startup if migration fails
        Console.WriteLine($"Warning: Database migration failed: {ex.Message}");
    }
}

// Health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Only checks API is running
});

app.Run();


