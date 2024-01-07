using CryptoBank.Errors.Exceptions.Base;

namespace CryptoBank.Errors.Exceptions;

public class LogicConflictException : ErrorException
{
    public LogicConflictException(string message, string code) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
