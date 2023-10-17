namespace CryptoBank.Features.Management.Domain;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; } 
    public User? User { get; set; } //= new User();
    public Role? Role { get; set; } //= new Role();
}
