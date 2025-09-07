using AskMeNowBot.User;

namespace AskMeNowBot.Command;

public record BaseCommandContext(IUser CommandSender, string? Argument = null, ExtraData? ExtraData = null)
    : ICommandContext
{
    public bool HasArgument()
    {
        return !string.IsNullOrEmpty(Argument);
    }

    public bool HasExtraData()
    {
        return ExtraData is not null;
    }
}
