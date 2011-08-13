using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebKit.JSCore;
using Moq;

namespace JSCore.Tests
{
    /// <summary>
    /// Summary description for JSIntegration
    /// </summary>
    [TestClass]
    public class JSIntegration
    {
        static double GOLDEN_RATIO = (1 + Math.Sqrt(5)) / 2;

        JSContext Context;
        Mock<TestFunctions> testFunctionsMock;

        class SimpleProperties
        {
            public string stringProperty { get; set; }
            public float floatProperty { get; set; }
            public double doubleProperty { get; set; }
            public bool boolProperty { get; set; }

        }

        public interface TestFunctions
        {
            void acceptsBoolean(bool b);
            void acceptsDouble(double d);
            void acceptsFloat(float f);
            void acceptsString(string s);
            void acceptsArray(object[] a);
            void acceptsObject(Dictionary<object, object> d);
        }

        [TestInitialize()]
        public void Initialize() {
            Context = new JSContext();
            Context.GetGlobalObject().SetProperty("simpleProperties", new SimpleProperties()
            {
                stringProperty = "stringPropertyValue",
                floatProperty = (float)Math.PI,
                doubleProperty = GOLDEN_RATIO,
                boolProperty = true
            });

            testFunctionsMock = new Mock<TestFunctions>();
            Context.GetGlobalObject().SetProperty("testFunctions", testFunctionsMock.Object);
        }

        [TestMethod]
        public void TestSimpleProperties()
        {
            JSValue result = Context.EvaluateScript("simpleProperties");
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsUndefined);

            JSObject o = result.ToObject();

            Assert.AreEqual("stringPropertyValue", o.GetProperty("stringProperty").ToString());
            Assert.IsTrue(precisionEquals(Math.PI, o.GetProperty("floatProperty").ToNumber()));
            Assert.IsTrue(precisionEquals(GOLDEN_RATIO, o.GetProperty("doubleProperty").ToNumber()));
            Assert.IsTrue(o.GetProperty("boolProperty").ToBoolean());
        }


        [TestMethod]
        public void TestFunctionInput()
        {
            testFunctionsMock.Setup(f => f.acceptsBoolean(true)).Verifiable();
            testFunctionsMock.Setup(f => f.acceptsDouble(It.Is<double>(d => precisionEquals(d, GOLDEN_RATIO)))).Verifiable();
            testFunctionsMock.Setup(f => f.acceptsFloat(It.Is<float>(fl => precisionEquals(fl, Math.PI)))).Verifiable();
            testFunctionsMock.Setup(f => f.acceptsString("stringVal")).Verifiable();
            
            testFunctionsMock.Setup(f => f.acceptsArray(It.Is<object[]>(o => 
                o.Length == 3 && 
                (double)o[0] == 1 && 
                o[1].Equals("second") && 
                ((bool)((object[])o[2])[0]) == true &&
                (double)((object[])o[2])[1] == 3
            ))).Verifiable();

            testFunctionsMock.Setup(f => f.acceptsObject(It.Is<Dictionary<object, object>>(d =>
                (double)d["x"] == 1 &&
                ((object[])d["array"]).Length == 3 &&
                ((Dictionary<object,object>)d["nestedObj"])["z"].Equals("nested")
            ))).Verifiable();
            
            Context.EvaluateScript("testFunctions.acceptsBoolean(true)");
            Context.EvaluateScript("testFunctions.acceptsDouble(" + GOLDEN_RATIO + ")");
            Context.EvaluateScript("testFunctions.acceptsFloat(" + Math.PI + ")");
            Context.EvaluateScript("testFunctions.acceptsString('stringVal')");
            Context.EvaluateScript("testFunctions.acceptsArray([1, 'second', [true, 3, []]])");
            Context.EvaluateScript("testFunctions.acceptsObject({x:1, array:[1,2,3], nestedObj:{z:'nested'}})");

            testFunctionsMock.Verify();
        }


        private bool precisionEquals(double expected, double actual)
        {
            return precisionEquals(expected, actual, 0.0001);
        }
        private bool precisionEquals(double expected, double actual, double precision)
        {
            return Math.Abs(expected - actual) < precision;
        }
    }
}
