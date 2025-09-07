namespace AskMeNowBot.Exceptions;

public class InvalidColorTagException(string tag) : Exception($"Invalid color tag: {tag}");
