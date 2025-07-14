using Tiba.Shared.Model;
using Tiba.Rest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tiba.Rest.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using Tiba.Rest.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
builder.Services.AddSingleton<ITodoService, TodoService>();
builder.Services.AddSingleton<IAuthService, MockAuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //TODO: this is a mock implementation, replace with actual JWT validation logic
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            RequireExpirationTime = false,
            SignatureValidator = (token, parameters) =>
            {
                // Custom signature validation logic can be added here if needed
                return new JsonWebToken(token);
            },
            ClockSkew = TimeSpan.Zero // Remove default 5-minute tolerance for token expiration timing
        };
    });

builder.Services
    .AddAuthorization()
    .AddExceptionHandling();

var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new { message = "An internal server error occurred." };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

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