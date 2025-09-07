namespace AskMeNowBot.Exceptions;

public class InvalidErrorTypeException(string type) : Exception($"Invalid error type: {type}");
