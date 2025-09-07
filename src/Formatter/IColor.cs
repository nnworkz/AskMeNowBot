namespace AskMeNowBot.Formatter;

public interface IColor
{
    string Black { get; }
    string Red { get; }
    string Green { get; }
    string Yellow { get; }
    string Blue { get; }
    string Purple { get; }
    string Cyan { get; }
    string White { get; }

    string LightBlack { get; }
    string LightRed { get; }
    string LightGreen { get; }
    string LightYellow { get; }
    string LightBlue { get; }
    string LightPurple { get; }
    string LightCyan { get; }
    string LightWhite { get; }

    string BgBlack { get; }
    string BgRed { get; }
    string BgGreen { get; }
    string BgYellow { get; }
    string BgBlue { get; }
    string BgPurple { get; }
    string BgCyan { get; }
    string BgWhite { get; }

    string BgLightBlack { get; }
    string BgLightRed { get; }
    string BgLightGreen { get; }
    string BgLightYellow { get; }
    string BgLightBlue { get; }
    string BgLightPurple { get; }
    string BgLightCyan { get; }
    string BgLightWhite { get; }

    string Reset { get; }
    string Bold { get; }
    string Underline { get; }
    string Reverse { get; }
    string Italic { get; }
    string ResetColor { get; }

    string GetCode(string colorTag);
    string[] GetAllCodes();
}
