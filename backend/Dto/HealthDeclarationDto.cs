using System;

namespace Dto
{
    public class HealthDeclarationDto
    {
        public ChildDetailsDto ChildDetails { get; set; } = new ChildDetailsDto();
        public FacilityDetailsDto FacilityDetails { get; set; } = new FacilityDetailsDto();
        public ParentDetailsDto Parent1 { get; set; } = new ParentDetailsDto();
        public ParentDetailsDto Parent2 { get; set; } = new ParentDetailsDto();

        public DateTime FormDate { get; set; }
        public string ProgramProvider { get; set; } = null!;
        public string ProgramFramework { get; set; } = null!;
        public decimal MonthlySelfParticipation { get; set; }
        public bool NoOtherProgramDeclaration { get; set; }
    }

    public class ChildDetailsDto
    {
        public string ChildFirstName { get; set; } = null!;
        public string ChildLastName { get; set; } = null!;
        public int ChildId { get; set; }  // ת"ז
        public DateTime ChildDob { get; set; } // תאריך לידה
        public string ChildAddress { get; set; } = null!;
    }

    public class FacilityDetailsDto
    {
        public string FacilityName { get; set; } = null!;
        public string FacilityOwnership { get; set; } = null!;
        public string FacilityManagerName { get; set; } = null!;
        public string FacilityAddress { get; set; } = null!;
        public string FacilityPhone { get; set; } = null!;
    }

    public class ParentDetailsDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Signature { get; set; } // נתוני החתימה כ-Base64
    }
}