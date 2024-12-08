using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotInstrukcijeBackend.ProfilePictureSavingUtility
{
    public class ByteArrayToBase64Converter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // If you need deserialization, implement here. Otherwise, just throw NotImplementedException.
            var base64String = reader.GetString();
            return base64String != null ? Convert.FromBase64String(base64String) : null;
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                var base64String = Convert.ToBase64String(value);
                writer.WriteStringValue(base64String);
            }
        }
    }
}
