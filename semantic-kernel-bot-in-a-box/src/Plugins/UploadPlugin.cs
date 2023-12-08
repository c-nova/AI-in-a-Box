using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using System.Linq;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using System.Collections.Generic;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Plugins;

public class UploadPlugin
{
    private readonly AzureOpenAITextEmbeddingGeneration _embeddingClient;
    private ConversationData _conversationData;
    private ITurnContext<IMessageActivity> _turnContext;

    public UploadPlugin(ConversationData conversationData, ITurnContext<IMessageActivity> turnContext, AzureOpenAITextEmbeddingGeneration embeddingClient)
    {
        _embeddingClient = embeddingClient;
        _conversationData = conversationData;
        _turnContext = turnContext;
    }


    [SKFunction, Description("アップロードされたドキュメントで関連情報を検索します。これは、ユーザーがアップロードしたドキュメントを参照する場合にのみ使用します。")]
    public async Task<string> SearchUploads(
        [Description("検索するドキュメントの正確な名前")] string docName,
        [Description("類似性で検索するテキスト")] string query
    )
    {
        await _turnContext.SendActivityAsync($"\"{query}\" で {docName} 内の類似性を検索中 ...");
        var embedding = await _embeddingClient.GenerateEmbeddingsAsync(new List<string> { query });
        var vector = embedding.First().ToArray();
        var similarities = new List<float>();
        var attachment = _conversationData.Attachments.Find(x => x.Name == docName);
        foreach (AttachmentPage page in attachment.Pages)
        {
            float similarity = 0;
            for (int i = 0; i < page.Vector.Count(); i++)
            {
                similarity += page.Vector[i] * vector[i];
            }
            similarities.Add(similarity);
        }
        var maxIndex = similarities.IndexOf(similarities.Max());
        return _conversationData.Attachments.First().Pages[maxIndex].Content;
    }

}