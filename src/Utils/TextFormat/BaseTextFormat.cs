using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot.Utils.TextFormat;

public class BaseTextFormat : ITextFormat
{
    public string EscapeMarkdownV2(string message)
    {
        char[] chars = ['_', '*', '`', '[', ']', '(', ')', '{', '}', '~', '>', '#', '+', '-', '.', '!', '|'];
        
        foreach (var c in chars)
        {
            message = message.Replace(c.ToString(), "\\" + c);
        }

        return message;
    }

    public string Format(string message, MessageEntity entity)
    {
        var offset = entity.Offset;
        var length = entity.Length;

        return entity.Type switch
        {
            MessageEntityType.Bold => MarkdownV2("*", "*"),
            MessageEntityType.Italic => MarkdownV2("_", "_"),
            MessageEntityType.Underline => MarkdownV2("__", "__"),
            MessageEntityType.Strikethrough => MarkdownV2("~", "~"),
            MessageEntityType.Spoiler => MarkdownV2("||", "||"),
            MessageEntityType.Code => MarkdownV2("```", "```"),
            MessageEntityType.Pre => MarkdownV2("`", "`"),
            _ => message
        };

        string MarkdownV2(string symbol, string end)
        {
            return message.Insert(offset + length, end).Insert(offset, symbol);
        }
    }
}
