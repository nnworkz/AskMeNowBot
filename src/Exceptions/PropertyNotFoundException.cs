namespace AskMeNowBot.Exceptions;

public class PropertyNotFoundException(string resource) : Exception($"Property not found: {resource}");
