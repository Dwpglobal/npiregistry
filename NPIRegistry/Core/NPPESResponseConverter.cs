using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dwp.NPPES.Core
{
    internal class NPPESResponseConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NPPESResponse);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
           
            object instance = objectType.GetConstructor(Type.EmptyTypes).Invoke(null);

            // create JObject for future parsing
            var jObject = JObject.Load(reader);

            // create reader to populate the object
            using (var jReader = jObject.CreateReader())
            {
                jReader.Culture = reader.Culture;
                jReader.SupportMultipleContent = reader.SupportMultipleContent;
                jReader.FloatParseHandling = reader.FloatParseHandling;
                jReader.DateParseHandling = reader.DateParseHandling;
                jReader.DateFormatString = reader.DateFormatString;
                jReader.MaxDepth = reader.MaxDepth;

                serializer.Populate(jReader, instance);
            }

            // now the fun part, check for either resultCount or result_count fields
            PropertyInfo[] props = objectType.GetProperties();

            foreach (var jp in jObject.Properties())
            {
                if (string.Equals(jp.Name, "resultCount", StringComparison.OrdinalIgnoreCase) || string.Equals(jp.Name, "result_count", StringComparison.OrdinalIgnoreCase))
                {
                    PropertyInfo prop = props.FirstOrDefault(pi =>
                    pi.CanWrite && string.Equals(pi.Name, nameof(NPPESResponse.ResultCount), StringComparison.OrdinalIgnoreCase));

                    if (prop != null)
                        prop.SetValue(instance, jp.Value.ToObject(prop.PropertyType, serializer));
                }
            }

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
