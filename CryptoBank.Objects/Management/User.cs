using System;
using System.Collections.Generic;

namespace CryptoBank.Objects.Management;

public class User
{
    public int Id { get; set; } 
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfRegistration { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
}
