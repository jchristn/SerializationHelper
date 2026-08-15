namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using SerializationHelper;
    using Touchstone.Core;

    /// <summary>
    /// Central source of truth for all SerializationHelper tests, expressed as
    /// runner-agnostic Touchstone descriptors. The CLI runner, xUnit adapter, and
    /// NUnit adapter all consume <see cref="All"/>.
    /// </summary>
    public static partial class SerializationSuites
    {
        /// <summary>
        /// All test suites covering the SerializationHelper library.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All { get; } = BuildAll();

        private static IReadOnlyList<TestSuiteDescriptor> BuildAll()
        {
            return new List<TestSuiteDescriptor>
            {
                SerializeSuite(),
                DeserializeSuite(),
                RoundTripSuite(),
                CopyObjectSuite(),
                ConfigurationSuite(),
                DateTimeConverterSuite(),
                StrictEnumSuite(),
                NameValueCollectionSuite(),
                IpAddressSuite(),
                ExceptionSuite(),
                IntPtrSuite()
            };
        }

        // ---- Helpers -------------------------------------------------------

        /// <summary>
        /// Wrap a synchronous test body in a Touchstone test case descriptor.
        /// </summary>
        private static TestCaseDescriptor Case(string suiteId, string caseId, string displayName, Action body)
        {
            return new TestCaseDescriptor(
                suiteId,
                caseId,
                displayName,
                ct => { body(); return Task.CompletedTask; });
        }

        private static Serializer NewSerializer() => new Serializer();

        // ==================================================================
        // Serialize
        // ==================================================================

        private const string SerializeId = "Serialize";

        private static TestSuiteDescriptor SerializeSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(SerializeId, "NullObjectReturnsNull", "SerializeJson(null) returns null", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Null(s.SerializeJson(null));
                }),

                Case(SerializeId, "PrettyProducesIndentedJson", "Pretty serialization is indented", () =>
                {
                    Serializer s = NewSerializer();
                    string json = s.SerializeJson(new Widget { Name = "x", Count = 1 }, true);
                    Check.NotNull(json);
                    Check.Contains(json, "\n", "Pretty JSON should contain newlines.");
                }),

                Case(SerializeId, "CompactProducesSingleLineJson", "Compact serialization has no newlines", () =>
                {
                    Serializer s = NewSerializer();
                    string json = s.SerializeJson(new Widget { Name = "x", Count = 1 }, false);
                    Check.NotNull(json);
                    Check.DoesNotContain(json, "\n", "Compact JSON should not contain newlines.");
                }),

                Case(SerializeId, "SimplePocoContainsProperties", "Simple POCO serializes its properties", () =>
                {
                    Serializer s = NewSerializer();
                    string json = s.SerializeJson(new Person { FirstName = "Joe", LastName = "Smith" }, false);
                    Check.Contains(json, "\"FirstName\":\"Joe\"");
                    Check.Contains(json, "\"LastName\":\"Smith\"");
                }),

                Case(SerializeId, "NullPropertiesOmittedByDefault", "Null properties omitted when IncludeNullProperties is false", () =>
                {
                    Serializer s = NewSerializer();
                    Check.False(s.IncludeNullProperties, "IncludeNullProperties should default to false.");
                    string json = s.SerializeJson(new Widget { Name = "x", Description = null, Count = 1 }, false);
                    Check.DoesNotContain(json, "Description");
                }),

                Case(SerializeId, "NullPropertiesIncludedWhenEnabled", "Null properties included when IncludeNullProperties is true", () =>
                {
                    Serializer s = NewSerializer();
                    s.IncludeNullProperties = true;
                    string json = s.SerializeJson(new Widget { Name = "x", Description = null, Count = 1 }, false);
                    Check.Contains(json, "\"Description\":null");
                }),

                Case(SerializeId, "ByteArraySerializedAsBase64", "byte[] serialized as base64 string", () =>
                {
                    Serializer s = NewSerializer();
                    string json = s.SerializeJson(new Person { Bytes = Encoding.UTF8.GetBytes("hi") }, false);
                    // "hi" -> base64 "aGk="
                    Check.Contains(json, "aGk=");
                }),

                Case(SerializeId, "EnumSerializedAsName", "Enum serialized as its string name", () =>
                {
                    Serializer s = NewSerializer();
                    string json = s.SerializeJson(new Order { Color = Color.Blue }, false);
                    Check.Contains(json, "\"Color\":\"Blue\"");
                }),

                Case(SerializeId, "NestedObjectSerialized", "Nested object graph serialized", () =>
                {
                    Serializer s = NewSerializer();
                    Order o = new Order
                    {
                        OrderId = Guid.NewGuid(),
                        Buyer = new Person { FirstName = "Ann", LastName = "Lee" },
                        Items = new List<string> { "apple", "pear" }
                    };
                    o.Quantities["apple"] = 3;
                    string json = s.SerializeJson(o, false);
                    Check.Contains(json, "\"Buyer\"");
                    Check.Contains(json, "\"FirstName\":\"Ann\"");
                    Check.Contains(json, "apple");
                }),

                Case(SerializeId, "CollectionsSerialized", "Lists and dictionaries serialized", () =>
                {
                    Serializer s = NewSerializer();
                    Order o = new Order();
                    o.Items.Add("a");
                    o.Items.Add("b");
                    o.Quantities["a"] = 1;
                    string json = s.SerializeJson(o, false);
                    Check.Contains(json, "\"Items\":[\"a\",\"b\"]");
                    Check.Contains(json, "\"a\":1");
                })
            };

            return new TestSuiteDescriptor(SerializeId, "Serialization", cases);
        }

        // ==================================================================
        // Deserialize
        // ==================================================================

        private const string DeserializeId = "Deserialize";

        private static TestSuiteDescriptor DeserializeSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(DeserializeId, "SimplePocoFromString", "Deserialize simple POCO from string", () =>
                {
                    Serializer s = NewSerializer();
                    Widget w = s.DeserializeJson<Widget>("{\"Name\":\"gear\",\"Count\":7}");
                    Check.NotNull(w);
                    Check.Equal("gear", w.Name);
                    Check.Equal(7, w.Count);
                }),

                Case(DeserializeId, "FromByteArray", "Deserialize from UTF-8 byte array", () =>
                {
                    Serializer s = NewSerializer();
                    byte[] bytes = Encoding.UTF8.GetBytes("{\"Name\":\"gear\",\"Count\":7}");
                    Widget w = s.DeserializeJson<Widget>(bytes);
                    Check.NotNull(w);
                    Check.Equal("gear", w.Name);
                    Check.Equal(7, w.Count);
                }),

                Case(DeserializeId, "TrailingCommaAllowed", "Trailing commas tolerated (AllowTrailingCommas)", () =>
                {
                    Serializer s = NewSerializer();
                    Widget w = s.DeserializeJson<Widget>("{\"Name\":\"gear\",\"Count\":7,}");
                    Check.Equal(7, w.Count);
                }),

                Case(DeserializeId, "CommentsSkipped", "Comments tolerated (ReadCommentHandling.Skip)", () =>
                {
                    Serializer s = NewSerializer();
                    Widget w = s.DeserializeJson<Widget>("{ /* comment */ \"Count\": 5 // trailing\n }");
                    Check.Equal(5, w.Count);
                }),

                Case(DeserializeId, "NumberFromStringAllowed", "Number read from string (NumberHandling.AllowReadingFromString)", () =>
                {
                    Serializer s = NewSerializer();
                    Widget w = s.DeserializeJson<Widget>("{\"Count\":\"42\"}");
                    Check.Equal(42, w.Count);
                }),

                Case(DeserializeId, "NullStringThrows", "Deserialize null string throws ArgumentNullException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<ArgumentNullException>(() => s.DeserializeJson<Widget>((string)null));
                }),

                Case(DeserializeId, "NullByteArrayThrows", "Deserialize null byte[] throws ArgumentNullException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<ArgumentNullException>(() => s.DeserializeJson<Widget>((byte[])null));
                }),

                Case(DeserializeId, "EmptyStringThrows", "Deserialize empty string throws JsonException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<JsonException>(() => s.DeserializeJson<Widget>(""));
                }),

                Case(DeserializeId, "MalformedJsonThrows", "Deserialize malformed JSON throws JsonException", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<JsonException>(() => s.DeserializeJson<Widget>("{ this is not json"));
                }),

                Case(DeserializeId, "NonNumericStringForIntThrows", "Non-numeric string for int throws", () =>
                {
                    Serializer s = NewSerializer();
                    Check.ThrowsAny(() => s.DeserializeJson<Widget>("{\"Count\":\"notanumber\"}"));
                })
            };

            return new TestSuiteDescriptor(DeserializeId, "Deserialization", cases);
        }

        // ==================================================================
        // Round trip
        // ==================================================================

        private const string RoundTripId = "RoundTrip";

        private static TestSuiteDescriptor RoundTripSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(RoundTripId, "PersonPreservesScalars", "Round trip preserves scalar properties", () =>
                {
                    Serializer s = NewSerializer();
                    Person original = new Person
                    {
                        Id = 42,
                        FirstName = "Grace",
                        LastName = "Hopper",
                        Birthday = new DateTime(1906, 12, 9, 0, 0, 0, DateTimeKind.Utc)
                    };
                    Person copy = s.DeserializeJson<Person>(s.SerializeJson(original, false));
                    Check.Equal(42, copy.Id);
                    Check.Equal("Grace", copy.FirstName);
                    Check.Equal("Hopper", copy.LastName);
                    Check.Equal(original.Birthday.ToUniversalTime(), copy.Birthday.ToUniversalTime());
                }),

                Case(RoundTripId, "ByteArrayPreserved", "Round trip preserves byte[] contents", () =>
                {
                    Serializer s = NewSerializer();
                    Person original = new Person { Bytes = Encoding.UTF8.GetBytes("Hello, world!") };
                    Person copy = s.DeserializeJson<Person>(s.SerializeJson(original, false));
                    Check.Equal("Hello, world!", Encoding.UTF8.GetString(copy.Bytes));
                }),

                Case(RoundTripId, "NameValueCollectionPreserved", "Round trip preserves NameValueCollection entries", () =>
                {
                    Serializer s = NewSerializer();
                    Person original = new Person();
                    original.Attributes.Add("role", "admin");
                    Person copy = s.DeserializeJson<Person>(s.SerializeJson(original, false));
                    Check.NotNull(copy.Attributes);
                    Check.Equal("admin", copy.Attributes["role"]);
                }),

                Case(RoundTripId, "NestedGraphPreserved", "Round trip preserves nested graph and collections", () =>
                {
                    Serializer s = NewSerializer();
                    Order original = new Order
                    {
                        OrderId = Guid.NewGuid(),
                        Buyer = new Person { FirstName = "Ada", LastName = "Lovelace" },
                        Status = StatusCode.NotFound,
                        Color = Color.Green
                    };
                    original.Items.Add("book");
                    original.Quantities["book"] = 2;

                    Order copy = s.DeserializeJson<Order>(s.SerializeJson(original, false));
                    Check.Equal(original.OrderId, copy.OrderId);
                    Check.NotNull(copy.Buyer);
                    Check.Equal("Ada", copy.Buyer.FirstName);
                    Check.Equal(StatusCode.NotFound, copy.Status);
                    Check.Equal(Color.Green, copy.Color);
                    Check.Equal(1, copy.Items.Count);
                    Check.Equal("book", copy.Items[0]);
                    Check.Equal(2, copy.Quantities["book"]);
                })
            };

            return new TestSuiteDescriptor(RoundTripId, "Round Trip", cases);
        }

        // ==================================================================
        // CopyObject
        // ==================================================================

        private const string CopyId = "CopyObject";

        private static TestSuiteDescriptor CopyObjectSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(CopyId, "NullReferenceReturnsNull", "CopyObject of null reference returns null", () =>
                {
                    Serializer s = NewSerializer();
                    Person copy = s.CopyObject<Person>(null);
                    Check.Null(copy);
                }),

                Case(CopyId, "ProducesDistinctInstance", "CopyObject produces a distinct instance", () =>
                {
                    Serializer s = NewSerializer();
                    Person original = new Person { FirstName = "Alan", LastName = "Turing" };
                    Person copy = s.CopyObject<Person>(original);
                    Check.NotNull(copy);
                    Check.False(ReferenceEquals(original, copy), "Copy should be a different instance.");
                    Check.Equal("Alan", copy.FirstName);
                    Check.Equal("Turing", copy.LastName);
                }),

                Case(CopyId, "DeepCopiesNestedGraph", "CopyObject deep-copies a nested graph", () =>
                {
                    Serializer s = NewSerializer();
                    Order original = new Order { Buyer = new Person { FirstName = "Katherine" } };
                    Order copy = s.CopyObject<Order>(original);
                    Check.NotNull(copy.Buyer);
                    Check.False(ReferenceEquals(original.Buyer, copy.Buyer), "Nested object should be a different instance.");
                    Check.Equal("Katherine", copy.Buyer.FirstName);
                }),

                Case(CopyId, "ValueTypeCopied", "CopyObject copies a boxed value type", () =>
                {
                    Serializer s = NewSerializer();
                    int copy = s.CopyObject<int>(5);
                    Check.Equal(5, copy);
                })
            };

            return new TestSuiteDescriptor(CopyId, "Copy Object", cases);
        }

        // ==================================================================
        // Configuration / properties
        // ==================================================================

        private const string ConfigId = "Configuration";

        private static TestSuiteDescriptor ConfigurationSuite()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(ConfigId, "IncludeNullPropertiesDefaultsFalse", "IncludeNullProperties defaults to false", () =>
                {
                    Check.False(NewSerializer().IncludeNullProperties);
                }),

                Case(ConfigId, "DefaultOptionsNotNull", "DefaultOptions is not null by default", () =>
                {
                    Check.NotNull(NewSerializer().DefaultOptions);
                }),

                Case(ConfigId, "DefaultConvertersNotEmpty", "DefaultConverters is populated by default", () =>
                {
                    Serializer s = NewSerializer();
                    Check.NotNull(s.DefaultConverters);
                    Check.True(s.DefaultConverters.Count > 0, "Expected default converters to be present.");
                }),

                Case(ConfigId, "SetDefaultOptionsNullThrows", "Setting DefaultOptions to null throws", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<ArgumentNullException>(() => s.DefaultOptions = null);
                }),

                Case(ConfigId, "SetDefaultConvertersNullThrows", "Setting DefaultConverters to null throws", () =>
                {
                    Serializer s = NewSerializer();
                    Check.Throws<ArgumentNullException>(() => s.DefaultConverters = null);
                }),

                Case(ConfigId, "DateTimeFormatDefault", "DateTimeFormat has the expected default", () =>
                {
                    Check.Equal("yyyy-MM-ddTHH:mm:ss.ffffffZ", Serializer.DateTimeFormat);
                }),

                Case(ConfigId, "SetDateTimeFormatNullThrows", "Setting DateTimeFormat to null throws", () =>
                {
                    Check.Throws<ArgumentNullException>(() => Serializer.DateTimeFormat = null);
                }),

                Case(ConfigId, "SetDateTimeFormatEmptyThrows", "Setting DateTimeFormat to empty throws", () =>
                {
                    Check.Throws<ArgumentNullException>(() => Serializer.DateTimeFormat = "");
                })
            };

            return new TestSuiteDescriptor(ConfigId, "Configuration", cases);
        }
    }
}
