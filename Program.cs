//using dotInstrukcijeBackend.Data;
using dotInstrukcijeBackend.Repositories; // Assuming your repository classes are here
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql;
using System.Text.Json;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.ServiceInterfaces;
using dotInstrukcijeBackend.Services;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.PasswordHashingUtilities;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.JWTTokenUtility;
using dotInstrukcijeBackend.Interfaces.Service;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using dotInstrukcijeBackend.Interfaces.Repository;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Dapper configuration
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        // Database connection
        builder.Services.AddScoped<IDbConnection>(sp =>
            new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.Configure<IdentityOptions>(options => options.SignIn.RequireConfirmedEmail = true);

        // Repository and service registrations
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
        builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<IInstructorService, InstructorService>();
        builder.Services.AddScoped<ISubjectService, SubjectService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<IProfilePhotoSaver, ProfilePhotoSaver>();
        builder.Services.AddScoped<ITokenService, TokenService>();

        builder.Services.AddTransient<EmailService>();

        // Add CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Configure JSON options
        builder.Services.AddControllers()
             .AddJsonOptions(options =>
       {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
       });


        // Configure Swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "dotInstrukcijeBackend", Version = "v1" });
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            };
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                { securityScheme, new string[] { } }
            });
        });

        // Configure JWT authentication
        SetUpJWT(builder);

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void SetUpJWT(WebApplicationBuilder builder)
    {
        var accessTokenSecretKey = builder.Configuration["Jwt:AccessTokenSecretKey"];

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var key = Encoding.ASCII.GetBytes(accessTokenSecretKey);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // Adjust based on your requirements
                ValidateAudience = false, // Adjust based on your requirements
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Optional: eliminate clock skew
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Read the token from the accessToken cookie
                    var accessToken = context.HttpContext.Request.Cookies["accessToken"];
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();

        // Configure a secure cookie policy for tokens
        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = CookieSecurePolicy.SameAsRequest; // Use 'Always' if you require HTTPS
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });
    }
}
