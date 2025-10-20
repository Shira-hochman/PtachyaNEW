using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Kindergarten
{
    public int KindergartenId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Child> Children { get; set; } = new List<Child>();
}
