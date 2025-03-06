using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot.Utils;

public static class TextFormat
{
	private static readonly char[] MarkdownV2Chars =
	[
		'_', '*', '`', '[', ']', '(', ')', '~', '>', '#', '+', '-', '.', '!'
	];

	public static StringBuilder EscapeMarkdownV2(StringBuilder message)
	{
		foreach (var symbol in MarkdownV2Chars)
		{
			var symbolString = symbol.ToString();

			message.Replace(symbolString, "\\" + symbolString);
		}

		return message;
	}

	public static StringBuilder Format(StringBuilder message, MessageEntity entity)
	{
		var offset = entity.Offset;
		var length = entity.Length;
		
		return entity.Type switch
		{
			MessageEntityType.Bold          => MarkdownV2(message, offset, length, "*", "*"),
			MessageEntityType.Italic        => MarkdownV2(message, offset, length, "_", "_"),
			MessageEntityType.Underline     => MarkdownV2(message, offset, length, "__", "__"),
			MessageEntityType.Strikethrough => MarkdownV2(message, offset, length, "~", "~"),
			MessageEntityType.Spoiler       => MarkdownV2(message, offset, length, "||", "||"),
			MessageEntityType.Code          => MarkdownV2(message, offset, length, "```", "```"),
			MessageEntityType.Pre           => MarkdownV2(message, offset, length, "`", "`"),
			_ => message
		};
	}

	private static StringBuilder MarkdownV2(StringBuilder message, int offset, int length, string symbol, string end)
	{
		return message.Insert(offset + length, end).Insert(offset, symbol);
	}
}
