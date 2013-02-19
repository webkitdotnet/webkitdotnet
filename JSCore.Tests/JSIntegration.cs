using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebKit.JSCore;
using Moq;
using System.Threading;
using System.Windows.Threading;

namespace JSCore.Tests
{
    /// <summary>
    /// Summary description for JSIntegration
    /// </summary>
    [TestClass]
    public class JSIntegration
    {
        static double GOLDEN_RATIO = (1 + Math.Sqrt(5)) / 2;

        static ManualResetEvent m_trigger = new ManualResetEvent(false);

        JSContext Context;
        Mock<TestFunctions> testFunctionsMock;

        class SimpleProperties
        {
            public string noGetterStringProperty = "stringProp";
            public string stringProperty { get; set; }
            public int intProperty { get; set; }
            public float floatProperty { get; set; }
            public double doubleProperty { get; set; }
            public bool boolProperty { get; set; }
            public float[] floatsProperty { get; set; }
            public Dictionary<object, object> dictProperty { get; set; }
            public Dictionary<string, object> nonGenericDictionary { get; set; }            
            public TestFunctions nestedProperty { get; set; }
        }

        public interface TestFunctions
        {
            void acceptsBoolean(bool b);
            void acceptsDouble(double d);
            void acceptsInt(int i);
            void acceptsLong(long l);
            void acceptsDateTime(DateTime dt);
            void acceptsFloat(float f);
            void acceptsString(string s);
            void acceptsArray(object[] a);
            void acceptsObject(JSObject d);
            void acceptsNonGenericDictionary(Dictionary<string, object> d);
            void acceptsDelegate(JavaScriptFunction d);
            void acceptsJSObject(JSObject d);
            
            void verified(bool r);

            object returnsNull();
        }

        [TestInitialize()]
        public void Initialize() {
            testFunctionsMock = new Mock<TestFunctions>();

            Context = new JSContext();
            Context.GetGlobalObject().SetProperty("simpleProperties", new SimpleProperties()
            {
                stringProperty = "stringPropertyValue",
                intProperty = 3,
                floatProperty = (float)Math.PI,
                doubleProperty = GOLDEN_RATIO,
                boolProperty = true,
                floatsProperty = new float[] { 3.14f, 16/9 },
                dictProperty = new Dictionary<object,object>() {
                    {"123", 2},
                    {"string", "hello world"}
                },         
                nonGenericDictionary = new Dictionary<string, object>() { { "x", 1 } },
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
            Assert.AreEqual(3, o.GetProperty("intProperty").ToNumber());
            Assert.IsTrue(precisionEquals(Math.PI, o.GetProperty("floatProperty").ToNumber()));
            Assert.IsTrue(precisionEquals(GOLDEN_RATIO, o.GetProperty("doubleProperty").ToNumber()));
            Assert.IsTrue(o.GetProperty("boolProperty").ToBoolean());
        }


        [TestMethod]
        public void TestArraySimpleProperties()
        {
            JSValue result = Context.EvaluateScript("simpleProperties.floatsProperty");
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsUndefined);

            JSObject o = result.ToObject();
            Assert.AreEqual(2, o.GetProperty("length").ToNumber());
        }

        [TestMethod]
        public void TestDictionarySimpleProperties()
        {
            JSValue result = Context.EvaluateScript("simpleProperties.dictProperty");
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsUndefined);

            JSObject dictionary = result.ToObject();

            JSValue r = Context.EvaluateScript("(function() {var x = 0; for (var p in simpleProperties.dictProperty) { x++ }; return x})()");

            Assert.IsFalse(result.IsUndefined);
            Assert.IsTrue(r.IsNumber);
            Assert.AreEqual(2, r.ToNumber());
            Assert.IsTrue(dictionary.HasProperty("string"));
            Assert.IsTrue(dictionary.HasProperty("123"));
            Assert.AreEqual(2, dictionary.GetProperty("123").ToNumber());
            Assert.AreEqual("hello world", dictionary.GetProperty("string").ToString());
        }

        [TestMethod]
        public void TestNonGenericDictionary()
        {
            JSObject dict = Context.EvaluateScript("simpleProperties.nonGenericDictionary").ToObject();
            Assert.IsTrue(dict.HasProperty("x"));
            JSValue val = dict.GetProperty("x");
            Assert.AreEqual(1, val.ToNumber());

            Assert.AreEqual(1, Context.EvaluateScript("simpleProperties.nonGenericDictionary['x']").ToNumber());

        }

