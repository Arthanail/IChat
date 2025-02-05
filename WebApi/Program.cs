using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Serilog;
using WebApi.Database;
using WebApi.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var appConfiguration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddScoped<MessageService>();
    builder.Services.AddSingleton<ChatService>();

    builder.Host.SerilogConfiguration();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    var webSocketOptions = new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromMinutes(2)
    };

    app.UseWebSockets(webSocketOptions);

    var connectionString = appConfiguration.GetConnectionString("DefaultConnection");
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    var dataSource = dataSourceBuilder.Build();
    var connection = await dataSource.OpenConnectionAsync();

    DbInitialize.Initialize(connection);

    app.MapGet("/ws", async (HttpContext context, ChatService chatService) =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await chatService.HandleWebSocketConnection(webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Expected a WebSocket request");
        }
    });

    app.MapPost("/messages",
            async ([FromServices] MessageService messageService, [FromBody] GetMessagesRequest request) =>
            {
                var result = await messageService.GetMessages(request, connection);

                return result.Count > 0 ? Results.Ok(result) : Results.NotFound("No data found");
            })
        .WithName("GetMessages")
        .WithOpenApi();

    app.MapPost("/message",
            async ([FromServices] MessageService messageService, [FromBody] SaveMessageRequest request) =>
            {
                var result = 0;
                try
                {
                    Log.Information("Saved message: {@Request}", request);
                    result = await messageService.SaveMessage(request, connection);
                }
                catch (Exception e)
                {
                    if (e.Message == "duplicate key value violates unique constraint \"messages_pkey\"")
                    {
                        result = 0;
                    }
                }

                return result > 0
                    ? Results.Ok("Message sent successfully!")
                    : Results.NotFound("Message with this Serial Number exists");
            })
        .WithName("PostMessage")
        .WithOpenApi();

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}