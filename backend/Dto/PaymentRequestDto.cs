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
        public string CardNumber { get; set; }
        public string Expiry { get; set; } // פורמט MMYY
        public string Cvv { get; set; }
        public string HolderId { get; set; }
        public string HolderName { get; set; }
    }
}
