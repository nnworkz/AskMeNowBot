using AskMeNowBot.User;

namespace AskMeNowBot.Command;

public interface ICommandContext
{
    IUser CommandSender { get; init; }
    string? Argument { get; init; }
    ExtraData? ExtraData { get; init; }

    bool HasArgument();
    bool HasExtraData();
}
