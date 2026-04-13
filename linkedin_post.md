🚀 **Evoluindo Agentes .NET: Da OpenAI para a liberdade do OpenRouter!**

Recentemente estudei um artigo excelente do Balta.io sobre como criar o primeiro agente de IA em .NET (https://blog.balta.io/criando-seu-primeiro-agente-de-ia/). A implementação original é fantástica, mas foca em OpenAI, o que exige uma conta paga.

Minha contribuição para a comunidade foi evoluir essa base e **adaptar o projeto para o OpenRouter**. Com isso, conseguimos testar o Microsoft Agent Framework (MAF) usando modelos gratuitos e variados (como Qwen, Llama, Claude e Gemini) de forma unificada!

O resultado é um repositório pronto para uso que implementa:

✅ **Chat Interativo via Console**: Interface fluida de Pergunta e Resposta.

✅ **Memória Persistente**: O Agente lembra do contexto usando o padrão `List<ChatMessage>`.

✅ **Respostas em Streaming**: Experiência de tempo real para o usuário final.

✅ **Arquitetura Flexível**: Gerenciamento de múltiplos provedores via `appsettings.json`.

O "pulo do gato" para a compatibilidade total foi sobrescrever o endpoint padrão do SDK da OpenAI. Como o OpenRouter segue o mesmo padrão de API, basta uma pequena configuração:

```csharp
var options = new OpenAIClientOptions();
options.Endpoint = new Uri("https://openrouter.ai/api/v1");
```

Esse projeto é ideal para quem quer começar a construir Agentes Inteligentes em C# sem custos iniciais de API, aproveitando o poder do OpenRouter com as bibliotecas oficiais da Microsoft (`Microsoft.Extensions.AI`).

🔗 **Repositório**: https://github.com/adelsonrms/MafOpenRouterClient

Se você trabalha com IA e .NET, vamos trocar figurinhas nos comentários! 🚀💻

#dotnet #csharp #ai #artificialintelligence #openrouter #balta #microsoft #agents #programming #developer #openai #github
