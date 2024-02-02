using System.Collections.Generic;
using UnityEngine;

namespace ZoboUI.Editor
{
    /// <summary>
    /// Serializable Dictionary mostly used for saving data in editor with custom property drawers
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        public List<TKey> serializedKeys = new List<TKey>();

        [SerializeField]
        public List<TValue> serializedValues = new List<TValue>();


        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();
            if (serializedKeys.Count != serializedValues.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
            for (int i = 0; i < serializedKeys.Count; i++)
                this[serializedKeys[i]] = serializedValues[i];


        }

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {

            serializedKeys.Clear();
            serializedValues.Clear();

            foreach (var kvp in this)
            {
                serializedKeys.Add(kvp.Key);
                serializedValues.Add(kvp.Value);
            }

        }









    }



}

