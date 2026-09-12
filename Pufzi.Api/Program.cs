using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Pufzi.Api.Middleware;
using Pufzi.Data.Database;
using Pufzi.Infrastructure.Authentication;
using Pufzi.Infrastructure.Email;
using Pufzi.Infrastructure.Storage;
using Pufzi.Services.Auth;
using Pufzi.Services.Businesses;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "FrontendCorsPolicy";

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<PufziDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Introdu access token-ul JWT."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });

    var apiXmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            "Pufzi.Api.xml");

    options.IncludeXmlComments(apiXmlPath);

    var contractsXmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            "Pufzi.Contracts.xml");

    options.IncludeXmlComments(contractsXmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration was not found.");

builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher>();

builder.Services.AddSingleton<
    ISecureTokenGenerator,
    SecureTokenGenerator>();

builder.Services.AddScoped<
    IJwtService,
    JwtService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SigningKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType =
                    JwtRegisteredClaimNames.Email,

                RoleClaimType =
                    "platform_role"
            };
    });

builder.Services.Configure<BrevoOptions>(
    builder.Configuration.GetSection(
        BrevoOptions.SectionName));

builder.Services.AddHttpClient<
    IEmailService,
    BrevoEmailService>(
        client =>
        {
            client.BaseAddress =
                new Uri("https://api.brevo.com/");
        });

builder.Services.Configure<AzureBlobStorageOptions>(
    builder.Configuration.GetSection(
        AzureBlobStorageOptions.SectionName));

builder.Services.AddSingleton<
    IBlobStorageService,
    AzureBlobStorageService>();

builder.Services.AddScoped<
    IBusinessService,
    BusinessService>();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();