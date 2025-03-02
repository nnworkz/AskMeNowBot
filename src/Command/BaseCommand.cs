using AskMeNowBot.User;

namespace AskMeNowBot.Command;

public record BaseCommand(string Name, string? Args, Sender Sender);
