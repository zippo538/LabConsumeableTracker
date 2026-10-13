namespace LabConsumableExpiryTracker.Application.Common.Result;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Kode error mesin-baca (mis. "LOT_NOT_FOUND"). Controller memakainya untuk
    /// memetakan status HTTP; null = error generik.
    /// </summary>
    public string? Code { get; set; }

    public static ServiceResult<T> SuccessResult(T data, string message = "Success")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ServiceResult<T> ErrorResult(string message, List<string>? errors = null, string? code = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? [],
            Code = code
        };
    }

}