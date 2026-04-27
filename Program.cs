using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TcgApi.Data;
using TcgApi.Data.Repositories;
using TcgApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularUI", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://kind-flower-0164c1e1e.7.azurestaticapps.net", "https://pindorama.cc")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    var key = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key is not configured.");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<CollectionRepository>();
builder.Services.AddScoped<BoosterPackRepository>();
builder.Services.AddScoped<WaitlistRepository>();
builder.Services.AddScoped<DailyTaskRepository>();

var app = builder.Build();

app.UseExceptionHandler("/error");
app.UseCors("AngularUI");
app.UseAuthentication();
app.UseAuthorization();

app.Map("/error", (HttpContext context, ILogger<Program> logger) =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    if (feature?.Error is { } ex)
    {
        logger.LogError(ex, "Unhandled exception on {Path}", feature.Path);
    }

    return Results.Problem(
        title: "Internal Server Error",
        detail: feature?.Error?.Message,
        statusCode: StatusCodes.Status500InternalServerError);
});

app.MapOpenApi();
app.MapScalarApiReference();
app.MapEndpoints();

await app.RunAsync();
