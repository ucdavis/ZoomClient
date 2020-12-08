using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZoomClient.JSON
{
    public class ZListConverter : JsonConverter
    {
        /// <summary>
        /// List of property names that can be mapped to the Results property of a ZList
        /// </summary>
        private readonly List<string> _resultsProperties = new List<string>
        {
            "users", "meetings", "participants"
        };

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsClass;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object instance = Activator.CreateInstance(objectType);
            var props = objectType.GetTypeInfo().DeclaredProperties.ToList();

            JObject jo = JObject.Load(reader);
            foreach (JProperty jp in jo.Properties())
            {
                var propname = jp.Name;
                if (_resultsProperties.Contains(propname))
                {
                    propname = "Results";
                }

                PropertyInfo prop = props.FirstOrDefault(pi => pi.CanWrite && pi.Name == propname);

                prop?.SetValue(instance, jp.Value.ToObject(prop.PropertyType, serializer));
            }

            return instance;
        }
    }
}
