using System.Numerics;
using System.Security.Cryptography;
using System.Text;

using AskMeNowBot.Configuration;

namespace AskMeNowBot.Utils.BotLinker;

public class BaseBotLinker(IConfig config) : IBotLinker
{
    public string GetStartLink(string link)
    {
        return $"t.me/share/url?url=t.me/{config.Bot.Name}?start={link}";
    }

    public string GenerateLink()
    {
        var bytes = RandomNumberGenerator.GetBytes(9);
        var number = new BigInteger(bytes.Append((byte)0).ToArray());
        const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var builder = new StringBuilder();

        while (number > 0 && builder.Length < 12)
        {
            builder.Insert(0, chars[(int)(number % 62)]);
            number /= 62;
        }

        return builder.ToString().PadLeft(12, '0');
    }
}
