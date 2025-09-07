using System.Text.RegularExpressions;

using AskMeNowBot.Exceptions;

using Serilog.Events;
using Serilog.Formatting;

namespace AskMeNowBot.Formatter;

public partial class ColorFormatter(IColor color) : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var segments = Escape().Split(logEvent.RenderMessage());
        var message = string.Empty;

        for (var i = 0; i < segments.Length; i++)
        {
            message += i > 0 ? segments[i].Replace("§", "§§") : segments[i];
        }

        var time = $"[{color.Blue}{logEvent.Timestamp:HH:mm:ss}{color.ResetColor}]";
        var level = GetLogLevel(logEvent.Level);
        message = $"{time} {level} {message}";

        message = color.GetAllCodes().Aggregate(
            message,
            (current, colorTag) => current.Replace(colorTag, color.GetCode(colorTag))
        );

        message += color.GetCode(color.Reset);
        output.WriteLine(message);
    }

    private string GetLogLevel(LogEventLevel level)
    {
        var (bgColor, name) = level switch
        {
            LogEventLevel.Verbose => (color.BgLightBlack, "VERB"),
            LogEventLevel.Debug => (color.BgBlue, "DEBG"),
            LogEventLevel.Information => (color.BgGreen, "INFO"),
            LogEventLevel.Warning => (color.BgLightYellow, "WARN"),
            LogEventLevel.Error => (color.BgRed, "ERRO"),
            LogEventLevel.Fatal => (color.BgPurple, "FATL"),
            _ => throw new InvalidLogLevelTypeException(level.ToString())
        };

        return $"{bgColor}{name}{color.Reset}";
    }

    [GeneratedRegex(@"\\§")]
    private partial Regex Escape();
}
