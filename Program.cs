using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

namespace MafOpenRouterClient;


internal class Program
{
    private static async Task Main(string[] args)
    {
        // 1. Carregar as configurações do appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var settings = config.GetSection("AiSettings").Get<AiSettings>();

        if (settings == null)
        {
            Console.WriteLine("Erro: Não foi possível carregar as configurações AiSettings do appsettings.json.");
            return;
        }

        Console.WriteLine($"[INFO] Inicializando usando o provedor ativo: {settings.ActiveProvider}");

        // 2. Inicializar o Agente via a fábrica reutilizável
        var agent = AiClientFactory.CreateAgent(settings);

        try
        {
            Console.WriteLine("\n=== Chat Interativo Iniciado (Digite 'sair' para encerrar) ===");
            
            // Memória da Conversa (Histórico)
            var history = new System.Collections.Generic.List<Microsoft.Extensions.AI.ChatMessage>();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\n👤 Você: ");
                Console.ResetColor();

                // Lê a pergunta do usuário
                var input = Console.ReadLine();

                // Verifica se o usuário deseja sair ou se a entrada é vazia
                if (string.IsNullOrWhiteSpace(input) || input.Trim().Equals("sair", StringComparison.OrdinalIgnoreCase)) break;

                // Adiciona o request do user na memória
                history.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.User, input));

                var sb = new System.Text.StringBuilder();
                bool isFirstChunk = true;

                // Em um foreach assíncrono, processa os updates do agente e exibe a resposta em tempo real
                await foreach (var update in agent.RunStreamingAsync(history))
                {
                    if (update.Text != null)
                    {
                        if (isFirstChunk)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("🤖 Agente: ");
                            Console.ResetColor();
                            isFirstChunk = false;
                        }

                        Console.Write(update.Text);
                        sb.Append(update.Text);
                    }
                }
                Console.WriteLine();
                
                // Adiciona a resposta do agente na memória
                history.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, sb.ToString()));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Ocorreu uma exceção: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}


public class AiSettings
{
    public string ActiveProvider { get; set; } = "OpenRouter";
    public bool EnableLogging { get; set; } = true;
    public string SystemPrompt { get; set; } = "Responda de forma direta e objetiva.";
    public Dictionary<string, AiProviderConfig> Providers { get; set; } = new();
}

public class AiProviderConfig
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string ModelId { get; set; }
    public string CustomTitle { get; set; }
    public bool RequiresCustomHeaders { get; set; }
}

public static class AiClientFactory
{
    public static IChatClient CreateChatClient(AiSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        if (string.IsNullOrWhiteSpace(settings.ActiveProvider)) throw new ArgumentException("ActiveProvider is not set.");
        if (!settings.Providers.TryGetValue(settings.ActiveProvider, out var providerConfig))
        {
            throw new ArgumentException($"Provider '{settings.ActiveProvider}' not found in configuration.");
        }

        var options = new OpenAIClientOptions();

        if (!string.IsNullOrWhiteSpace(providerConfig.BaseUrl))
        {
            options.Endpoint = new Uri(providerConfig.BaseUrl);
        }

        var client = new OpenAIClient(new ApiKeyCredential(providerConfig.ApiKey), options);
        return client.GetChatClient(providerConfig.ModelId).AsIChatClient();
    }

    public static AIAgent CreateAgent(AiSettings settings)
    {
        var chatClient = CreateChatClient(settings);
        return chatClient.CreateAIAgent(settings.SystemPrompt);
    }
}
