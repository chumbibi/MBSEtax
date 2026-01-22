namespace MBSWeb.Models.Dto
{
    public class DateRangeDto
    {
        public int CompanyId { get; set; }  

        public string? CustomerCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
