namespace AskMeNowBot.Exceptions;

public class TransactionsNotFoundException(long id) : Exception($"Transactions not found: {id}");
