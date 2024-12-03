namespace StudentApi.Helpers
{
    public class SuccessResponse
    {
        public int Status  { get; set; }
        public string Message { get; set; }
        public Object? Data { get; set; }

        public SuccessResponse(int status, string message, Object? data)
        {
            Status = status;
            Message = message;
            Data = data;
        }
    }
}