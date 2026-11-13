using System.Text.Json.Serialization;
using BeatTheRocky.Api.Factories;
using BeatTheRocky.Api.Repositories;
using BeatTheRocky.Api.Services;
using BeatTheRocky.Api.Strategies;

var builder = WebApplication.CreateBuilder(args);

// Dynamically bind to cloud provider port (Render, Railway, Fly.io) or default to 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

// Register Core & Domain Services (Dependency Injection)
builder.Services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
builder.Services.AddSingleton<IAiStrategy, UnbeatableMinimaxStrategy>();
builder.Services.AddSingleton<IGameFactory, GameFactory>();
builder.Services.AddScoped<IGameService, GameService>();

// Configure Controllers with String Enum Serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Enable CORS for flexibility (supporting local dev, Vercel, and Render deployments)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Beat The Rocky - Tic Tac Toe API", Version = "v1" });
});

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve Frontend from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// Health check endpoint for Render / cloud monitoring
app.MapGet("/health", () => Results.Ok(new { status = "healthy", game = "Beat The Rocky", timestamp = DateTime.UtcNow }));

app.Run();
