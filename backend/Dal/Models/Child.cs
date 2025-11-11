using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Child
{
    public int ChildId { get; set; }

    public int KindergartenId { get; set; }

    public string IdNumber { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string lastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string SchoolYear { get; set; } = null!;

    public string? FormLink { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? PaymentId { get; set; }

    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();

    public virtual Kindergarten Kindergarten { get; set; } = null!;

    public virtual Payment? Payment { get; set; }
}
