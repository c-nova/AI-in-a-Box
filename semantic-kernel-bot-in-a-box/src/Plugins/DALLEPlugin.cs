using System.ComponentModel;
using System.Threading.Tasks;
using Azure;
using Microsoft.SemanticKernel;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Azure.AI.OpenAI;
using System.Collections.Generic;

namespace Plugins;
public class DALLEPlugin
{
    private readonly OpenAIClient _aoaiClient;
    private ITurnContext<IMessageActivity> _turnContext;

    public DALLEPlugin(ConversationData conversationData, ITurnContext<IMessageActivity> turnContext, OpenAIClient aoaiClient)
    {
        _aoaiClient = aoaiClient;
        _turnContext = turnContext;
    }



    [SKFunction, Description("説明から画像を生成します。")]
    public async Task<string> GenerateImages(
        [Description("生成する画像の説明。")] string prompt,
        [Description("生成する画像の数。指定しない場合は、1を使用する必要があります")] int n
    )
    {
        await _turnContext.SendActivityAsync($"Generating {n} images with the description \"{prompt}\"...");
        Response<ImageGenerations> imageGenerations = await _aoaiClient.GetImageGenerationsAsync(
            new ImageGenerationOptions()
            {
                Prompt = prompt,
                Size = ImageSize.Size512x512,
                ImageCount = n
            });

        List<object> images = new();
        images.Add(
            new {
                type="TextBlock",
                text="こちらが生成された画像です。",
                size="large"
            }
        );
        foreach (ImageLocation img in imageGenerations.Value.Data)
            images.Add(new { type = "Image", url = img.Url.AbsoluteUri });
        object adaptiveCardJson = new
        {
            type = "AdaptiveCard",
            version = "1.0",
            body = images
        };

        var adaptiveCardAttachment = new Microsoft.Bot.Schema.Attachment()
        {
            ContentType = "application/vnd.microsoft.card.adaptive",
            Content = adaptiveCardJson,
        };
        await _turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment));
        return "画像は正常に生成され、ユーザーに送信されました。";
    }

}