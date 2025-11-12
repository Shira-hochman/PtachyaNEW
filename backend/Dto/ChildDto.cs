using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class ChildDto
    {
        public int ChildId { get; set; }

        public int KindergartenId { get; set; }

        public string IdNumber { get; set; } = null!;

        public DateTime BirthDate { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string SchoolYear { get; set; } = null!;

        public string? FormLink { get; set; }

        public string Phone { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int? PaymentId { get; set; }


    }

}
