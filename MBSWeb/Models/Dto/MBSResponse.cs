namespace MBSWeb.Models.Dto
{
    public class MBSResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public object Data { get; set; }
    }
}
