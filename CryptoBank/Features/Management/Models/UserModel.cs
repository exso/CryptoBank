namespace CryptoBank.Features.Management.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfRegistration { get; set; }
}

public class Roles
{
    public const string Administrator = "Administrator";
    public const string User = "User";
    public const string Analyst = "Analyst";
}


