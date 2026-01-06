using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class CreateAdminDto
    {
        public string Username { get; set; }
        public string Password { get; set; } // סיסמה גלויה שהמנהל מקליד
        // אפשר להוסיף: Email, FullName וכו'
    }
}