using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;

var builder = WebApplication.CreateBuilder(args);


// Render PORT BINDING

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


// CONTROLLERS

builder.Services.AddControllers();


// CONNECTION STRING (SAFE HANDLING)

var conn = builder.Configuration.GetConnectionString("Default");

// DEBUG (will show in Render logs)
Console.WriteLine("DB CONNECTION STRING: " + conn);

// FAIL FAST if missing (VERY IMPORTANT for Render debugging)
if (string.IsNullOrWhiteSpace(conn))
{
    throw new Exception(" Connection string 'Default' is missing. Check Render Environment Variables.");
}

// ================================
// DATABASE (PostgreSQL - Supabase)
// ================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn));

// ================================
// CORS (Angular frontend)
// ================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://adorable-travesseiro-9337e2.netlify.app" // your Angular production URL
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
app.UseSwagger();
app.UseSwaggerUI();

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