using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Microsoft.Diagnostics.Runtime;
using SampleProject;

namespace MDExportObjects
{
    internal class Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _objects  = Program.LoadDump(file).ToList();
        }

        private static Dictionary<string, FieldInfo> GetFields(Type t)
        {
            var fieldInfos = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            return fieldInfos.ToDictionary(info => info.Name);
        }

        private string file = @"C:\work\bitbucket\memorydumpbrowser\MDExportObjects\SampleProject.dmp";
        private IEnumerable<ClrObject> _objects;

        [Test]
        public void Simple_Fields()
        {
            var obj = Program.Load<SimpleFieldsType>(_objects).Single();
            var simpleType = (SimpleFieldsType)obj.RuntimeObject;

            Assert.AreEqual("arus", simpleType.Name);
            Assert.AreEqual(99, simpleType.Age);
            Assert.AreEqual(eType.PlainWeird, simpleType.Type);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Simple_Structs()
        {
            var obj = Program.Load<SimpleStruct>(_objects).Single();
            var simpleType = (SimpleStruct)obj.RuntimeObject;

            Assert.AreEqual("aur", simpleType.Name);
            Assert.AreEqual(55, simpleType.Age);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void ObjectContainingStruct()
        {
             var obj = Program.Load<ObjectContainingStruct>(_objects).Single();
             var simpleType = (ObjectContainingStruct)obj.RuntimeObject;

            Assert.AreEqual("arus", simpleType.Data.Name);
            Assert.AreEqual(99, simpleType.Data.Age);
            Assert.AreEqual(new Point(13,18), simpleType.Data.Location);
            Assert.AreEqual(new DateTime(200, 12, 31), simpleType.Data.Timestamp);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Simple_Array()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Simple_List()
        {
            var obj = Program.Load<SimpleListType>(_objects).Single();
            var simpleType = (SimpleListType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new List<string> { "two", "three" }, simpleType.Names);

            //AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        private void AssertNoErrors(Mixins.Pair obj)
        {
            CollectionAssert.IsEmpty(obj.Errors, "Errors found");
        }

        private void AssertNoWarnings(Mixins.Pair obj)
        {
            CollectionAssert.IsEmpty(obj.Warnings, "Warnings found");
        }
    }
}
