using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// ================================
// RENDER PORT BINDING
// ================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"[http://0.0.0.0:{port}](http://0.0.0.0:{port})");

// ================================
// SERVICES
// ================================
builder.Services.AddControllers();

// ================================
// CONNECTION STRING
// ================================
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("DB connection loaded");

// ================================
// DATABASE (PostgreSQL)
// ================================
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseNpgsql(conn));

// ================================
// CORS (Angular + Netlify)
// ================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
        "http://localhost:4200",
        "https://adorable-travesseiro-9337e2.netlify.app"
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("AllowAngular");

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
