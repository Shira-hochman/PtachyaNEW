namespace Dto
{
    public class DashboardStatsDto
    {
        public int TotalChildren { get; set; }
        public int ActiveGardens { get; set; }
        public int PendingForms { get; set; } // זה המספר שיתעדכן
        public int UnpaidPayments { get; set; }
    }
}