using System;
using System.Collections.Generic;
using System.Drawing;

namespace SampleProject
{
    internal class Program
    {
        private static SimpleFieldsType _simpleFileds = new SimpleFieldsType("arus", 99, eType.PlainWeird);
        private static SimpleArrayType _simpleArray = new SimpleArrayType(new[] { "one", "two" }, new[] { 1, 2 });
        private static SimpleListType _simpleListType = new SimpleListType(new List<string>() { "two", "three" });
        private static SimpleStruct _simpleStruct = new SimpleStruct("aur", 55);
        private static ObjectContainingStruct _objectWithStruct = new ObjectContainingStruct(new MyStruct(
            "arus",
            99,
            new DateTime(2000, 12, 31),
            new Point(13,18)
            ));

        private static void Main(string[] args)
        {
            Console.ReadLine();
        }
    }

    public class SimpleFieldsType
    {
        public SimpleFieldsType(string name, int age, eType type)
        {
            this.Name = name;
            this.Age = age;
            this.Type = type;
        }

        public string Name { get; }

        public int Age { get; }

        public eType Type { get; }
    }

    public struct SimpleStruct
    {
        public SimpleStruct(string name, int age)
        {
            this.Name = name;
            this.Age = age;
        }

        public int Age { get; }

        public string Name { get; }
    }

    public class ObjectContainingStruct
    {
        public ObjectContainingStruct(MyStruct data)
        {
            Data = data;
        }

        public MyStruct Data { get; }
    }

    public struct MyStruct
    {
        public MyStruct(string name, int age, DateTime timestamp, Point location)
        {
            Name = name;
            Age = age;
            Timestamp = timestamp;
            Location = location;
        }

        public string Name { get; }
        public int Age { get; }

        public Point Location { get; }

        public DateTime Timestamp { get; }
    }

    public class SimpleArrayType
    {
        public SimpleArrayType(string[] names, int[] ages)
        {
            this.Ages = ages;
            this.Names = names;
        }

        public string[] Names { get; }
        public int[] Ages { get; }
    }

    public class SimpleListType
    {
        public SimpleListType(List<string> names)
        {
            this.Names = names;
        }

        public List<string> Names { get; }
    }

    public class ClassWithSelfReference
    {
        public ClassWithSelfReference()
        {
            This = this;
        }

        public ClassWithSelfReference This { get; }
    }

    public class ClassWithObjectGraph
    {
        public ClassWithObjectGraph(MyClass innerObject)
        {
            InnerObject = innerObject;
        }

        public MyClass InnerObject { get; }

        public class MyClass
        {
            public MyClass2 InnerObj { get; }

            public MyClass(MyClass2 innerObject)
            {
                InnerObj = innerObject;
            }

            public class MyClass2
            {
                public MyClass2(string name)
                {
                    Name = name;
                }

                public string Name { get; }
            }
        }
    }
}