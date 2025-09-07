namespace AskMeNowBot.Exceptions;

public class InvalidPaymentTypeException(string type) : Exception($"Invalid payment type: {type}");
