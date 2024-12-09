namespace CourseApi.Helpers;
public class ErrorResponse
{
    public int Status { get; set; }
    public string? Message { get; set; }
    public Object? Error { get; set; }
    public ErrorResponse(int status, string? message, Object? error)
    {
        Status = status;
        Message = message;
        Error = error;
    }
}