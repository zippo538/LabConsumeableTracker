namespace LabConsumableExpiryTracker.Application.Validators.Helpers;

public class LotHelperValidator
{
    public static bool NotBeBlank(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
    public static bool BeInThePastOrPresent(DateTime value, TimeProvider timeProvider)
    {
        if (value == default)
        {
            return false;
        }
        return value.ToUniversalTime() <= timeProvider.GetUtcNow().UtcDateTime;
    }
    public static bool ValidateGuid(string guidString)
    {
        return Guid.TryParse(guidString, out _);
    }
}