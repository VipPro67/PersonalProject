namespace ApiWebApp.Helpers;

public class ErrorResponse
{
    public int Status { get; set; }
    public string Message { get; set; }
    public string MessageDetail { get; set; }

    public ErrorResponse(int status, string message, string messageDetail)
    {
        Status = status;
        Message = message;
        MessageDetail = messageDetail;
    }
}
