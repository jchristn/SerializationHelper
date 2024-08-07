﻿namespace Test
{
    using System;
    using System.Collections.Specialized;
    using System.Text;
    using GetSomeInput;
    using SerializationHelper;

    public static class Program
    {
        static bool _RunForever = true;
        static Serializer _Serializer = new Serializer();

        public static void Main(string[] args)
        {
            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [?/help]:", null, false);

                switch (userInput)
                {
                    case "q":
                        _RunForever = false;
                        break;
                    case "?":
                        Menu();
                        break;
                    case "c":
                    case "cls":
                        Console.Clear();
                        break;
                    case "se":
                        Serialize();
                        break;
                    case "de":
                        Deserialize();
                        break;
                    case "ex":
                        SerializeException();
                        break;
                    case "gc":
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        break;
                }
            }
        }

        static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("    q          Quit this program");
            Console.WriteLine("    ?          Help, this menu");
            Console.WriteLine("    cls        Clear the screen");
            Console.WriteLine("    se         Serialize");
            Console.WriteLine("    de         Deserialize");
            Console.WriteLine("    ex         Throw an exception and serialize it");
            Console.WriteLine("    gc         Run garbage collection");
            Console.WriteLine("");
        }

        static void Serialize()
        {
            int count = Inputty.GetInteger("Count:", 10, true, false);

            for (int i = 0; i < count; i++)
            {
                Person p = new Person
                {
                    Id = 10,
                    FirstName = "Joe",
                    LastName = "Smith",
                    Birthday = DateTime.Now,
                    Bytes = Encoding.UTF8.GetBytes("Hello, world!")
                };

                p.Attributes.Add("handsome", "true");

                Console.WriteLine(i + ": " + _Serializer.SerializeJson(p, true));
            }
        }

        static void Deserialize()
        {
            int count = Inputty.GetInteger("Count:", 5000, true, false);

            for (int i = 0; i < count; i++)
            {
                string birthday = DateTime.UtcNow.AddYears(-50).ToString(Serializer.DateTimeFormat);
                Person p = _Serializer.DeserializeJson<Person>("{\"FirstName\":\"Joe\",\"LastName\":\"Smith\",\"Birthday\":\"" + birthday + "\", \"Bytes\":\"aGVsbG8gd29ybGQ=\"}");
                Console.WriteLine(i + ": " + p.ToString());
            }
        }

        static void SerializeException()
        {
            try
            {
                throw new DivideByZeroException();
            }
            catch (Exception e)
            {
                e.Data.Add("Hello", "World");
                Console.WriteLine(_Serializer.SerializeJson(e, true));
            }
        }
    }

    public class Person
    {
        public int Id { get; set; } = 0;
        public string FirstName { get; set; } = "Joe";
        public string LastName { get; set; } = "Smith";
        public DateTime Birthday { get; set; } = DateTime.UtcNow.AddYears(-40);
        public NameValueCollection Attributes { get; set; } = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
        public byte[] Bytes { get; set; } = Encoding.UTF8.GetBytes("hi");

        public Person()
        {

        }

        public override string ToString()
        {
            return FirstName + " " + LastName + " " + Birthday.ToString() + " " + Encoding.UTF8.GetString(Bytes);
        }
    }
}