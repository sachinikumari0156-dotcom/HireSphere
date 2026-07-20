using Microsoft.EntityFrameworkCore;
using HireSphere.API.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System;


var builder = WebApplication.CreateBuilder(args);


// Controllers + JSON cycle handling
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

// CORS - restrict via configuration (Phase 1)
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


// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    ));


// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
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


        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!
            )
        )
    };
});


// Swagger + JWT Authorization
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

            Description =
            "Enter JWT Token like: Bearer {your token}"
        });


    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type =
                        ReferenceType.SecurityScheme,

                        Id = "Bearer"
                    }
                },

                new string[] {}
            }
        });
});


var app = builder.Build();

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