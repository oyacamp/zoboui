using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace ZoboUI.Editor
{

    public class SerializableDictionaryConverter<TKey, TValue> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            bool canConvert = typeof(SerializableDictionary<TKey, TValue>).IsAssignableFrom(objectType);
            Debug.Log($"CanConvert: objectType = {objectType}, canConvert = {canConvert}");
            return canConvert;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.Log($"Starting ReadJson: objectType = {objectType}");

            if (reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.Null)
            {
                Debug.Log("ReadJson: JSON is empty or null.");
                return null;
            }

            JObject jsonObject = JObject.Load(reader);
            var dictionary = new SerializableDictionary<TKey, TValue>();
            foreach (var prop in jsonObject.Properties())
            {
                TKey key = serializer.Deserialize<TKey>(new JTokenReader(prop.Name));
                TValue value = serializer.Deserialize<TValue>(new JTokenReader(prop.Value));
                dictionary.Add(key, value);

                Debug.Log($"ReadJson: Added key = {key}, value = {value}");
            }

            Debug.Log("ReadJson: Completed successfully.");
            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Debug.Log($"Starting WriteJson: value type = {value.GetType()}");

            var dictionary = (SerializableDictionary<TKey, TValue>)value;
            JObject obj = new JObject();
            foreach (var kvp in dictionary)
            {
                obj.Add(JToken.FromObject(kvp.Key, serializer).ToString(), JToken.FromObject(kvp.Value, serializer));
                Debug.Log($"WriteJson: Writing key = {kvp.Key}, value = {kvp.Value}");
            }
            obj.WriteTo(writer);

            Debug.Log("WriteJson: Completed successfully.");
        }
    }
}