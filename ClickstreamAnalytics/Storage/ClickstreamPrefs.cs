using System;
using UnityEngine;

namespace ClickstreamAnalytics.Storage
{
    public static class ClickstreamPrefs
    {
        public const string KeyPrefix = "aws-solution/clickstream-unity/";

        public static void SaveData(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
            switch (value)
            {
                case int i:
                    PlayerPrefs.SetInt(key, i);
                    break;
                case float f:
                    PlayerPrefs.SetFloat(key, f);
                    break;
                case string s:
                    PlayerPrefs.SetString(key, s);
                    break;
            }

            PlayerPrefs.Save();
        }

        public static object GetData(string key, Type type)
        {
            if (string.IsNullOrEmpty(key) || !PlayerPrefs.HasKey(key)) return null;
            if (type == typeof(int))
            {
                return PlayerPrefs.GetInt(key);
            }

            if (type == typeof(float))
            {
                return PlayerPrefs.GetFloat(key);
            }

            if (type == typeof(string))
            {
                return PlayerPrefs.GetString(key);
            }

            PlayerPrefs.Save();
            return null;
        }

        public static void ClearData(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}