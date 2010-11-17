namespace Tests
{
    using NUnit.Framework;
    using WebKit.JSCore;

    public class TestGlobalObject
    {
        public double GetPi()
        {
            return 3.14159;
        }

        public bool IsTrue()
        {
            return true;
        }

        public string GetName()
        {
            return "Mittens";
        }

        public TestObject GetObject()
        {
            return new TestObject();
        }
    }

    public class TestObject
    {
        public string Name { get { return "Test"; } }

        public string Function()
        {
            return string.Format("Hello {0}", Name);
        }
    }

    [TestFixture]
    public class TestIntegration
    {
        protected JSContext Context { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Context = new JSContext();
            Context.GetGlobalObject().SetProperty("tests", new TestGlobalObject());
        }

        [Test]
        public void FunctionReturnNumberTest()
        {
            var ret = Context.EvaluateScript("tests.GetPi()");
            Assert.IsTrue(ret.IsNumber);
            var x = ret.ToNumber();
            Assert.AreEqual(3.14159, x);
        }

        [Test]
        public void FunctionReturnBooleanTest()
        {
            var ret = Context.EvaluateScript("tests.IsTrue()");
            Assert.IsTrue(ret.IsBoolean);
            var y = ret.ToBoolean();
            Assert.IsTrue(y);
        }

        [Test]
        public void FunctionReturnStringTest()
        {
            var ret = Context.EvaluateScript("tests.GetName()");
            Assert.IsTrue(ret.IsString);
            var name = ret.ToString();
            Assert.AreEqual("Mittens", name);
        }

        [Test]
        public void FunctionReturnObjectTest()
        {
            var ret = Context.EvaluateScript("tests.GetObject()");
            Assert.IsTrue(ret.IsObject);
        }

        [Test]
        public void FunctionReturnObjectHasStringPropertyTest()
        {
            var ret = Context.EvaluateScript("tests.GetObject()");
            Assert.IsTrue(ret.IsObject);
            var obj = ret.ToObject();
            Assert.IsTrue(obj.HasProperty("Name"));
            var name = obj.GetProperty("Name");
            Assert.IsTrue(name.IsString);
            var nameStr = name.ToString();
            Assert.AreEqual("Test", nameStr);
        }

        [Test]
        public void FunctionReturnObjectsStringPropertyTest()
        {
            var ret = Context.EvaluateScript("tests.GetObject().Name");
            Assert.IsTrue(ret.IsString);
            var str = ret.ToString();
            Assert.AreEqual("Test", str);
        }

        [Test]
        public void FunctionReturnObjectCallFunctionTest()
        {
            var ret = Context.EvaluateScript("tests.GetObject().Function()");
            Assert.IsTrue(ret.IsString);
            var str = ret.ToString();
            Assert.AreEqual("Hello Test", str);
        }
    }
}
