using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// ================================
// RENDER PORT BINDING
// ================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ================================
// SERVICES
// ================================
builder.Services.AddControllers();

// ================================
// CONNECTION STRING
// ================================
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(conn))
{
    throw new Exception("Database connection string not found.");
}

Console.WriteLine("DB connection loaded");

// ================================
// DATABASE (PostgreSQL)
// ================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn));

// ================================
// CORS
// ================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://expencebook.netlify.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// ================================
// SWAGGER
// ================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================================
// MIDDLEWARE
// ================================
app.UseRouting();

app.UseCors("AllowAngular");

// Swagger enabled for production also
app.UseSwagger();
app.UseSwaggerUI();

// Authorization
app.UseAuthorization();

// ================================
// TEST ROUTE
// ================================
app.MapGet("/", () => "ExpenseTracker API is running!");

// ================================
// CONTROLLERS
// ================================
app.MapControllers();

app.Run();