﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Common.Base
{
    public static class JsonExtension
    {
        public static void AddValue(this JsonObject json, string name, object value)
        {
            if (json == null)
                throw new ArgumentNullException("Json object can not be null.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Name parameter can not be null or empty.");
            json.Add(name, EvaluateJsonValue(value));
        }

        public static IJsonValue EvaluateJsonValue(object value)
        {
            if (value == null)
                return JsonValue.CreateNullValue();
            else if (value is string)
                return JsonValue.CreateStringValue(value as string);
            else if (value is bool)
                return JsonValue.CreateBooleanValue((bool)value);
            else if (value is int || value is double || value is float || value is long ||
                value is sbyte || value is short || value is uint || value is ulong || value is ushort)
                return JsonValue.CreateNumberValue((double)value);
            else if (value is IList)
            {
                JsonArray array = new JsonArray();
                foreach(object item in (value as IList))
                    array.Add(EvaluateJsonValue(item));
                return array;
            }
            else
                return JsonValue.CreateNullValue();
        }

        public static string GetValueString(this IJsonValue value)
        {
            StringBuilder builder;
            switch (value.ValueType)
            {
                case JsonValueType.String:
                    return value.GetString();
                case JsonValueType.Boolean:
                    return value.GetBoolean().ToString();
                case JsonValueType.Number:
                    return value.GetNumber().ToString();
                case JsonValueType.Array:
                    builder = new StringBuilder();
                    foreach (IJsonValue item in value.GetArray())
                    {
                        builder.AppendLine(GetValueString(item));
                    }
                    return builder.ToString();
                case JsonValueType.Object:
                    builder = new StringBuilder();
                    foreach (IJsonValue item in value.GetObject().Values)
                    {
                        builder.AppendLine(GetValueString(item));
                    }
                    return builder.ToString();
                case JsonValueType.Null:
                default:
                    return null;
            }
        }
    }
}
