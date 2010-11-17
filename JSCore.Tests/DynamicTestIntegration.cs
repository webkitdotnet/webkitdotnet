using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class DynamicTestIntegration : TestIntegration
    {
        [Test]
        public void DynamicFunctionReturnNumberTest()
        {
            dynamic ret = Context.EvaluateScript("tests.GetPi()");
            var x = (double)ret;
            Assert.AreEqual(3.14159, x);
        }

        [Test]
        public void DynamicFunctionReturnBooleanTest()
        {
            dynamic ret = Context.EvaluateScript("tests.IsTrue()");
            var y = (bool)ret;
            Assert.IsTrue(y);
        }

        [Test]
        public void DynamicFunctionReturnStringTest()
        {
            dynamic ret = Context.EvaluateScript("tests.GetName()");
            var name = (string)ret;
            Assert.AreEqual("Mittens", name);
        }

        [Test]
        public void DynamicFunctionReturnObjectHasStringPropertyTest()
        {
            dynamic ret = Context.EvaluateScript("tests.GetObject()");
            Assert.AreEqual("Test", ret.Name);
        }

        [Test]
        public void DynamicFunctionReturnObjectsStringPropertyTest()
        {
            dynamic ret = Context.EvaluateScript("tests.GetObject().Name");
            var str = (string)ret;
            Assert.AreEqual("Test", str);
        }

        [Test]
        public void DynamicFunctionReturnObjectCallFunctionTest()
        {
            dynamic ret = Context.EvaluateScript("tests.GetObject().Function()");
            var str = (string)ret;
            Assert.AreEqual("Hello Test", str);
        }

        [Test]
        public void DynamicFunctionReturnObjectsFunctionTest()
        {
            dynamic ret = Context.EvaluateScript("tests.GetObject()");
            dynamic ret2 = ret.Function();
            Assert.AreEqual("Hello Test", (string)ret2);
        }
    }
}
