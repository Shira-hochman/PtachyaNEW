using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// בהנחה ש-KindergartenDto קיים, יש לוודא שהוא כולל את שדה ה-Code
namespace Dto
{
    public class KindergartenDto
    {
        public int KindergartenId { get; set; } // אופציונלי
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty; // מפתח ייחודי

        public string Code { get; set; } = string.Empty; // מפתח ייחודי

    }
}
