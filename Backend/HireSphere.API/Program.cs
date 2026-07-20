using System.Text;
using System.Text.Json.Serialization;
using HireSphere.API.Data;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IResourceAuthorizationService, ResourceAuthorizationService>();
builder.Services.AddScoped<ILocalFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<ICandidateProfileService, CandidateProfileService>();
builder.Services.AddScoped<IJobMatchingProvider, DeterministicJobMatchingProvider>();
builder.Services.AddScoped<ICandidateJobService, CandidateJobService>();
builder.Services.AddScoped<ICandidateApplicationService, CandidateApplicationService>();
builder.Services.AddScoped<ICandidateAssessmentService, CandidateAssessmentService>();
builder.Services.AddScoped<ICandidateInterviewService, CandidateInterviewService>();
builder.Services.AddScoped<ICandidateNotificationService, CandidateNotificationService>();
builder.Services.AddScoped<INotificationWriter, NotificationWriter>();
builder.Services.AddScoped<IJobStatusTransitionService, JobStatusTransitionService>();
builder.Services.AddScoped<IApplicationStatusTransitionService, ApplicationStatusTransitionService>();
builder.Services.AddScoped<IRecruiterPortalService, RecruiterPortalService>();
builder.Services.AddScoped<IRecruiterPhase52Service, RecruiterPhase52Service>();
builder.Services.AddScoped<IRecruiterPhase53Service, RecruiterPhase53Service>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        policy
            .SetIsOriginAllowed(origin =>
                allowedOrigins.Contains(
                    origin,
                    StringComparer.OrdinalIgnoreCase))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        RoleClaimType = "role",
        NameClaimType = "uid",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CandidateOnly", policy => policy.RequireRole("Candidate"));
    options.AddPolicy("RecruiterOnly", policy => policy.RequireRole("Recruiter"));
    options.AddPolicy("HiringManagerOnly", policy => policy.RequireRole("HiringManager"));
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RecruiterOrAdministrator", policy => policy.RequireRole("Recruiter", "Admin"));
    options.AddPolicy("HiringManagerOrAdministrator", policy => policy.RequireRole("HiringManager", "Admin"));
    options.AddPolicy("RecruitmentTeam", policy => policy.RequireRole("Recruiter", "HiringManager", "Admin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT Token like: Bearer {your token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (!app.Environment.IsEnvironment("Testing"))
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DbSeeder");
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (await db.Database.CanConnectAsync())
            {
                await HireSphere.API.Data.Seed.DbSeeder.SeedAsync(
                    db,
                    app.Configuration,
                    logger);
            }
            else
            {
                logger.LogWarning(
                    "Database is not reachable. Skipping seed. Apply migrations and configure ConnectionStrings__DefaultConnection.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Database seed skipped because the database is unavailable or not migrated yet.");
        }
    }
}

// Standard exception handler (sanitized response)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected error occurred.",
            traceId = context.TraceIdentifier
        });
    });
});


// Swagger Middleware
app.UseSwagger();

app.UseSwaggerUI();


// HTTPS
app.UseHttpsRedirection();


// CORS Middleware
app.UseCors("AllowFrontend");


// Authentication + Authorization
app.UseAuthentication();

app.UseAuthorization();


// API Controllers
app.MapControllers();


app.Run();

public partial class Program { }
