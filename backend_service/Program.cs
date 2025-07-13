using backend_service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using backend_service.dal;

var builder = Host.CreateApplicationBuilder(args);

// Configure PostgreSQL connection string
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? "Host=localhost;Database=tiba_todos;Username=postgres;Password=postgres";

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repository
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddHostedService<TodoWorker>();

builder.Logging.AddConsole();

var host = builder.Build();

// Ensure database is created and migrated
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

Console.WriteLine("Starting Backend Worker Service...");
await host.RunAsync();
