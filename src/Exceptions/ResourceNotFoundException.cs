namespace AskMeNowBot.Exceptions;

public class ResourceNotFoundException(string resource) : Exception($"Resource not found: {resource}");
