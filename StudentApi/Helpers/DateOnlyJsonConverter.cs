using System.Globalization;
using Newtonsoft.Json;

namespace StudentApi.Helpers;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "dd-MM-yyyy";

    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
        {
            return default;
        }

        string? dateString = reader.Value.ToString();
        if (string.IsNullOrEmpty(dateString))
        {
            return default;
        }

        if (DateOnly.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result))
        {
            return result;
        }

        if (DateTime.TryParse(dateString, out DateTime dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        throw new JsonSerializationException($"Unable to parse '{dateString}' as DateOnly.");
    }

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(DateFormat));
    }
}