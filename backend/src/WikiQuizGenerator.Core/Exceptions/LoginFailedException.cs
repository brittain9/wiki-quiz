namespace WikiQuizGenerator.Core.Exceptions;

public class LoginFailedException(string email) : Exception($"Invalid email: {email} or password.");