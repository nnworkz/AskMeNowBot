using AskMeNowBot.Localization.Enum;

namespace AskMeNowBot.User;

public record BaseUser(
    long Id,
    LanguageName Language,
    UserRole Role,
    long Messages,
    int MessagesToday,
    DateTime LastResetAt,
    DateTime? LastMessageAt,
    DateTime RegisteredAt
) : IUser
{
    public long Id { get; set; } = Id;
    public LanguageName Language { get; set; } = Language;
    public UserRole Role { get; set; } = Role;
    public long Messages { get; set; } = Messages;
    public int MessagesToday { get; set; } = MessagesToday;
    public DateTime LastResetAt { get; set; } = LastResetAt;
    public DateTime? LastMessageAt { get; set; } = LastMessageAt;
    public DateTime RegisteredAt { get; set; } = RegisteredAt;
}
