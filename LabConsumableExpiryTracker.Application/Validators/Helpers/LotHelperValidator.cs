namespace LabConsumableExpiryTracker.Application.Validators.Helpers;
public abstract class LotHelperValidator
{
    public static bool NotBeBlank(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
    public static bool ValidateGuid(string guidString)
    {
        return Guid.TryParse(guidString, out _);
    }
}