using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Jint.Native;
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
    }
}
