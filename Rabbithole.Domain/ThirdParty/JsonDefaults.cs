using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rabbithole.Domain.ThirdParty
{
    public static class JsonDefaults
    {
        // Upstream APIs mix casing and sometimes send numbers as strings
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
}
