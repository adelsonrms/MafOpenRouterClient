using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using MafOpenRouterClient;

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
                Console.Write("\nVocê: ");

                // Lê a pergunta do usuário
                var input = Console.ReadLine();

                // Verifica se o usuário deseja sair ou se a entrada é vazia
                if (string.IsNullOrWhiteSpace(input) || input.Trim().Equals("sair", StringComparison.OrdinalIgnoreCase)) break;

                // Adiciona o request do user na memória
                history.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.User, input));

                Console.Write("Agente: ");
                
                var sb = new System.Text.StringBuilder();


                //Em um foreach assíncrono, processa os updates do agente e exibe a resposta em tempo real
                // O método RunStreamingAsync retorna um IAsyncEnumerable<ChatUpdate>, onde cada ChatUpdate contém um fragmento da resposta do agente.
                await foreach (var update in agent.RunStreamingAsync(history))
                {
                    if (update.Text != null)
                    {
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