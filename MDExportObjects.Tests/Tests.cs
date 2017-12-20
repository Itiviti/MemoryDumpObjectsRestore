using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Microsoft.Diagnostics.Runtime;
using SampleProject;

namespace MDExportObjects
{
    public class Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _objects  = Program.LoadDump(file).ToList();
        }

        private string file = @"..\MDExportObjects\SampleProject.dmp";
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
        public void Object_Graph()
        {
            var obj = Program.Load<ClassWithObjectGraph>(_objects).Single();
            var simpleType = (ClassWithObjectGraph)obj.RuntimeObject;

            Assert.AreEqual("hello", simpleType.InnerObject.InnerObj.Name);
            Assert.AreEqual("beautiful", simpleType.InnerObject.Name);
            Assert.AreEqual("world", simpleType.StringBuilder.ToString());

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Simple_Struct()
        {
            var obj = Program.Load<SimpleStruct>(_objects).Single();
            var simpleType = (SimpleStruct)obj.RuntimeObject;

            Assert.AreEqual("aur", simpleType.Name);
            Assert.AreEqual(55, simpleType.Age);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Object_Containing_Struct()
        {
             var obj = Program.Load<ObjectContainingStruct>(_objects).Single();
             var simpleType = (ObjectContainingStruct)obj.RuntimeObject;

            Assert.AreEqual("arus", simpleType.Data.Name);
            Assert.AreEqual(99, simpleType.Data.Age);
            Assert.AreEqual(new Point(13,18), simpleType.Data.Location);
            Assert.AreEqual(new DateTime(2000, 12, 31), simpleType.Data.Timestamp);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Object_With_Self_Reference()
        {
            var obj = Program.Load<ClassWithSelfReference>(_objects).Single();
            var selfReference = (ClassWithSelfReference)obj.RuntimeObject;

            Assert.AreSame(selfReference, selfReference.This);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Objects_With_Singleton_Reference()
        {
            var objs = Program.Load<ClassWithSingletonReference>(_objects).ToArray();
            var objA = (ClassWithSingletonReference)objs[0].RuntimeObject;
            var objB = (ClassWithSingletonReference)objs[1].RuntimeObject;

            Assert.AreSame(objA.Singleton, objB.Singleton);

            AssertNoWarnings(objs[0]);
            AssertNoErrors(objs[0]);
            AssertNoWarnings(objs[1]);
            AssertNoErrors(objs[1]);
        }

        [Test]
        public void Object_With_Polymorphic_Interface_Field()
        {
            Mixins.Pair[] objs = Program.Load<ClassWithInterfaceField>(_objects).ToArray();

            Assert.AreEqual(2, objs.Length);
            var objA = (ClassWithInterfaceField)objs[0].RuntimeObject;
            var objB = (ClassWithInterfaceField)objs[1].RuntimeObject;

            Assert.IsNotNull(objA.Int);
            Assert.IsNotEmpty(objA.Int.Name);
            Assert.IsNotNull(objB.Int);
            Assert.IsNotEmpty(objB.Int.Name);
            Assert.AreNotEqual(objA.Int.Name, objB.Int.Name);

            AssertNoWarnings(objs[0]);
            AssertNoErrors(objs[0]);
            AssertNoWarnings(objs[1]);
            AssertNoErrors(objs[1]);
        }

        [Test]
        public void Simple_Array_String_Int()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void Array_ObjectReferences()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void Array_Structs()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void Array_Interface()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void Simple_Dictionary()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void Dictionary_Objects()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void Dictionary_Objects_Generics()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void List_Objects()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void GenericObject_One()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void GenericObject_Two()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test, Ignore("TODO")]
        public void GenericObject_Three()
        {
            var obj = Program.Load<SimpleArrayType>(_objects).Single();
            var simpleType = (SimpleArrayType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new[] { "one", "two" }, simpleType.Names);
            CollectionAssert.AreEqual(new[] { 1, 2 }, simpleType.Ages);

            AssertNoWarnings(obj);
            AssertNoErrors(obj);
        }

        [Test]
        public void Simple_List_String()
        {
            var obj = Program.Load<SimpleListType>(_objects).Single();
            var simpleType = (SimpleListType)obj.RuntimeObject;

            CollectionAssert.AreEqual(new List<string> { "two", "three" }, simpleType.Names);

            AssertNoWarnings(obj);
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
