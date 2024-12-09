using CourseApi.Models;

namespace CourseApi.Helpers;
public enum ResultType
{
    Ok,
    BadRequest,
    NotFound,
    Unauthorized,
    InternalServerError
}
public class ServiceResult<T>
{
    public ResultType Type { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }


    public ServiceResult(T? data, string message)
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