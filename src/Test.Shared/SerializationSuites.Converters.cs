namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text.Json;
    using SerializationHelper;
    using Touchstone.Core;

    /// <summary>
    /// Test suites focused on the individual JSON converters bundled with the Serializer.
    /// </summary>
    public static partial class SerializationSuites
    {
        // ==================================================================
        // DateTime converter
        // ==================================================================

        private const string DateTimeId = "DateTimeConverter";

        private static TestSuiteDescriptor DateTimeConverterSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(DateTimeId, "WritesDefaultFormat", "DateTime written using the default format", () =>
                {
                    Serializer s = NewSerializer();
                    Person p = new Person { Birthday = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc) };
                    string json = s.SerializeJson(p, false);
                    Check.Contains(json, "\"Birthday\":\"2020-01-02T03:04:05.000000Z\"");
                }),

                Case(DateTimeId, "ReadsIso8601", "DateTime parsed from ISO-8601 string", () =>
                {
                    Serializer s = NewSerializer();
                    Person p = s.DeserializeJson<Person>("{\"Birthday\":\"2020-01-02T03:04:05.000000Z\"}");
                    Check.Equal(new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), p.Birthday.ToUniversalTime());
                }),

                Case(DateTimeId, "ReadsAlternateFormat", "DateTime parsed from an alternate parseable format", () =>
                {
                    Serializer s = NewSerializer();
                    // DateTime.TryParse accepts many formats; verify a plain date works.
                    Person p = s.DeserializeJson<Person>("{\"Birthday\":\"2020-01-02\"}");
                    Check.Equal(2020, p.Birthday.Year);
                    Check.Equal(1, p.Birthday.Month);
                    Check.Equal(2, p.Birthday.Day);
                }),

                Case(DateTimeId, "InvalidValueThrowsFormatException", "Unparseable DateTime throws FormatException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<FormatException>(() => s.DeserializeJson<Person>("{\"Birthday\":\"not-a-date\"}"));
                }),

                Case(DateTimeId, "EmptyStringThrowsFormatException", "Empty DateTime string throws FormatException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<FormatException>(() => s.DeserializeJson<Person>("{\"Birthday\":\"\"}"));
                }),

                Case(DateTimeId, "CustomFormatHonored", "Custom DateTimeFormat is honored (static, restored)", () =>
                {
                    string original = Serializer.DateTimeFormat;
                    try
                    {
                        Serializer.DateTimeFormat = "yyyy-MM-dd";
                        Serializer s = NewSerializer();
                        Person p = new Person { Birthday = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc) };
                        string json = s.SerializeJson(p, false);
                        Check.Contains(json, "\"Birthday\":\"2020-01-02\"");
                    }
                    finally
                    {
                        Serializer.DateTimeFormat = original;
                    }
                })
            };

            return new TestSuiteDescriptor(DateTimeId, "DateTime Converter", cases);
        }

        // ==================================================================
        // Strict enum converter
        // ==================================================================

        private const string EnumId = "StrictEnum";

        private static TestSuiteDescriptor StrictEnumSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(EnumId, "WritesEnumName", "Enum written as its name", () =>
                {
                    Serializer s = NewSerializer();
                    string json = s.SerializeJson(new Order { Color = Color.Blue }, false);
                    Check.Contains(json, "\"Color\":\"Blue\"");
                }),

                Case(EnumId, "ReadsValidName", "Enum read from a valid name", () =>
                {
                    Serializer s = NewSerializer();
                    Order o = s.DeserializeJson<Order>("{\"Color\":\"Green\"}");
                    Check.Equal(Color.Green, o.Color);
                }),

                Case(EnumId, "ReadsNameCaseInsensitively", "Enum name is matched case-insensitively", () =>
                {
                    Serializer s = NewSerializer();
                    Order o = s.DeserializeJson<Order>("{\"Color\":\"green\"}");
                    Check.Equal(Color.Green, o.Color);
                }),

                Case(EnumId, "ReadsValidInteger", "Enum read from a valid (defined) integer", () =>
                {
                    Serializer s = NewSerializer();
                    Order o = s.DeserializeJson<Order>("{\"Status\":404}");
                    Check.Equal(StatusCode.NotFound, o.Status);
                }),

                Case(EnumId, "ReadsZeroValueInteger", "Enum read from the defined zero value", () =>
                {
                    Serializer s = NewSerializer();
                    Order o = s.DeserializeJson<Order>("{\"Color\":0}");
                    Check.Equal(Color.Red, o.Color);
                }),

                Case(EnumId, "InvalidNameThrows", "Invalid enum name throws JsonException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<JsonException>(() => s.DeserializeJson<Order>("{\"Color\":\"Purple\"}"));
                }),

                Case(EnumId, "UndefinedIntegerThrows", "Undefined integer value throws JsonException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<JsonException>(() => s.DeserializeJson<Order>("{\"Color\":5}"));
                }),

                Case(EnumId, "UndefinedStatusIntegerThrows", "Undefined non-contiguous integer throws JsonException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<JsonException>(() => s.DeserializeJson<Order>("{\"Status\":999}"));
                }),

                Case(EnumId, "BooleanTokenThrows", "Non string/number token for enum throws JsonException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<JsonException>(() => s.DeserializeJson<Order>("{\"Color\":true}"));
                }),

                Case(EnumId, "FactoryCanConvertEnum", "StrictEnumConverterFactory.CanConvert(enum) is true", () =>
                {
                    Serializer.StrictEnumConverterFactory factory = new Serializer.StrictEnumConverterFactory();
                    Check.True(factory.CanConvert(typeof(Color)));
                }),

                Case(EnumId, "FactoryRejectsNonEnum", "StrictEnumConverterFactory.CanConvert(non-enum) is false", () =>
                {
                    Serializer.StrictEnumConverterFactory factory = new Serializer.StrictEnumConverterFactory();
                    Check.False(factory.CanConvert(typeof(int)));
                }),

                Case(EnumId, "FactoryCreatesConverter", "StrictEnumConverterFactory creates a converter", () =>
                {
                    Serializer.StrictEnumConverterFactory factory = new Serializer.StrictEnumConverterFactory();
                    System.Text.Json.Serialization.JsonConverter converter =
                        factory.CreateConverter(typeof(Color), new JsonSerializerOptions());
                    Check.NotNull(converter);
                })
            };

            return new TestSuiteDescriptor(EnumId, "Strict Enum Converter", cases);
        }

        // ==================================================================
        // NameValueCollection converter
        // ==================================================================

        private const string NvcId = "NameValueCollection";

        private static TestSuiteDescriptor NameValueCollectionSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(NvcId, "WritesSingleValue", "Single NameValueCollection value written", () =>
                {
                    Serializer s = NewSerializer();
                    Person p = new Person();
                    p.Attributes.Add("role", "admin");
                    string json = s.SerializeJson(p, false);
                    Check.Contains(json, "\"role\":\"admin\"");
                }),

                Case(NvcId, "WritesMultipleValuesJoined", "Multiple values joined with comma-space", () =>
                {
                    Serializer s = NewSerializer();
                    Person p = new Person();
                    p.Attributes.Add("tag", "a");
                    p.Attributes.Add("tag", "b");
                    string json = s.SerializeJson(p, false);
                    Check.Contains(json, "\"tag\":\"a, b\"");
                }),

                Case(NvcId, "ReadsObjectValue", "NameValueCollection read from a JSON object", () =>
                {
                    Serializer s = NewSerializer();
                    Person p = s.DeserializeJson<Person>("{\"Attributes\":{\"role\":\"admin\"}}");
                    Check.Equal("admin", p.Attributes["role"]);
                }),

                Case(NvcId, "SplitsCommaSeparatedValues", "Comma-separated value is split into multiple entries", () =>
                {
                    Serializer s = NewSerializer();
                    Person p = s.DeserializeJson<Person>("{\"Attributes\":{\"tag\":\"a,b,c\"}}");
                    string[] values = p.Attributes.GetValues("tag");
                    Check.NotNull(values);
                    Check.Equal(3, values.Length);
                }),

                Case(NvcId, "MultiValueRoundTrips", "Multi-valued entry survives a serialize/deserialize round trip", () =>
                {
                    Serializer s = NewSerializer();
                    Person original = new Person();
                    original.Attributes.Add("tag", "a");
                    original.Attributes.Add("tag", "b");

                    // Write joins as "a, b"; Read splits on comma and trims back to individual values.
                    Person copy = s.DeserializeJson<Person>(s.SerializeJson(original, false));
                    string[] values = copy.Attributes.GetValues("tag");
                    Check.NotNull(values);
                    Check.Equal(2, values.Length);
                    Check.True(new List<string>(values).Contains("a"), "Expected value 'a' after round trip.");
                    Check.True(new List<string>(values).Contains("b"), "Expected value 'b' after round trip.");
                }),

                Case(NvcId, "ReadsNullValue", "Null value in NameValueCollection is accepted", () =>
                {
                    Serializer s = NewSerializer();
                    Person p = s.DeserializeJson<Person>("{\"Attributes\":{\"opt\":null}}");
                    Check.NotNull(p.Attributes);
                    Check.True(new List<string>(p.Attributes.AllKeys).Contains("opt"), "Expected key 'opt' to be present.");
                }),

                Case(NvcId, "NonObjectValueThrows", "Non-object value for NameValueCollection throws JsonException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<JsonException>(() => s.DeserializeJson<Person>("{\"Attributes\":\"notanobject\"}"));
                })
            };

            return new TestSuiteDescriptor(NvcId, "NameValueCollection Converter", cases);
        }

        // ==================================================================
        // IPAddress converter
        // ==================================================================

        private const string IpId = "IPAddress";

        private static TestSuiteDescriptor IpAddressSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(IpId, "WritesIpv4", "IPv4 address written as string", () =>
                {
                    Serializer s = NewSerializer();
                    string json = s.SerializeJson(new Endpoint { Address = IPAddress.Parse("192.168.1.1") }, false);
                    Check.Contains(json, "\"Address\":\"192.168.1.1\"");
                }),

                Case(IpId, "ReadsIpv4", "IPv4 address read from string", () =>
                {
                    Serializer s = NewSerializer();
                    Endpoint e = s.DeserializeJson<Endpoint>("{\"Address\":\"10.0.0.5\"}");
                    Check.Equal(IPAddress.Parse("10.0.0.5"), e.Address);
                }),

                Case(IpId, "RoundTripsIpv6", "IPv6 address round-trips", () =>
                {
                    Serializer s = NewSerializer();
                    Endpoint original = new Endpoint { Address = IPAddress.Parse("::1") };
                    Endpoint copy = s.DeserializeJson<Endpoint>(s.SerializeJson(original, false));
                    Check.Equal(IPAddress.Parse("::1"), copy.Address);
                }),

                Case(IpId, "InvalidAddressThrows", "Invalid IP address string throws FormatException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<FormatException>(() => s.DeserializeJson<Endpoint>("{\"Address\":\"not-an-ip\"}"));
                })
            };

            return new TestSuiteDescriptor(IpId, "IPAddress Converter", cases);
        }

        // ==================================================================
        // Exception converter
        // ==================================================================

        private const string ExceptionId = "ExceptionConverter";

        private static TestSuiteDescriptor ExceptionSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(ExceptionId, "SerializesMessage", "Exception serialization includes the message", () =>
                {
                    Serializer s = NewSerializer();
                    string json;
                    try
                    {
                        throw new InvalidOperationException("boom");
                    }
                    catch (Exception e)
                    {
                        json = s.SerializeJson(e, false);
                    }
                    Check.NotNull(json);
                    Check.Contains(json, "\"Message\"");
                    Check.Contains(json, "boom");
                }),

                Case(ExceptionId, "OmitsTargetSite", "Exception serialization omits TargetSite", () =>
                {
                    Serializer s = NewSerializer();
                    string json;
                    try
                    {
                        throw new InvalidOperationException("boom");
                    }
                    catch (Exception e)
                    {
                        json = s.SerializeJson(e, false);
                    }
                    Check.DoesNotContain(json, "TargetSite");
                }),

                Case(ExceptionId, "DeserializeThrowsNotSupported", "Deserializing an exception throws NotSupportedException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<NotSupportedException>(() => s.DeserializeJson<Exception>("{\"Message\":\"x\"}"));
                }),

                Case(ExceptionId, "OmitsNullPropertiesByDefault", "Null exception properties (InnerException) omitted by default", () =>
                {
                    Serializer s = NewSerializer();
                    // Default IncludeNullProperties == false, so the converter drops null-valued members.
                    string json;
                    try
                    {
                        throw new InvalidOperationException("boom");
                    }
                    catch (Exception e)
                    {
                        json = s.SerializeJson(e, false);
                    }
                    Check.DoesNotContain(json, "InnerException");
                }),

                Case(ExceptionId, "IncludesNullPropertiesWhenEnabled", "Null exception properties included when IncludeNullProperties is true", () =>
                {
                    Serializer s = NewSerializer();
                    s.IncludeNullProperties = true;
                    string json;
                    try
                    {
                        throw new InvalidOperationException("boom");
                    }
                    catch (Exception e)
                    {
                        json = s.SerializeJson(e, false);
                    }
                    // InnerException is null here; with null properties enabled it is written explicitly.
                    Check.Contains(json, "\"InnerException\":null");
                })
            };

            return new TestSuiteDescriptor(ExceptionId, "Exception Converter", cases);
        }

        // ==================================================================
        // IntPtr converter (not in default set; exercised directly)
        // ==================================================================

        private const string IntPtrId = "IntPtrConverter";

        private static TestSuiteDescriptor IntPtrSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(IntPtrId, "WritesValueAsString", "IntPtr written as a string", () =>
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.Converters.Add(new Serializer.IntPtrConverter());
                    string json = JsonSerializer.Serialize(new IntPtr(42), options);
                    Check.Equal("\"42\"", json);
                }),

                Case(IntPtrId, "WritesInsideObject", "IntPtr written inside an object", () =>
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.Converters.Add(new Serializer.IntPtrConverter());
                    string json = JsonSerializer.Serialize(new HandleHolder { Handle = new IntPtr(7) }, options);
                    Check.Contains(json, "\"Handle\":\"7\"");
                }),

                Case(IntPtrId, "ReadThrows", "Deserializing IntPtr throws", () =>
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.Converters.Add(new Serializer.IntPtrConverter());
                    Exception ex = Check.ThrowsAny(() => JsonSerializer.Deserialize<IntPtr>("\"42\"", options));
                    // The converter throws InvalidOperationException; System.Text.Json may surface it directly.
                    Check.True(
                        ex is InvalidOperationException || ex.InnerException is InvalidOperationException,
                        "Expected an InvalidOperationException from the IntPtr converter, got: " + ex.GetType().Name);
                })
            };

            return new TestSuiteDescriptor(IntPtrId, "IntPtr Converter", cases);
        }
    }
}
