using Tiba.Shared.Model;
using Tiba.Rest.Services;
using System.Text;
using Tiba.Rest.Exceptions;
using Tiba.Rest.Extentions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddApplicationServices()
    .AddJwtAuthentication()
    .AddAuthorization()
    .AddExceptionHandling()
    .AddHealthCheck();

var app = builder.Build()
    .AddGenericExceptionHandling();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapGet("/todos", async (ITodoService todoService, IAuthService authService, HttpContext context) =>
{

    // Use the authService to get the user ID from the context
    var userId = authService.TryGetUserIdFromAuth(context);
    if (userId <= 0)
    {
        return Results.Unauthorized();
    }
    var todos = await todoService.GetTodosByUserIdAsync(userId);
    return Results.Ok(todos);
})
.WithName("GetTodos")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/todos", async (Todo todo, ITodoService todoService, IAuthService authService, HttpContext context) =>
{
    // Use the authService to get the user ID from the context
    var userId = authService.TryGetUserIdFromAuth(context);
    if (userId <= 0)
    {
        return Results.Unauthorized();
    }
    todo.UserId = userId;
    var createdTodo = await todoService.CreateTodoAsync(todo);
    return Results.Created($"/todos/{createdTodo.Id}", createdTodo);
})
.WithName("InsertTodo")
.WithOpenApi()
.RequireAuthorization();

app.Run();