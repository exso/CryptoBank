namespace CryptoBank.Tests.Integration.Errors.Contracts;

public class ValidationProblemDetailsContract
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public ErrorDataContract[] Errors { get; set; } = Array.Empty<ErrorDataContract>();
}

public class ErrorDataContract
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
