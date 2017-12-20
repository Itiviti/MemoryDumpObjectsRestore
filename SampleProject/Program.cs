using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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

        private static ClassWithObjectGraph _objectGraph = new ClassWithObjectGraph(new ClassWithObjectGraph.MyClass(new ClassWithObjectGraph.MyClass.MyClass2("hello"), "beautiful"), "world");
        private static ClassWithSelfReference _classWithSelfReference = new ClassWithSelfReference();
        private static ClassWithSingletonReference _classWithSingletonReference = new ClassWithSingletonReference("ClassA");
        private static ClassWithSingletonReference _classWithSingletonReferenceB = new ClassWithSingletonReference("ClassB");
        private static ClassWithInterfaceField _classWithInterfaceA = new ClassWithInterfaceField(new Impl1("IntA"));
        private static ClassWithInterfaceField _classWithInterfaceB = new ClassWithInterfaceField(new Impl2("IntB"));

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

    public class Singleton
    {
        private Singleton()
        {
        }

        public static Singleton Instance = new Singleton();
    }

    public class ClassWithSingletonReference
    {
        public string Name { get; }
        public Singleton Singleton = Singleton.Instance;

        public ClassWithSingletonReference(string name)
        {
            Name = name;
        }
    }

    public interface IInterface
    {
        string Name { get; }
    }

    public class Impl1 : IInterface
    {
        public Impl1(string name)
        {
            Name = name;
        }

        public string Name
        {
            get;
        }
    }

    public class Impl2 : IInterface
    {
        public Impl2(string name)
        {
            Name = name;
        }

        public string Name
        {
            get;
        }
    }

    public class ClassWithInterfaceField
    {
        public ClassWithInterfaceField(IInterface i)
        {
            Int = i;
        }

        public IInterface Int { get; }
    }

    public class ClassWithObjectGraph
    {
        public ClassWithObjectGraph(MyClass innerObject, string stringBuilder)
        {
            InnerObject = innerObject;
            StringBuilder = new StringBuilder(stringBuilder);
        }

        public StringBuilder StringBuilder { get; }
        public MyClass InnerObject { get; }

        public class MyClass
        {
            public string Name { get; }
            public MyClass2 InnerObj { get; }

            public MyClass(MyClass2 innerObject, string name)
            {
                InnerObj = innerObject;
                Name = name;
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