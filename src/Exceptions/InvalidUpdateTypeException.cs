namespace AskMeNowBot.Exceptions;

public class InvalidUpdateTypeException(string type) : Exception($"Unsupported update type: {type}");
