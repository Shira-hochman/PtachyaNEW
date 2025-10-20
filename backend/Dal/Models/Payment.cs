using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int ChildId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public virtual ICollection<Child> Children { get; set; } = new List<Child>();
}
