using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class VerificationRequest
    {
        public string IdNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
    }
}

