using Telegram.Bot.Types;

namespace AskMeNowBot.Utils.TextFormat;

public interface ITextFormat
{
    string EscapeMarkdownV2(string message);
    string Format(string message, MessageEntity entity);
}
