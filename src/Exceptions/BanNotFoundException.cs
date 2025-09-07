namespace AskMeNowBot.Exceptions;

public class BanNotFoundException(long senderId, long recipientId)
    : Exception($"Ban not found: {senderId},  {recipientId}");
