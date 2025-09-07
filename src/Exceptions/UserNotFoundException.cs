namespace AskMeNowBot.Exceptions;

public class UserNotFoundException(long id) : Exception($"User not found: {id}");
