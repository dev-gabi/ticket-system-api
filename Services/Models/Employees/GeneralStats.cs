namespace Services.Models.Employees
{
    public class GeneralMonthlyStats
    {
        public int TotalClosedTickets { get; set; }
        public int ClosedTicketsThatWereOpenThisMonth { get; set; }
        public int TotalReplies { get; set; }
        public int OpenedTickets { get; set; }
        public TopEmployeesPerformance TopPerformance { get; set; }
    }
}
