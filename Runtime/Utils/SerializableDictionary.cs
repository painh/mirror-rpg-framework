using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirrorRPG.Utils
{
    /// <summary>
    /// Unity Inspector에서 편집 가능한 Dictionary
    /// JSON 직렬화도 지원 (Newtonsoft.Json 사용 시)
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in this)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            int count = Mathf.Min(_keys.Count, _values.Count);
            for (int i = 0; i < count; i++)
            {
                if (_keys[i] != null && !ContainsKey(_keys[i]))
                {
                    Add(_keys[i], _values[i]);
                }
            }
        }
    }

    // ============================================
    // 자주 쓰는 타입 미리 정의 (Unity 제네릭 직렬화 제약)
    // ============================================

    [Serializable] public class StringIntDictionary : SerializableDictionary<string, int> { }
    [Serializable] public class StringFloatDictionary : SerializableDictionary<string, float> { }
    [Serializable] public class StringStringDictionary : SerializableDictionary<string, string> { }
    [Serializable] public class StringBoolDictionary : SerializableDictionary<string, bool> { }
    [Serializable] public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> { }
    [Serializable] public class StringSpriteDictionary : SerializableDictionary<string, Sprite> { }
    [Serializable] public class StringAudioClipDictionary : SerializableDictionary<string, AudioClip> { }
    [Serializable] public class IntStringDictionary : SerializableDictionary<int, string> { }
    [Serializable] public class IntGameObjectDictionary : SerializableDictionary<int, GameObject> { }
}
