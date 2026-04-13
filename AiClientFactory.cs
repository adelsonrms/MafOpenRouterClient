using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;

namespace MafOpenRouterClient;

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

        if (settings.EnableLogging)
        {
            options.ClientLoggingOptions = new ClientLoggingOptions
            {
                EnableLogging = true,
                EnableMessageContentLogging = true
            };
            options.AddPolicy(new RawResponseLoggingPolicy(), PipelinePosition.PerCall);
        }

        if (providerConfig.RequiresCustomHeaders)
        {
            options.AddPolicy(new AddHeadersPolicy(providerConfig.ApiKey, providerConfig.CustomTitle ?? "MAF Agent"), PipelinePosition.PerCall);
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

public class AddHeadersPolicy : PipelinePolicy
{
    private readonly string _apiKey;
    private readonly string _customTitle;
    
    public AddHeadersPolicy(string apiKey, string customTitle)
    {
        _apiKey = apiKey;
        _customTitle = customTitle;
    }

    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        message.Request.Headers.Set("Authorization", $"Bearer {_apiKey}");
        message.Request.Headers.Set("X-Title", _customTitle);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        message.Request.Headers.Set("Authorization", $"Bearer {_apiKey}");
        message.Request.Headers.Set("X-Title", _customTitle);
        return ProcessNextAsync(message, pipeline, currentIndex);
    }
}

public class RawResponseLoggingPolicy : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ProcessNext(message, pipeline, currentIndex);
        var resp = message.Response;
        if (resp?.ContentStream != null && resp.ContentStream.CanSeek)
        {
            using var sr = new System.IO.StreamReader(resp.ContentStream, leaveOpen: true);
            resp.ContentStream.Position = 0;
            //var body = sr.ReadToEnd();
            //Console.WriteLine($"[RAW RESPONSE] {resp.Status}");
            resp.ContentStream.Position = 0;
        }
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        await ProcessNextAsync(message, pipeline, currentIndex);
        var resp = message.Response;
        if (resp?.ContentStream != null && resp.ContentStream.CanSeek)
        {
            using var sr = new System.IO.StreamReader(resp.ContentStream, leaveOpen: true);
            resp.ContentStream.Position = 0;
            //var body = await sr.ReadToEndAsync();
            //Console.WriteLine($"[RAW RESPONSE] {resp.Status}");
            resp.ContentStream.Position = 0;
        }
    }
}
