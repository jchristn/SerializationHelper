namespace SerializationHelper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// JSON serializer.
    /// </summary>
    public class Serializer
    {
        #region Public-Members

        /// <summary>
        /// DateTime format.
        /// </summary>
        public static string DateTimeFormat
        {
            get
            {
                return _DateTimeFormat;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(DateTimeFormat));
                _DateTimeFormat = value;
            }
        }

        /// <summary>
        /// True to include null properties when serializing, false to not include null properties when serializing.
        /// </summary>
        public bool IncludeNullProperties { get; set; } = false;

        /// <summary>
        /// Default serializer options.
        /// </summary>
        public JsonSerializerOptions DefaultOptions
        {
            get
            {
                return _DefaultOptions;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(DefaultOptions));
                _DefaultOptions = value;
            }
        }

        /// <summary>
        /// Default converters.
        /// </summary>
        public List<JsonConverter> DefaultConverters
        {
            get
            {
                return _DefaultConverters;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(DefaultConverters));
                _DefaultConverters = value;
            }
        }

        #endregion

        #region Private-Members

        private static string _DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

        private JsonSerializerOptions _DefaultOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private List<JsonConverter> _DefaultConverters = new List<JsonConverter>
        {
            new ExceptionConverter<Exception>(),
            new NameValueCollectionConverter(),
            new JsonStringEnumConverter(),
            new DateTimeConverter(),
            new IPAddressConverter()
        };

        #endregion

        #region Constructors-and-Factories

        #endregion

        #region Public-Methods

        /// <summary>
        /// Deserialize JSON to an instance.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="json">JSON bytes.</param>
        /// <returns>Instance.</returns>
        public T DeserializeJson<T>(byte[] json)
        {
            return DeserializeJson<T>(Encoding.UTF8.GetString(json));
        }

        /// <summary>
        /// Deserialize JSON to an instance.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>Instance.</returns>
        public T DeserializeJson<T>(string json)
        {
            JsonSerializerOptions options = new JsonSerializerOptions(_DefaultOptions);

            foreach (JsonConverter converter in _DefaultConverters)
            {
                options.Converters.Add(converter);
            }

            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Serialize object to JSON.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="pretty">Pretty print.</param>
        /// <returns>JSON.</returns>
        public string SerializeJson(object obj, bool pretty = true)
        {
            if (obj == null) return null;

            JsonSerializerOptions options = new JsonSerializerOptions(_DefaultOptions);

            foreach (JsonConverter converter in _DefaultConverters)
            {
                options.Converters.Add(converter);
            }

            if (!IncludeNullProperties) options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            if (!pretty)
            {
                options.WriteIndented = false;
                string json = JsonSerializer.Serialize(obj, options);
                options = null;
                return json;
            }
            else
            {
                options.WriteIndented = true;
                string json = JsonSerializer.Serialize(obj, options);
                options = null;
                return json;
            }
        }

        /// <summary>
        /// Copy an object.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="o">Object.</param>
        /// <returns>Instance.</returns>
        public T CopyObject<T>(object o)
        {
            if (o == null) return default(T);
            string json = SerializeJson(o, false);
            T ret = DeserializeJson<T>(json);
            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion

        #region Public-Classes

        /// <summary>
        /// Exception converter.
        /// </summary>
        /// <typeparam name="TExceptionType">Exception type.</typeparam>
        public class ExceptionConverter<TExceptionType> : JsonConverter<TExceptionType>
        {
            /// <summary>
            /// Can convert.
            /// </summary>
            /// <param name="typeToConvert">Type to convert.</param>
            /// <returns>Boolean.</returns>
            public override bool CanConvert(Type typeToConvert)
            {
                return typeof(Exception).IsAssignableFrom(typeToConvert);
            }

            /// <summary>
            /// Read.
            /// </summary>
            /// <param name="reader">Reader.</param>
            /// <param name="typeToConvert">Type to convert.</param>
            /// <param name="options">Options.</param>
            /// <returns>TExceptionType.</returns>
            public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException("Deserializing exceptions is not allowed");
            }

            /// <summary>
            /// Write.
            /// </summary>
            /// <param name="writer">Writer.</param>
            /// <param name="value">Value.</param>
            /// <param name="options">Options.</param>
            public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
            {
                var serializableProperties = value.GetType()
                    .GetProperties()
                    .Select(uu => new { uu.Name, Value = uu.GetValue(value) })
                    .Where(uu => uu.Name != nameof(Exception.TargetSite));

                if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
                {
                    serializableProperties = serializableProperties.Where(uu => uu.Value != null);
                }

                var propList = serializableProperties.ToList();

                if (propList.Count == 0)
                {
                    // Nothing to write
                    return;
                }

                writer.WriteStartObject();

                foreach (var prop in propList)
                {
                    writer.WritePropertyName(prop.Name);
                    JsonSerializer.Serialize(writer, prop.Value, options);
                }

                writer.WriteEndObject();
            }
        }

        /// <summary>
        /// Name value collection converter.
        /// </summary>
        public class NameValueCollectionConverter : JsonConverter<NameValueCollection>
        {
            /// <summary>
            /// Read.
            /// </summary>
            /// <param name="reader">Reader.</param>
            /// <param name="typeToConvert">Type to convert.</param>
            /// <param name="options">Options.</param>
            /// <returns>NameValueCollection.</returns>
            public override NameValueCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

            /// <summary>
            /// Write.
            /// </summary>
            /// <param name="writer">Writer.</param>
            /// <param name="value">Value.</param>
            /// <param name="options">Options.</param>
            public override void Write(Utf8JsonWriter writer, NameValueCollection value, JsonSerializerOptions options)
            {
                if (value != null)
                {
                    Dictionary<string, string> val = new Dictionary<string, string>();

                    for (int i = 0; i < value.AllKeys.Count(); i++)
                    {
                        string key = value.Keys[i];
                        string[] values = value.GetValues(key);
                        string formattedValue = null;

                        if (values != null && values.Length > 0)
                        {
                            int added = 0;

                            for (int j = 0; j < values.Length; j++)
                            {
                                if (!String.IsNullOrEmpty(values[j]))
                                {
                                    if (added == 0) formattedValue += values[j];
                                    else formattedValue += ", " + values[j];
                                }

                                added++;
                            }
                        }

                        val.Add(key, formattedValue);
                    }

                    System.Text.Json.JsonSerializer.Serialize(writer, val);
                }
            }
        }

        /// <summary>
        /// DateTime converter.
        /// </summary>
        public class DateTimeConverter : JsonConverter<DateTime>
        {
            /// <summary>
            /// Read.
            /// </summary>
            /// <param name="reader">Reader.</param>
            /// <param name="typeToConvert">Type to convert.</param>
            /// <param name="options">Options.</param>
            /// <returns>NameValueCollection.</returns>
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string str = reader.GetString();

                DateTime val;
                if (DateTime.TryParse(str, out val)) return val;

                throw new FormatException("The JSON value '" + str + "' could not be converted to System.DateTime.");
            }

            /// <summary>
            /// Write.
            /// </summary>
            /// <param name="writer">Writer.</param>
            /// <param name="value">Value.</param>
            /// <param name="options">Options.</param>
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(_DateTimeFormat, CultureInfo.InvariantCulture));
            }

            /// <summary>
            /// Reserved for future use.
            /// Not used because Read does a TryParse which will evaluate several formats.
            /// </summary>
            private List<string> _AcceptedFormats = new List<string>
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyy-MM-dd HH:mm:ss.ffffff",
                "yyyy-MM-ddTHH:mm:ss.ffffff",
                "yyyy-MM-ddTHH:mm:ss.fffffffK",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy hh:mm tt",
                "MM/dd/yyyy H:mm",
                "MM/dd/yyyy h:mm tt",
                "MM/dd/yyyy HH:mm:ss"
            };
        }

        /// <summary>
        /// IntPtr serializer.  IntPtr cannot be deserialized.
        /// </summary>
        public class IntPtrConverter : JsonConverter<IntPtr>
        {
            /// <summary>
            /// Read.
            /// </summary>
            /// <param name="reader">Reader.</param>
            /// <param name="typeToConvert">Type to convert.</param>
            /// <param name="options">Options.</param>
            /// <returns>NameValueCollection.</returns>
            public override IntPtr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new InvalidOperationException("Properties of type IntPtr cannot be deserialized from JSON.");
            }

            /// <summary>
            /// Write.
            /// </summary>
            /// <param name="writer">Writer.</param>
            /// <param name="value">Value.</param>
            /// <param name="options">Options.</param>
            public override void Write(Utf8JsonWriter writer, IntPtr value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        /// <summary>
        /// IP address converter.
        /// </summary>
        public class IPAddressConverter : JsonConverter<IPAddress>
        {
            /// <summary>
            /// Read.
            /// </summary>
            /// <param name="reader">Reader.</param>
            /// <param name="typeToConvert">Type to convert.</param>
            /// <param name="options">Options.</param>
            /// <returns>NameValueCollection.</returns>
            public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string str = reader.GetString();
                return IPAddress.Parse(str);
            }

            /// <summary>
            /// Write.
            /// </summary>
            /// <param name="writer">Writer.</param>
            /// <param name="value">Value.</param>
            /// <param name="options">Options.</param>
            public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        #endregion
    }
}