using Newtonsoft.Json;

namespace ArtTest.Utilities
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj
            , Formatting formatting = Formatting.Indented
            , JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };
            }

            var returnValue = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            return returnValue;
        }

        public static T FromJson<T>(this string json
            , JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };
            }

            var returnValue = JsonConvert.DeserializeObject<T>(json, settings);
            return returnValue;
        }
    }
}
