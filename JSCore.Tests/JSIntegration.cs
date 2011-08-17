using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebKit.JSCore;
using Moq;
using System.Threading;

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
            public string noGetterStringProperty = "stringProp";
            public string stringProperty { get; set; }
            public float floatProperty { get; set; }
            public double doubleProperty { get; set; }
            public bool boolProperty { get; set; }
            public TestFunctions nestedProperty { get; set; }
        }

        public interface TestFunctions
        {
            void acceptsBoolean(bool b);
            void acceptsDouble(double d);
            void acceptsFloat(float f);
            void acceptsString(string s);
            void acceptsArray(object[] a);
            void acceptsObject(Dictionary<object, object> d);
            void acceptsDelegate(Delegate d);
            
            void verified(bool r);
        }

        [TestInitialize()]
        public void Initialize() {
            testFunctionsMock = new Mock<TestFunctions>();

            Context = new JSContext();
            Context.GetGlobalObject().SetProperty("simpleProperties", new SimpleProperties()
            {
                stringProperty = "stringPropertyValue",
                floatProperty = (float)Math.PI,
                doubleProperty = GOLDEN_RATIO,
                boolProperty = true,
                nestedProperty = testFunctionsMock.Object
            });

            
            Context.GetGlobalObject().SetProperty("testFunctions", testFunctionsMock.Object);
        }

        [TestMethod]
        public void TestSimpleProperties()
        {
            JSValue result = Context.EvaluateScript("simpleProperties");
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsUndefined);

            JSObject o = result.ToObject();                      

            Assert.AreEqual("stringProp", o.GetProperty("noGetterStringProperty").ToString());
            Assert.AreEqual("stringPropertyValue", o.GetProperty("stringProperty").ToString());
            Assert.IsTrue(precisionEquals(Math.PI, o.GetProperty("floatProperty").ToNumber()));
            Assert.IsTrue(precisionEquals(GOLDEN_RATIO, o.GetProperty("doubleProperty").ToNumber()));
            Assert.IsTrue(o.GetProperty("boolProperty").ToBoolean());
        }

        [TestMethod]
        public void TestNestedSimpleProperties()
        {
            Assert.IsFalse(Context.EvaluateScript("simpleProperties.nestedProperty").IsUndefined);

            testFunctionsMock.Setup(f => f.acceptsBoolean(true));
            Context.EvaluateScript("simpleProperties.nestedProperty.acceptsBoolean(true)");
            testFunctionsMock.VerifyAll();
        }

        [TestMethod]
        public void TestFunctionSimpleInput()
        {
            testFunctionsMock.Setup(f => f.acceptsBoolean(true));
            testFunctionsMock.Setup(f => f.acceptsDouble(It.Is<double>(d => precisionEquals(d, GOLDEN_RATIO))));
            testFunctionsMock.Setup(f => f.acceptsFloat(It.Is<float>(fl => precisionEquals(fl, Math.PI))));
            testFunctionsMock.Setup(f => f.acceptsString("stringVal"));
            
            testFunctionsMock.Setup(f => f.acceptsArray(It.Is<object[]>(o => 
                o.Length == 3 && 
                (double)o[0] == 1 && 
                o[1].Equals("second") && 
                ((bool)((object[])o[2])[0]) == true &&
                (double)((object[])o[2])[1] == 3
            )));

            testFunctionsMock.Setup(f => f.acceptsObject(It.Is<Dictionary<object, object>>(d =>
                (double)d["x"] == 1 &&
                ((object[])d["array"]).Length == 3 &&
                ((Dictionary<object,object>)d["nestedObj"])["z"].Equals("nested")
            )));

            Context.EvaluateScript("testFunctions.acceptsBoolean(true)");
            Context.EvaluateScript("testFunctions.acceptsDouble(" + GOLDEN_RATIO + ")");
            Context.EvaluateScript("testFunctions.acceptsFloat(" + Math.PI + ")");
            Context.EvaluateScript("testFunctions.acceptsString('stringVal')");
            Context.EvaluateScript("testFunctions.acceptsArray([1, 'second', [true, 3, []]])");
            Context.EvaluateScript("testFunctions.acceptsObject({x:1, array:[1,2,3], nestedObj:{z:'nested'}})");

            testFunctionsMock.VerifyAll();
        }

        /// <summary>
        /// C# methods should be able to accept a delegate function and be able to invoke it
        /// </summary>
        [TestMethod]
        public void TestFunctionSimpleCallbacks()
        {           
            testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<Delegate>())).Callback<Delegate>(d => Assert.IsTrue((bool)d.DynamicInvoke((object) new object[] { })));
            testFunctionsMock.Setup(f => f.verified(true));

            Context.EvaluateScript("testFunctions.acceptsDelegate(function() { testFunctions.verified(true); return true; })");

            testFunctionsMock.VerifyAll();
        }

        /// <summary>
        /// C# methods should be able to accept a delegate function with parameters and object return values
        /// </summary>
        [TestMethod]       
        public void TestFunctionCallbacksWithParametersAndReturnsObject()
        {
            testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<Delegate>())).Callback<Delegate>(VerifyObjectCallbackFunction);
            testFunctionsMock.Setup(f => f.verified(true));

            Context.EvaluateScript(@"
                testFunctions.acceptsDelegate(function(boolParam) {
                    testFunctions.verified(boolParam);
                    return { success:'hello world', x:true }; 
                })
            ");

            testFunctionsMock.VerifyAll();
        }

        /// <summary>
        /// C# methods should be able to accept a delegate function with parameters and array return values
        /// </summary>
        [TestMethod]
        public void TestFunctionCallbacksWithParametersAndReturnsArray()
        {
            testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<Delegate>())).Callback<Delegate>(VerifyArrayCallbackFunction);
            testFunctionsMock.Setup(f => f.verified(true));

            Context.EvaluateScript(@"
                testFunctions.acceptsDelegate(function(boolParam) {
                    testFunctions.verified(boolParam);
                    return ['first', 2, boolParam]; 
                })
            ");

            testFunctionsMock.VerifyAll();
        }

        /// <summary>
        /// Have the C# delegate spawn a thread and call the passed in calback function asyncrounously
        /// </summary>
        [TestMethod]
        public void TestFunctionsAsyncCallbacks()
        {
            lock (this)
            {
                testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<Delegate>())).Callback<Delegate>(AsyncCallbackFunction);
                testFunctionsMock.Setup(f => f.verified(true));
                
                JSValue result = Context.EvaluateScript(@"                    
                    testFunctions.acceptsDelegate(function() { 
                        testFunctions.verified(true);
                        return true; 
                    })                    
                ");
                

                if (!Monitor.Wait(this, 1000)) Assert.Fail("Callback function didn't fire");
            }

            testFunctionsMock.VerifyAll();
        }


        /// <summary>
        /// Verify that the callback accepts parameters and returns an object
        /// </summary>
        /// <param name="d"></param>
        private void VerifyObjectCallbackFunction(Delegate d)
        {
            bool param = true;
            Dictionary<object, object> returnVal = (Dictionary<object, object>)d.DynamicInvoke((object)new object[] { param });
            Assert.AreEqual<string>("success", (string)returnVal.Keys.ElementAt(0));
            Assert.AreEqual<string>("hello world", (string)returnVal["success"]);
        }

        /// <summary>
        /// Verify that the callback accepts parametsr and returns an array
        /// </summary>
        /// <param name="d"></param>
        private void VerifyArrayCallbackFunction(Delegate d)
        {
            bool param = true;
            object[] ret = (object[]) d.DynamicInvoke((object)new object[] { param });
            Assert.AreEqual(3, ret.Length);
            Assert.AreEqual("first", ret[0]);
            Assert.IsTrue(precisionEquals(2.0, (double)ret[1]));
            Assert.AreEqual(param, (bool)ret[2]);
        }

        /// <summary>
        /// Fire off a thread and invoke the delegate later
        /// </summary>
        /// <param name="d"></param>
        private void AsyncCallbackFunction(Delegate d)
        {
            var worker = new System.ComponentModel.BackgroundWorker();            
            worker.DoWork += delegate
            {
                lock (this)
                {
                    Thread.Sleep(100);
                    Assert.IsTrue((bool)d.DynamicInvoke((object)new object[] { }));
                    Monitor.Pulse(this);    
                }                 
            };            
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Are the double close enough?
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <returns></returns>
        private bool precisionEquals(double expected, double actual)
        {
            return precisionEquals(expected, actual, 0.0001);
        }

        /// <summary>
        /// Are the double values within precision?
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        private bool precisionEquals(double expected, double actual, double precision)
        {
            return Math.Abs(expected - actual) < precision;
        }
    }
}
