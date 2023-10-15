using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;


string YOUR_ACCESS_TOKEN_HERE = "6134522521:AAG9kjfCnGmMV9pFKZPIHsIDPTl3EGgwRUM";
var botClient = new TelegramBotClient($"{YOUR_ACCESS_TOKEN_HERE}");

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{

    var handler = update.Type switch
    {
        UpdateType.Message => HandleMessageAsync(botClient, update, cancellationToken),
        UpdateType.CallbackQuery => HandleCallBackQueryAsync(botClient, update, cancellationToken),
    };
    try
    {
        await handler;
    }
    catch
    {
        await Console.Out.WriteLineAsync("XATO");
    }

}

static async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;

    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;


    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
    ////////////////////////////////////////////////////////////////////////////

    string x = messageText;

    string Natijalar;
    List<string> NatijalarListi = new List<string>();

    string str = "[";
    for (int i = 0; i < x.Length; i++)
    {
        if (!str.Contains(x[i].ToString())) str = str + x[i].ToString();
    }
    str += "]";

    using (HttpClient client = new HttpClient())
    {

        string BaseUrl = "https://api.publicapis.org/entries";
        HttpResponseMessage proces = await client.GetAsync(BaseUrl);
        string JsonResult = await proces.Content.ReadAsStringAsync();
        CountAndApi APIss = JsonConvert.DeserializeObject<CountAndApi>(JsonResult);
        //Console.WriteLine(JsonResult);

        foreach (var item in APIss.entries)
        {
            if (Regex.IsMatch(item.API, str))
            {
                NatijalarListi.Add(item.Link);
            }
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////
    Random rnd = new Random();
    int KeyingiButton = rnd.Next(NatijalarListi.Count);
    int sanoqchi = 0;
    string ekeangaChiquvchiMalumot = "";
    /////////////////////////////////////////////////////////////////////////////////////////////// 

    List<List<InlineKeyboardButton>> KopQatorButtonlar = new List<List<InlineKeyboardButton>>();
    for (int i = 0; i < 2; i++)
    {
        List<InlineKeyboardButton> qatorButtonlar = new List<InlineKeyboardButton>();
        for (int j = 0; j < 5; j++)
        {
            if (KeyingiButton * 10 + sanoqchi < NatijalarListi.Count)
            {
                qatorButtonlar.Add(InlineKeyboardButton.WithCallbackData(text: (sanoqchi + 1).ToString(), callbackData: NatijalarListi[KeyingiButton * 10 + sanoqchi]));
                ekeangaChiquvchiMalumot = ekeangaChiquvchiMalumot + String.Format("{0}.{1}\n", 1 + sanoqchi, NatijalarListi[KeyingiButton * 10 + sanoqchi]);
                sanoqchi++;
            }
        }
        KopQatorButtonlar.Add(qatorButtonlar);
    }

    List<InlineKeyboardButton> harakatButtonlar = new List<InlineKeyboardButton>()
        {
            InlineKeyboardButton.WithCallbackData(text: "<-", callbackData: "<-"),
            InlineKeyboardButton.WithCallbackData(text: "x", callbackData: "x"),
            InlineKeyboardButton.WithCallbackData(text: "->", callbackData: "->"),
        };

    ///////////////////////////////////////////////////////////////////////////////////////////////

    KopQatorButtonlar.Add(harakatButtonlar);
    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(KopQatorButtonlar);
    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: ekeangaChiquvchiMalumot,
        replyMarkup: inlineKeyboard,
        cancellationToken: cancellationToken);



}

static async Task HandleCallBackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    string? xabarniJonat = update.CallbackQuery.Data;
    if (xabarniJonat.Equals("x") || xabarniJonat.Equals("->") || xabarniJonat.Equals("<-"))
        HandleMessageAsync(botClient, update, cancellationToken);
    else
    {
        Message sentMessage1 = await botClient.SendTextMessageAsync(
        chatId: update.CallbackQuery.From.Id,
        text: String.Format("{0}", xabarniJonat),
        cancellationToken: cancellationToken);
    }

}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
