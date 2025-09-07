namespace AskMeNowBot.Exceptions;

public class SubscriptionNotFoundException(long id) : Exception($"Subscription not found: {id}");
