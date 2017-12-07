using System;
using System.Collections.Generic;

namespace SampleProject
{
    internal class Program
    {
        static SimpleFieldsType _simpleFileds = new SimpleFieldsType("arus", 99, eType.PlainWeird);
        static SimpleArrayType _simpleArray = new SimpleArrayType(new[] { "one", "two" }, new[] { 1, 2 });
        static SimpleListType _simpleListType = new SimpleListType(new List<string>() { "two", "three" });

        private static void Main(string[] args)
        {
            Console.ReadKey();
        }
    }

    public class SimpleFieldsType
    {
        private readonly eType type;
        private readonly int age;
        private readonly string name;

        public SimpleFieldsType(string name, int age, eType type)
        {
            this.name = name;
            this.age = age;
            this.type = type;
        }


        public string Name
        {
            get
            {
                return name;
            }
        }

        public int Age
        {
            get
            {
                return age;
            }
        }

        public eType Type
        {
            get
            {
                return type;
            }
        }
    }

    public class SimpleEnum
    {
        private readonly int age;
        private readonly string name;

        public SimpleEnum(string name, int age)
        {
            this.name = name;
            this.age = age;
        }

        public int Age
        {
            get
            {
                return age;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
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
}