
using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Services;
using WorkSessionTrackerAPI.Repositories;
using Microsoft.AspNetCore.Identity; // Add this using statement
using WorkSessionTrackerAPI.Models; // Add this using statement for your User model
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WorkSessionTrackerAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using WorkSessionTrackerAPI.Extensions;
using WorkSessionTrackerAPI.Models.Enums;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up the application");

var builder = WebApplication.CreateBuilder(args);

// Replace the default logging with Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers(); // Add this line to enable controllers
builder.Services.AddSwaggerGen(options =>
{
    // Add a security definition for JWT Bearer tokens
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = true; // Enforce email verification
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>() // Use Entity Framework Core as the store
    .AddDefaultTokenProviders(); // Add token providers for email confirmation, password reset, etc.

// Register Repositories
// Register Services (Keep only non-user-specific services/repositories)
builder.Services.AddScoped<IWorkSessionRepository, WorkSessionRepository>();
builder.Services.AddScoped<IUserService, UserService>(); // Re-added: The refactored UserService is still needed for custom business logic
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWorkSessionService, WorkSessionService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();

// Register Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, StudentDataAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, WorkSessionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CommentAuthorizationHandler>();

// This is needed for handlers that use IHttpContextAccessor to get route data
// Note: Not strictly needed for the current implementation as we pass resources directly,
// but good practice to have if handlers evolve.
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
    {
        // Force the system to use JWT for everything, overriding AddIdentity's Cookies
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.")))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Simple role-based policies
    options.AddPolicy(Policies.StudentOnly, policy => policy.RequireRole(UserRoleEnum.Student.ToString()));
    options.AddPolicy(Policies.CompanyOnly, policy => policy.RequireRole(UserRoleEnum.Company.ToString()));

    // Resource-based policies
    options.AddPolicy(Policies.CanAccessStudentData, policy => policy.AddRequirements(new StudentDataRequirement()));
    options.AddPolicy(Policies.IsWorkSessionOwner, policy => policy.AddRequirements(new IsWorkSessionOwnerRequirement()));
    options.AddPolicy(Policies.CanVerifyWorkSession, policy => policy.AddRequirements(new CanVerifyWorkSessionRequirement()));
    options.AddPolicy(Policies.CanCommentOnWorkSession, policy => policy.AddRequirements(new CanCommentOnWorkSessionRequirement()));
    options.AddPolicy(Policies.CanViewComment, policy => policy.AddRequirements(new CanViewCommentRequirement()));
    options.AddPolicy(Policies.IsCommentOwner, policy => policy.AddRequirements(new IsCommentOwnerRequirement()));
}); // Add authorization services

var app = builder.Build();

// Seed the database with roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Initializing database and seeding data...");
    await DataSeeder.InitializeAsync(services);
}

// Register the custom exception handling middleware at the top of the pipeline.
app.UseExceptionMiddleware();

// Add Serilog request logging. This will log every incoming HTTP request.
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enables the Swagger middleware
    app.UseSwaggerUI(c =>
    {
        c.EnablePersistAuthorization();
    }); // Enables the Swagger UI middleware
    
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();  // Must be after UseAuthentication
app.MapControllers(); // Map controller routes

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
