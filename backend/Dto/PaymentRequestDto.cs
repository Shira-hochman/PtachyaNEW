using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class PaymentRequestDTO
    {
        public int ChildId { get; set; }
        public decimal Amount { get; set; }

        public string? CardNumber { get; set; }
        public string? Expiry { get; set; }
        public string? Cvv { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int NumPayments { get; set; } = 3;
    }


}
