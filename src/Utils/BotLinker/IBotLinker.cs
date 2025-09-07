namespace AskMeNowBot.Utils.BotLinker;

public interface IBotLinker
{
    string GetStartLink(string id);
    string GenerateLink();
}
