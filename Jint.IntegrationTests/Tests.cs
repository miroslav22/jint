using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using NUnit.Framework;

namespace Jint.IntegrationTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CallInstanceMethod_CorrectCase_ResultCorrect()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); test.TestMethod1();").GetCompletionValue().AsNumber().Should().Be(12d);
        }

        [Test]
        public void CallInstanceMethod_WrongCase_ResultCorrect()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); test.testMethod1();").GetCompletionValue().AsNumber().Should().Be(12d);
        }

        [Test]
        public void CallStaticMethod_CorrectCase_ResultCorrect()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); test.StaticMethod1();").GetCompletionValue().AsNumber().Should().Be(66d);
        }

        [Test]
        public void CallStaticMethod_WrongCase_ResultCorrect()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); test.staticMethod1();").GetCompletionValue().AsNumber().Should().Be(66d);
        }


        [Test]
        public void SetProperty_InstanceNotSet_SetsAndReadsCorrectly()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); test.newProperty = 99;  test.newProperty;").GetCompletionValue().AsNumber().Should().Be(99d);
        }

        [Test]
        public void StaticNotSet_SetsAndReadsCorrectly()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); testobj.newProperty = 89;  testobj.newProperty;").GetCompletionValue().AsNumber().Should().Be(89d);
        }

        [Test]
        public void InstanceSetAndDeleted_DeletesAndReadsUndefined()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); test.testProperty1;").GetCompletionValue().AsNumber().Should().Be(77d);
            engine.Execute("var test = new testobj(); delete test.testProperty1;  test.testProperty1;").GetCompletionValue().Should().Be(JsValue.Undefined);
            engine.Execute("var test = new testobj(); delete test.testProperty1;  test.testProperty1 = 'hh'; test.testProperty1;").GetCompletionValue().AsString().Should().Be("hh");
        }

        [Test]
        public void StaticSetAndDeleted_DeletesAndReadsUndefined()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); testobj.staticProperty1;").GetCompletionValue().AsNumber().Should().Be(64d);
            engine.Execute("var test = new testobj(); delete testobj.staticProperty1;  testobj.staticProperty1;").GetCompletionValue().Should().Be(JsValue.Undefined);
            engine.Execute("var test = new testobj(); delete testobj.staticProperty1;  testobj.staticProperty1 = 'hh'; testobj.staticProperty1;").GetCompletionValue().AsString().Should().Be("hh");
        }

        [Test]
        public void AsNetObject_DifferentTypesWithExplicitConversion_ResultCorrect()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); testobj.staticProperty1;").GetCompletionValue().AsNetObject<int>().Should().Be(64);
            engine.Execute("var test = new testobj(); testobj.staticProperty1;").GetCompletionValue().AsNetObject<double>().Should().Be(64d);
            engine.Execute("var test = new testobj(); testobj.staticProperty1;").GetCompletionValue().AsNetObject<ushort>().Should().Be(64);
            engine.Execute("var test = new testobj(); test.stringProperty;").GetCompletionValue().AsNetObject<string>().Should().Be("hello");
            engine.Execute("var test = new testobj(); test.boolProperty;").GetCompletionValue().AsNetObject<bool>().Should().Be(true);
            engine.Execute("var test = new testobj(); test.nullProperty;").GetCompletionValue().AsNetObject<object>().Should().Be(null);
            engine.Execute("var test = new testobj(); test.GetEncoding();").GetCompletionValue().AsNetObject<Encoding>().Should().Be(Encoding.UTF8);

            engine.Execute("var test = new testobj(); test.sdhfihsofois;").GetCompletionValue().AsNetObject().Should().Be(JsValue.Undefined);

        }

        [Test]
        public void AsNetObject_DifferentTypesWithoutExplicitConversion_ResultCorrect()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var test = new testobj(); testobj.staticProperty1;").GetCompletionValue().AsNetObject().Should().Be(64);
            engine.Execute("var test = new testobj(); test.doubleProperty;").GetCompletionValue().AsNetObject().Should().Be(55.5d);
            engine.Execute("var test = new testobj(); test.stringProperty;").GetCompletionValue().AsNetObject().Should().Be("hello");
            engine.Execute("var test = new testobj(); test.boolProperty;").GetCompletionValue().AsNetObject().Should().Be(true);
            engine.Execute("var test = new testobj(); test.nullProperty;").GetCompletionValue().AsNetObject().Should().Be(null);
            engine.Execute("var test = new testobj(); test.GetEncoding();").GetCompletionValue().AsNetObject().Should().Be(Encoding.UTF8);

            engine.Execute("var test = new testobj(); test.sdhfihsofois;").GetCompletionValue().AsNetObject().Should().Be(JsValue.Undefined);

            engine.Execute("var bob = function(){return 6;};bob;").GetCompletionValue().AsNetObject().Should().BeAssignableTo<FunctionInstance>();

        }

        [Test]
        public void OverwritingMethodWithAnother_ResultCorrect()
        {
            var engine = new Engine();

            engine.SetValue("testobj", TypeReference.CreateTypeReference(engine, typeof(TestNetObject)));

            engine.Execute("var bob = new testobj(); bob.testMethod1()").GetCompletionValue().AsNetObject().Should().Be(12);
            engine.Execute("var bob = new testobj(); bob.testMethod2()").GetCompletionValue().AsNetObject().Should().Be(13);

            engine.Execute("var bob = new testobj(); bob.testMethod1 = bob.testMethod2;  bob.testMethod1();").GetCompletionValue().AsNetObject().Should().Be(13);

        }


        public delegate void LogDelegate(JsValue value);

        [Test]
        public void Test()
        {
            var engine = new Engine(o => o.DebugMode());

            engine.SetValue("log", new LogDelegate(v =>
            {

            }));

            using (var sr = new StreamReader("c:\\temp\\polyfill.js"))
            {
                try
                {
                    engine.Execute("Array.prototype.toString.call(1);");
                    res = engine.GetCompletionValue();

                    //engine.Execute("var TO_STRING = 'toString';var $toString = /./[TO_STRING]; $toString.call({ source: 'a', flags: 'b' }); ");

                    //engine.Execute("var symbol3 = Symbol('foo');symbol3.toString();");

                    //var res = engine.GetCompletionValue();



                    engine.Execute(sr.ReadToEnd());
                }
                catch (Exception ex)
                {
                    var synt = engine.GetLastSyntaxNode();
                    var location = synt.Location.Start;
                }
            }
        }
    }

}
