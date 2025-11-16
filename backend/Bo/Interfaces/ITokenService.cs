using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dto; // ודאי שזה מכיל את Dto.ChildDto

namespace Bo.Interfaces
{
    // ⭐️ חובה: זה חייב להיות 'public interface'
    public interface ITokenService
    {
        // חתימה של המתודה ליצירת התוקן
        string GenerateToken(ChildDto child);
    }
}