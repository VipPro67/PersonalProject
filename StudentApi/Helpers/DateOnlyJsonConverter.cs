using Newtonsoft.Json;
namespace StudentApi.Helpers
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string DateFormat = "dd-MM-yyyy";

        public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return DateOnly.ParseExact(reader.Value.ToString(), DateFormat, null);
        }

        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(DateFormat));
        }
    }
}