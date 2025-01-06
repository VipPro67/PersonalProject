namespace StudentApi.Helpers;
public enum ResultType
{
    Ok,
    BadRequest,
    NotFound,
    Unauthorized,
    InternalServerError
}
public class Pagination
{
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPage { get; set; }
    public int ItemsPerPage { get; set; }
}

public class ServiceResult<T>
{
    public ResultType Type { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public Pagination? Pagination { get; set; }

    public ServiceResult()
    {
    }

    public ServiceResult(T data, string message)
    {
        Type = ResultType.Ok;
        Message = message;
        Data = data;
    }

    public ServiceResult(T data, string message, Pagination? pagination)
    {
        Type = ResultType.Ok;
        Message = message;
        Data = data;
        Pagination = pagination;
    }

    public ServiceResult(ResultType type, string message)
    {
        Type = type;
        Message = message;
        Data = default;
    }
}
