using Telegram.Bot.Types.Enums;

namespace AskMeNowBot.Exceptions;

public class UnexpectedUpdateTypeException(UpdateType type) : Exception($"Unexpected update type: {type.ToString()}");