        [TestMethod]
        public void TestSetDictionaryValues()
        {
            Context.EvaluateScript("simpleProperties.nonGenericDictionary['y'] = 2");
            Assert.AreEqual(2, Context.EvaluateScript("simpleProperties.nonGenericDictionary['y']").ToNumber());           
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
            testFunctionsMock.Setup(f => f.acceptsInt(1024));            
            testFunctionsMock.Setup(f => f.acceptsLong(It.Is<Int64>(l => l == 1318538364334)));
            testFunctionsMock.Setup(f => f.acceptsString("stringVal"));
            testFunctionsMock.Setup(f => f.returnsNull()).Returns(null);
            //testFunctionsMock.Setup(f => f.acceptsDateTime(It.Is<DateTime>(d => d != null)));

            testFunctionsMock.Setup(f => f.acceptsArray(It.Is<object[]>(o => 
                o.Length == 3 && 
                (double)o[0] == 1 && 
                o[1].Equals("second") && 
                ((bool)((object[])o[2])[0]) == true &&
                (double)((object[])o[2])[1] == 3
            )));

            testFunctionsMock.Setup(f => f.acceptsObject(It.Is<JSObject>(d =>
                (double) (d.ToDictionary(true))["x"] == 1 &&
                ((object[]) (d.ToDictionary(true))["array"]).Length == 3 &&
                // Test both recursive and non-recursive conversion to dictionary
                ((JSObject) ((d.ToDictionary())["nestedObj"])).ToDictionary(true)["z"].Equals("nested") &&
                ((Dictionary<object, object>)(d.ToDictionary(true))["nestedObj"])["z"].Equals("nested")
            )));

            Context.EvaluateScript("testFunctions.acceptsBoolean(true)");
            Context.EvaluateScript("testFunctions.acceptsDouble(" + GOLDEN_RATIO + ")");
            Context.EvaluateScript("testFunctions.acceptsInt(1024)");
            Context.EvaluateScript("testFunctions.acceptsLong(1318538364334)");
            Context.EvaluateScript("testFunctions.acceptsFloat(" + Math.PI + ")");
            Context.EvaluateScript("testFunctions.acceptsString('stringVal')");
            Context.EvaluateScript("testFunctions.acceptsArray([1, 'second', [true, 3, []]])");
            Context.EvaluateScript("testFunctions.acceptsObject({x:1, array:[1,2,3], nestedObj:{z:'nested'}})");
            JSValue v = Context.EvaluateScript("testFunctions.returnsNull()");
            Assert.IsTrue(v.IsUndefined);

            //Context.EvaluateScript("testFunctions.acceptsDateTime(new Date(1980, 1, 9))");


            testFunctionsMock.VerifyAll();
        }

        /*
        [TestMethod]
        public void TestFunctionNonGenericDictionary()
        {
            testFunctionsMock.Setup(f => f.acceptsNonGenericDictionary(It.Is<Dictionary<string, object>>(d => true)));
            Context.EvaluateScript("testFunctions.acceptsNonGenericDictionary({'x': 1})");
        }*/

        /// <summary>
        /// C# methods should be able to accept a delegate function and be able to invoke it
        /// </summary>
        [TestMethod]
        public void TestFunctionSimpleCallbacks()
        {           
            testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<JavaScriptFunction>())).Callback<JavaScriptFunction>(d => Assert.IsTrue((bool)d(Context)));
            testFunctionsMock.Setup(f => f.verified(true));

            Context.EvaluateScript("testFunctions.acceptsDelegate(function() { testFunctions.verified(true); return true; })");

            testFunctionsMock.VerifyAll();
        }

        

        /// <summary>
        /// C# methods should be able to accept a delegate function and be able to invoke it with parameter
        /// </summary>
        [TestMethod]
        public void TestFunctionArgumentCallbacks()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            
            testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<JavaScriptFunction>())).Callback<JavaScriptFunction>(d => {

                var worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += delegate
                {
                    m_trigger.Set();
                    dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        //m_trigger.Set();
                        Assert.IsTrue((bool)d(Context, 10));
                    });    
                };
       
                worker.RunWorkerAsync();
            });
            testFunctionsMock.Setup(f => f.verified(true));
            
            Context.EvaluateScript("testFunctions.acceptsDelegate(function(aFloat) { testFunctions.verified(aFloat == 10.0); return true; })");
            
            m_trigger.WaitOne();

            //testFunctionsMock.VerifyAll();
        }

        /// <summary>
        /// C# methods should be able to accept a delegate function with parameters and object return values
        /// </summary>
        [TestMethod]       
        public void TestFunctionCallbacksWithParametersAndReturnsObject()
        {
            testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<JavaScriptFunction>())).Callback<JavaScriptFunction>(VerifyObjectCallbackFunction);
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
            testFunctionsMock.Setup(f => f.acceptsDelegate(It.IsAny<JavaScriptFunction>())).Callback<JavaScriptFunction>(VerifyArrayCallbackFunction);
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
        /// Verify that the callback accepts parameters and returns an object
        /// </summary>
        /// <param name="d"></param>
        private void VerifyObjectCallbackFunction(JavaScriptFunction d)
        {
            bool param = true;
            Dictionary<object, object> returnVal = ((JSObject)d(Context, param)).ToDictionary();
            Assert.AreEqual<string>("success", (string)returnVal.Keys.ElementAt(0));
            Assert.AreEqual<string>("hello world", (string)returnVal["success"]);
        }

        /// <summary>
        /// Verify that the callback accepts parametsr and returns an array
        /// </summary>
        /// <param name="d"></param>
        private void VerifyArrayCallbackFunction(JavaScriptFunction d)
        {
            bool param = true;
            object[] ret = (object[])d(Context, param);
            Assert.AreEqual(3, ret.Length);
            Assert.AreEqual("first", ret[0]);
            Assert.IsTrue(precisionEquals(2.0, (double)ret[1]));
            Assert.AreEqual(param, (bool)ret[2]);
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
