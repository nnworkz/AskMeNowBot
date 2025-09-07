using AskMeNowBot.Localization.Enum;

namespace AskMeNowBot.User;

public interface IUser
{
    long Id { get; set; }
    LanguageName Language { get; set; }
    UserRole Role { get; set; }
    long Messages { get; set; }
    int MessagesToday { get; set; }
    DateTime LastResetAt { get; set; }
    DateTime? LastMessageAt { get; set; }
    DateTime RegisteredAt { get; set; }
}
