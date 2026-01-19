using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Tools.DataReader
{
    public class CSVUtils
    {
        public static List<T> ReadCSV<T>(string filePath) where T : new()
        {
            var result = new List<T>();

            if (!File.Exists(filePath))
            {
                Debug.LogError($"CSV file not found at path: {filePath}");
                return result;
            }

            var lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
            if (lines.Length <= 1)
            {
                Debug.LogWarning("CSV file is empty or missing header");
                return result;
            }

            var headers = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                var row = lines[i].Split(',');
                if (row.Length == 0 || string.IsNullOrWhiteSpace(row[0])) continue;

                T obj = new T();
                for (int j = 0; j < headers.Length && j < row.Length; j++)
                {
                    string header = headers[j].Trim();
                    string value = row[j].Trim();

                    FieldInfo field = typeof(T).GetField(header);
                    PropertyInfo property = typeof(T).GetProperty(header);

                    if (field != null)
                    {
                        object convertedValue = ConvertToType(value, field.FieldType);
                        field.SetValue(obj, convertedValue);
                    }
                    else if (property != null && property.CanWrite)
                    {
                        object convertedValue = ConvertToType(value, property.PropertyType);
                        property.SetValue(obj, convertedValue);
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        private static object ConvertToType(string value, Type targetType)
        {
            if (targetType.IsEnum)  // 如果目标类型是枚举类型
            {
                // 尝试将字符串转换为对应的枚举值
                return Enum.Parse(targetType, value);
            }

            // 对于其他类型，继续使用 Convert.ChangeType
            return Convert.ChangeType(value, targetType);
        }
    }
}
