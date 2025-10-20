using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Form
{
    public int FormId { get; set; }

    public int ChildId { get; set; }

    public string FormLink { get; set; } = null!;

    public DateTime? SubmittedDate { get; set; }

    public virtual Child Child { get; set; } = null!;
}
