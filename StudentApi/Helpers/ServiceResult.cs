namespace StudentApi.Helpers;
public enum ResultType
{
    Ok,
    BadRequest,
    NotFound,
    Unauthorized,
    InternalServerError
}
public class ServiceResult
{
    public ResultType Type { get; set; }
    public string? Message { get; set; }
    public Object? Data { get; set; }


    public ServiceResult(Object? data, string message)
    {
        Type = ResultType.Ok;
        Message = message;
        Data = data;
    }
    public ServiceResult(ResultType type, string message)
    {
        Type = type;
        Message = message;
        Data = default;
    }
}