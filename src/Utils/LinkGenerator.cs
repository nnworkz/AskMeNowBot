using AskMeNowBot.Configuration;

namespace AskMeNowBot.Utils;

public class LinkGenerator(Config config)
{
	public string Get(long id)
	{
		return $"t.me/share/url?url=t.me/{config.BotUsername}?start={id}";
	}
}
