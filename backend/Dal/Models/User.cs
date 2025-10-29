using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    // ✅ השדה הזה יחזיק מעכשיו את הסיסמה המגובבת (Hash)
    public string PasswordHash { get; set; } = null!;
}
