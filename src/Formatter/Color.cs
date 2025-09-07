using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Formatter;

public class Color : IColor
{
    public string Black => "§0";
    public string Red => "§1";
    public string Green => "§2";
    public string Yellow => "§3";
    public string Blue => "§4";
    public string Purple => "§5";
    public string Cyan => "§6";
    public string White => "§7";

    public string LightBlack => "§8";
    public string LightRed => "§9";
    public string LightGreen => "§a";
    public string LightYellow => "§b";
    public string LightBlue => "§c";
    public string LightPurple => "§d";
    public string LightCyan => "§e";
    public string LightWhite => "§f";

    public string BgBlack => "§g";
    public string BgRed => "§h";
    public string BgGreen => "§i";
    public string BgYellow => "§j";
    public string BgBlue => "§k";
    public string BgPurple => "§l";
    public string BgCyan => "§m";
    public string BgWhite => "§n";

    public string BgLightBlack => "§o";
    public string BgLightRed => "§p";
    public string BgLightGreen => "§q";
    public string BgLightYellow => "§r";
    public string BgLightBlue => "§s";
    public string BgLightPurple => "§t";
    public string BgLightCyan => "§u";
    public string BgLightWhite => "§v";

    public string Reset => "§w";
    public string Bold => "§x";
    public string Underline => "§y";
    public string Reverse => "§z";
    public string Italic => "§A";
    public string ResetColor => "§B";

    private readonly Dictionary<string, string> _codes;

    public Color()
    {
        _codes = new Dictionary<string, string>
        {
            { Black, "\u001b[0;30m" },
            { Red, "\u001b[0;31m" },
            { Green, "\u001b[0;32m" },
            { Yellow, "\u001b[0;33m" },
            { Blue, "\u001b[0;34m" },
            { Purple, "\u001b[0;35m" },
            { Cyan, "\u001b[0;36m" },
            { White, "\u001b[0;37m" },
            { LightBlack, "\u001b[0;90m" },
            { LightRed, "\u001b[0;91m" },
            { LightGreen, "\u001b[0;92m" },
            { LightYellow, "\u001b[0;93m" },
            { LightBlue, "\u001b[0;94m" },
            { LightPurple, "\u001b[0;95m" },
            { LightCyan, "\u001b[0;96m" },
            { LightWhite, "\u001b[0;97m" },
            { BgBlack, "\u001b[0;40m" },
            { BgRed, "\u001b[0;41m" },
            { BgGreen, "\u001b[0;42m" },
            { BgYellow, "\u001b[0;43m" },
            { BgBlue, "\u001b[0;44m" },
            { BgPurple, "\u001b[0;45m" },
            { BgCyan, "\u001b[0;46m" },
            { BgWhite, "\u001b[0;47m" },
            { BgLightBlack, "\u001b[0;100m" },
            { BgLightRed, "\u001b[0;101m" },
            { BgLightGreen, "\u001b[0;102m" },
            { BgLightYellow, "\u001b[0;103m" },
            { BgLightBlue, "\u001b[0;104m" },
            { BgLightPurple, "\u001b[0;105m" },
            { BgLightCyan, "\u001b[0;106m" },
            { BgLightWhite, "\u001b[0;107m" },
            { Reset, "\u001b[0m" },
            { Bold, "\u001b[1m" },
            { Italic, "\u001b[3m" },
            { Underline, "\u001b[4m" },
            { Reverse, "\u001b[7m" },
            { ResetColor, "\u001b[39m" }
        };
    }

    public string GetCode(string colorTag)
    {
        if (_codes.TryGetValue(colorTag, out var code))
        {
            return code;
        }

        throw new InvalidColorTagException(colorTag);
    }

    public string[] GetAllCodes() => _codes.Keys.ToArray();
}
