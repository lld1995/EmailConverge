using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;

namespace EmailConverge.Services
{
    public class AiSummaryService
    {
        private AiConfig _config;
        private static readonly HttpClient _httpClient = new();
        
        private const int MaxChunkSize = 64000;
        private const int ChunkOverlap = 500;

        public AiSummaryService()
        {
            _config = AiConfig.Load();
        }

        public void UpdateConfig(AiConfig config)
        {
            _config = config;
            _config.Save();
        }

        public AiConfig GetConfig() => _config;

        public async Task<List<string>> GetModelsAsync(string? endpoint = null, string? apiKey = null)
        {
            var models = new List<string>();
            try
            {
                var baseUrl = endpoint ?? _config.Endpoint;
                var key = apiKey ?? _config.ApiKey;
                
                if (!baseUrl.EndsWith("/"))
                    baseUrl += "/";
                var modelsUrl = baseUrl + "models";

                using var request = new HttpRequestMessage(HttpMethod.Get, modelsUrl);
                if (!string.IsNullOrEmpty(key) && key != "none" && key != "ollama")
                {
                    request.Headers.Add("Authorization", $"Bearer {key}");
                }

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    
                    if (doc.RootElement.TryGetProperty("data", out var dataArray))
                    {
                        foreach (var model in dataArray.EnumerateArray())
                        {
                            if (model.TryGetProperty("id", out var idProp))
                            {
                                models.Add(idProp.GetString() ?? "");
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return models;
        }


        public async Task<string> SummarizeAsync(string emailContent, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new OpenAIClient(
                    new ApiKeyCredential(_config.ApiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(_config.Endpoint) });

                var chatClient = client.GetChatClient(_config.Model);

                var messages = new List<ChatMessage>
                {
                    new UserChatMessage(SummaryTemplates.GetPrompt(SummaryTemplateType.KeyPoints, emailContent))
                };

                var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                return response.Value.Content[0].Text ?? "无法获取总结结果";
            }
            catch (Exception ex)
            {
                return $"AI总结失败: {ex.Message}";
            }
        }

        public async Task StreamSummarizeAsync(string emailContent, SummaryTemplateType templateType, Action<string> onToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new OpenAIClient(
                    new ApiKeyCredential(_config.ApiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(_config.Endpoint) });

                var chatClient = client.GetChatClient(_config.Model);

                // 检查内容是否需要分段处理
                if (emailContent.Length > MaxChunkSize)
                {
                    await ProcessLargeContentAsync(emailContent, templateType, chatClient, onToken, cancellationToken);
                }
                else
                {
                    var messages = new List<ChatMessage>
                    {
                        new UserChatMessage(SummaryTemplates.GetPrompt(templateType, emailContent))
                    };

                    await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken))
                    {
                        foreach (var part in update.ContentUpdate)
                        {
                            if (!string.IsNullOrEmpty(part.Text))
                            {
                                onToken(part.Text);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                onToken($"\n\nAI总结失败: {ex.Message}");
            }
        }

        private async Task ProcessLargeContentAsync(string content, SummaryTemplateType templateType, ChatClient chatClient, Action<string> onToken, CancellationToken cancellationToken)
        {
            var chunks = SplitIntoChunks(content);
            var chunkSummaries = new List<string>();

            onToken($"📊 内容较大，正在分 {chunks.Count} 段处理...\n\n");

            // 第一阶段：对每个分段进行摘要
            for (int i = 0; i < chunks.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                onToken($"--- 处理第 {i + 1}/{chunks.Count} 段 ---\n");

                var chunkPrompt = $"""
                    请对以下内容片段进行简要总结，提取关键信息（这是第 {i + 1} 段，共 {chunks.Count} 段）：

                    {chunks[i]}

                    请简洁总结这段内容的要点：
                    """;

                var messages = new List<ChatMessage> { new UserChatMessage(chunkPrompt) };
                var summaryBuilder = new StringBuilder();

                await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken))
                {
                    foreach (var part in update.ContentUpdate)
                    {
                        if (!string.IsNullOrEmpty(part.Text))
                        {
                            summaryBuilder.Append(part.Text);
                        }
                    }
                }

                chunkSummaries.Add(summaryBuilder.ToString());
                onToken($"✓ 第 {i + 1} 段完成\n\n");
            }

            // 第二阶段：合并所有分段摘要，生成最终总结
            onToken("--- 正在生成最终总结 ---\n\n");

            var combinedSummaries = string.Join("\n\n", chunkSummaries.Select((s, i) => $"【第{i + 1}段摘要】\n{s}"));
            var finalPrompt = SummaryTemplates.GetPrompt(templateType, combinedSummaries);

            var finalMessages = new List<ChatMessage> { new UserChatMessage(finalPrompt) };

            await foreach (var update in chatClient.CompleteChatStreamingAsync(finalMessages, cancellationToken: cancellationToken))
            {
                foreach (var part in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                    {
                        onToken(part.Text);
                    }
                }
            }
        }

        private List<string> SplitIntoChunks(string content)
        {
            var chunks = new List<string>();
            var lines = content.Split('\n');
            var currentChunk = new StringBuilder();

            foreach (var line in lines)
            {
                if (currentChunk.Length + line.Length + 1 > MaxChunkSize)
                {
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString());
                        
                        // 保留一些重叠内容以保持上下文连贯
                        var overlap = currentChunk.ToString();
                        currentChunk.Clear();
                        if (overlap.Length > ChunkOverlap)
                        {
                            var lastPart = overlap.Substring(overlap.Length - ChunkOverlap);
                            var lastNewline = lastPart.LastIndexOf('\n');
                            if (lastNewline > 0)
                            {
                                currentChunk.Append(lastPart.Substring(lastNewline + 1));
                            }
                        }
                    }
                }
                currentChunk.AppendLine(line);
            }

            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
            }

            return chunks;
        }
    }
}
