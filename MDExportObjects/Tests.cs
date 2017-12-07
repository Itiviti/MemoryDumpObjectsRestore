using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Text.RegularExpressions;
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

        string file = @"C:\Users\adrian.rus\Documents\Visual Studio 2015\Projects\MemDumpBrowser\MDExportObjects\SampleProject.dmp";
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

        [Test]
        public void Tests1()
        {

            var t2 = typeof(System.Collections.Generic.List<System.String>);
            
            // arrange
            var t1 = "System.Collections.Generic.List<System.String>"
                .Replace("<",
                "`1[").Replace(">", "]");
            var type = Type.GetType(t1);
            //act

            //assert
            Assert.NotNull(type);
        }
    }
}
