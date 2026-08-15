namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Enumeration used to exercise the strict enum converter.
    /// </summary>
    public enum Color
    {
        /// <summary>Red.</summary>
        Red = 0,
        /// <summary>Green.</summary>
        Green = 1,
        /// <summary>Blue.</summary>
        Blue = 2
    }

    /// <summary>
    /// Enumeration with non-contiguous values used to exercise strict integer handling.
    /// </summary>
    public enum StatusCode
    {
        /// <summary>Ok.</summary>
        Ok = 200,
        /// <summary>NotFound.</summary>
        NotFound = 404,
        /// <summary>ServerError.</summary>
        ServerError = 500
    }

    /// <summary>
    /// Simple person model used across serialization tests.
    /// </summary>
    public class Person
    {
        /// <summary>Identifier.</summary>
        public int Id { get; set; } = 0;

        /// <summary>First name.</summary>
        public string FirstName { get; set; } = "Joe";

        /// <summary>Last name.</summary>
        public string LastName { get; set; } = "Smith";

        /// <summary>Birthday.</summary>
        public DateTime Birthday { get; set; } = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>Attributes.</summary>
        public NameValueCollection Attributes { get; set; } = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>Bytes.</summary>
        public byte[] Bytes { get; set; } = Encoding.UTF8.GetBytes("hi");

        /// <summary>Instantiate.</summary>
        public Person()
        {
        }
    }

    /// <summary>
    /// Model with a nullable/optional string used to exercise null-property handling.
    /// </summary>
    public class Widget
    {
        /// <summary>Name.</summary>
        public string Name { get; set; }

        /// <summary>Optional description (frequently null).</summary>
        public string Description { get; set; }

        /// <summary>Count.</summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Model that nests another object and various collections.
    /// </summary>
    public class Order
    {
        /// <summary>Order identifier.</summary>
        public Guid OrderId { get; set; }

        /// <summary>Buyer.</summary>
        public Person Buyer { get; set; }

        /// <summary>Line items.</summary>
        public List<string> Items { get; set; } = new List<string>();

        /// <summary>Quantities keyed by item.</summary>
        public Dictionary<string, int> Quantities { get; set; } = new Dictionary<string, int>();

        /// <summary>Status.</summary>
        public StatusCode Status { get; set; } = StatusCode.Ok;

        /// <summary>Color preference.</summary>
        public Color Color { get; set; } = Color.Red;
    }

    /// <summary>
    /// Model containing an IPAddress to exercise the IP address converter.
    /// </summary>
    public class Endpoint
    {
        /// <summary>Host name.</summary>
        public string Host { get; set; }

        /// <summary>Address.</summary>
        public IPAddress Address { get; set; }

        /// <summary>Port.</summary>
        public int Port { get; set; }
    }

    /// <summary>
    /// Model containing an IntPtr to exercise the IntPtr converter directly.
    /// </summary>
    public class HandleHolder
    {
        /// <summary>Handle.</summary>
        public IntPtr Handle { get; set; }
    }
}
