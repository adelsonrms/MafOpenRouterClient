# 🤖 MAF + OpenRouter - Chat Interativo com Memória em .NET

Este repositório demonstra como integrar o **Microsoft Agent Framework (MAF)** e as novas abstrações de IA do .NET (`Microsoft.Extensions.AI`) com o **OpenRouter** (ou OpenAI) para criar um chat interativo de console com memória de conversação.

## 🚀 Funcionalidades
- **Suporte Multi-Provedor**: Configure facilmente OpenRouter, OpenAI ou qualquer provedor compatível com o protocolo OpenAI via `appsettings.json`.
- **Chat Interativo**: Loop de console contínuo (Pergunta/Resposta).
- **Memória Persistent**: O agente mantém o contexto da conversa utilizando `List<ChatMessage>`.
- **Streaming**: Respostas em tempo real conforme são geradas pelo modelo.
- **Factory Flexível**: Arquitetura desacoplada para gerenciar clients e configurações.

## 🛠️ Pré-requisitos e Dependências
- **Runtime**: .NET 8.0, 9.0 ou 10.0.
- **API Key**: Uma chave válida do [OpenRouter](https://openrouter.ai/) ou [OpenAI](https://openai.com/).

### Pacotes NuGet Necessários:
Para reproduzir este projeto, você precisará instalar os seguintes pacotes via CLI:

```bash
# Core do Microsoft Agent Framework (MAF)
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0-preview.*

# Abstrações oficiais de IA da Microsoft e Integração OpenAI
dotnet add package Microsoft.Extensions.AI
dotnet add package Microsoft.Extensions.AI.OpenAI --version 10.*-preview.*

# Infraestrutura de Configuração
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.Binder
```

## 📖 Guia de Implementação

### 1. Configurando o Agente
A base da implementação utiliza as classes `OpenAIClient` e a extensão `AsIChatClient()`.

> [!TIP]
> **O Pulo do Gato**: Como o OpenRouter segue o padrão da OpenAI, para fazê-lo funcionar basta sobrescrever o `Endpoint` nas opções do Client. Isso torna o seu Agente compatível com qualquer provedor que suporte esse protocolo!

```csharp
// Configuração de opções com endpoint customizado para OpenRouter
var options = new OpenAIClientOptions { 
    Endpoint = new Uri("https://openrouter.ai/api/v1") 
};

// Adição de Policy para Headers Customizados (X-Title e Auth)
options.AddPolicy(new AddHeadersPolicy(apiKey, "Meu App MAF"), PipelinePosition.PerCall);

var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);

// Criando o ChatClient e o Agente
IChatClient chatClient = client.GetChatClient("modelo-escolhido").AsIChatClient();
AIAgent agent = chatClient.CreateAIAgent("Você é um assistente prestativo.");
```

### 2. Memória da Conversa
Para o agente "lembrar" do que foi dito, passamos o histórico completo de `ChatMessage` a cada requisição.

```csharp
// Histórico em memória
var history = new List<ChatMessage>();

// Ao receber o input do usuário:
history.Add(new ChatMessage(ChatRole.User, inputUsuario));

// Ao processar e receber a resposta do assistente:
history.Add(new ChatMessage(ChatRole.Assistant, respostaCompleta));
```

### 3. Processando Respostas em Streaming
Utilize o `RunStreamingAsync` para uma experiência de UI muito melhor no console.

```csharp
var sb = new StringBuilder();

await foreach (var update in agent.RunStreamingAsync(history))
{
    if (update.Text != null)
    {
        Console.Write(update.Text);
        sb.Append(update.Text);
    }
}

// Salva o texto completo retornado na memória
history.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));
```

## ⚙️ Como usar este repositório
1. Clone o repositório.
2. Renomeie o arquivo `appsettings.example.json` para `appsettings.json`.
3. Insira sua chave de API no campo `ApiKey`.
4. Execute com `dotnet run`.

## 📚 Referências e Agradecimentos
Este projeto foi inspirado e serviu como uma evolução baseada no excelente artigo do **Balta.io**: 
- [Criando seu primeiro Agente de IA com MAF](https://blog.balta.io/criando-seu-primeiro-agente-de-ia/)

**Contribuição desta versão**: 
Adaptação da implementação original (feita para OpenAI) para o **OpenRouter**, permitindo o uso de modelos gratuitos e a flexibilidade de transitar entre diferentes LLMs (Anthropic, Gemini, Qwen, etc) mantendo a mesma base de código.

---
Desenvolvido para fins didáticos explorando o ecossistema de Agents no .NET. 🚀
