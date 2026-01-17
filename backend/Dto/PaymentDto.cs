using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int ChildId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
