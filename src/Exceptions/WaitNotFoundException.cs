namespace AskMeNowBot.Exceptions;

public class WaitNotFoundException(long id) : Exception($"Wait not found: {id}");
