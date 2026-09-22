using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LabConsumableExpiryTracker.Application.Validators.Helpers
{
    public class SafeGuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && Guid.TryParse(reader.GetString(), out var guid))
            {
                return guid;
            }

            throw new JsonException("Format GUID tidak valid.");
        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}