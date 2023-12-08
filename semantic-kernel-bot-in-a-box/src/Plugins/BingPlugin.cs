using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Text.Json;
using Models;
using Services;

namespace Plugins;
public class BingPlugin
{
    private ITurnContext<IMessageActivity> _turnContext;
    private BingClient _bingClient;

    public BingPlugin(ConversationData conversationData, ITurnContext<IMessageActivity> turnContext, BingClient bingClient)
    {
        _turnContext = turnContext;
        _bingClient = bingClient;
    }
    // Returns search results with headers.

    [SKFunction, Description("Bingを使用してテキストでインターネットを検索します。利用規約では、検索結果がウェブから取得されたことを明示し、提供する情報に関連するリンクをリストアップする必要があります。この機能を使用する前に、アシスタントは必ずユーザーに Web 検索を依頼する必要があります。")]
    public async Task<string> BingSearch(
        [Description("Bing に渡すクエリ")] string query,
        [Description("探している結果の種類。\"webpages\",\"images\",\"videos\",\"news\" のいずれか。news で結果が返されない場合は、フォールバックとして webpages を試すことができます。")] string resultType
    )
    {
        await _turnContext.SendActivityAsync($"インターネットで {resultType} を \"{query}\" という説明で検索しています...");

        SearchResult result = await _bingClient.WebSearch(query, resultType);

        return JsonSerializer.Serialize(result);

    }

}