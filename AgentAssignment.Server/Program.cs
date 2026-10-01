
using AgentAssignment.Server.Contracts;

namespace AgentAssignment.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        var port = builder.Configuration.GetValue<int?>("Server:Port") ?? 5080;
        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(port));

        var modelAlias = builder.Configuration["Server:ModelAlias"]
             ?? FoundryLocalChatCompletionService.DefaultAlias;
        builder.Services.AddSingleton<IChatCompletionService>(
            _ => new FoundryLocalChatCompletionService(modelAlias));

        // Add services to the container.

        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        var chatCompletionService = app.Services
                    .GetRequiredService<IChatCompletionService>();

        if (chatCompletionService is FoundryLocalChatCompletionService foundryService)
        {
            _ = foundryService.EnsureStartedAsync();
        }

        app.MapControllers();

        app.Run();
    }

}
