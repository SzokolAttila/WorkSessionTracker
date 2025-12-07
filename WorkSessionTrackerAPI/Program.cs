
using Microsoft.EntityFrameworkCore;
using WorkSessionTrackerAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer(); // Required for Swashbuckle to discover endpoints
builder.Services.AddSwaggerGen(); // Adds Swagger generation services

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.Run();