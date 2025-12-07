
using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Data;
using WorkSessionTrackerAPI.DTOs; // Added for clarity, though not strictly needed here
using WorkSessionTrackerAPI.Interfaces;
using WorkSessionTrackerAPI.Services;
using WorkSessionTrackerAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers(); // Add this line to enable controllers
builder.Services.AddSwaggerGen();

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
// Register Services
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Already present, ensure it's here
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enables the Swagger middleware
    app.UseSwaggerUI(); // Enables the Swagger UI middleware
    // You can optionally configure the Swagger UI endpoint, e.g., app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
    // If you were using app.MapOpenApi(), you would remove it here.
}

app.UseHttpsRedirection();

app.MapControllers(); // Map controller routes

app.Run();