using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TestOvoDotNetClient
{
    [TestClass]
    public class TestClient
    {
        OvoDotNetClient.Client _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new OvoDotNetClient.Client();
            _sut.SetLog(System.Console.Out);
        }
        [TestMethod]
        public void TestPutAndGet()
        {
            var p1 = createTestProduct(1, "milk");
            _sut.Put("myproduct", p1, 0);

            Product p2 = _sut.Get<Product>("myproduct");

            Assert.IsTrue(p1.Name == p2.Name && p1.CreationDate == p2.CreationDate);
        }

        [TestMethod]
        public void TestPutAndGetLoop()
        {
            for (int i = 0; i < 200; i++)
            {
                var p1 = createTestProduct(i, "milk"+i);
                _sut.Put("myproduct"+i, p1, 0);
            }
            for (int i = 0; i < 200; i++)
            {
                Product p2 = _sut.Get<Product>("myproduct"+i);
                Assert.IsTrue(p2 != null);
            }
        }

        [TestMethod]
        public void TestDelete()
        {
            var p1 = createTestProduct(1, "bread");
            _sut.Put("mybread", p1, 0);

            _sut.Delete("mybread");

            Product p2 = _sut.Get<Product>("mybread");

            Assert.IsTrue(p2 == null);
        }

        [TestMethod]
        public void TestCount()
        {
             var p1 = createTestProduct(1, "bread1");
            _sut.Put("mybread1", p1, 0);
            long count = _sut.Count();
            Console.Out.WriteLine("Count = " + count);
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void TestKeys()
        {
            var p1 = createTestProduct(2, "bread2");
            _sut.Put("mybread2", p1, 0);
            List<string> keys = _sut.Keys();
            Console.Out.WriteLine("Keys Count = " + keys.Count);
            Assert.IsTrue(keys.Count > 0);
        }
        
        [TestMethod]
        public void TestGetAndRemove()
        {
            var p1 = createTestProduct(1000, "chocolate");
            _sut.Put("mychocolate", p1, 0);

            Product p2 = _sut.GetAndRemove<Product>("mychocolate");

            Product p3 = _sut.Get<Product>("mychocolate");

            Assert.IsTrue(p1.Name == p2.Name && p3 == null);
        }

        [TestMethod]
        public void TestUpdateValueIfEqual()
        {
            var p1 = createTestProduct(1000, "cofee");
            _sut.Put("mycofee", p1, 0);

            var p2 = createTestProduct(1000, "moka cofee");

            var result = _sut.UpdateValueIfEqual("mycofee", p1, p2);

            Assert.IsTrue(result);

            Product p3 = _sut.Get<Product>("mycofee");

            Assert.IsTrue(p3.Name == p2.Name);
        }

        [TestMethod]
        public void TestIncrement()
        {
            long increment = 100L;
            long result1 = _sut.Increment("mycounter1", 11, 0);
            long result2 = _sut.Increment("mycounter1", increment, 0);

            Console.WriteLine(String.Format("result1 = {0} - result2 = {1}", result1, result2));

            Assert.IsTrue((result2 - result1) == increment);
        }

        [TestMethod]
        public void TestSetCounter()
        {
            long increment = 100L;
            long result1 = _sut.Increment("mycounter2", 11, 0);
            long result2 = _sut.Increment("mycounter2", increment, 0);
            long result3 = _sut.SetCounter("mycounter2", 5, 0);

            Console.WriteLine(String.Format("result1 = {0} - result2 = {1} - result3 = {2}", result1, result2, result3));

            Assert.IsTrue(result3 == 5);
        }

        [TestMethod]
        public void TestGetCounter()
        {
            long increment = 100L;
            long result1 = _sut.Increment("mycounter3", 11, 0);
            long result2 = _sut.Increment("mycounter3", increment, 0);

            long result3 = _sut.GetCounter("mycounter3");

            Console.WriteLine(String.Format("result1 = {0} - result2 = {1} - result3 = {2}", result1, result2, result3));

            Assert.IsTrue(result3 == result2);
        }

        [TestMethod]
        public void TestDeleteCounter()
        {
            long increment = 100L;
            long result1 = _sut.Increment("mycounter4", 11, 0);
            long result2 = _sut.Increment("mycounter4", increment, 0);
            _sut.DeleteCounter("mycounter4");
            long result3 = _sut.GetCounter("mycounter4");

            Console.WriteLine(String.Format("result1 = {0} - result2 = {1} - result3 = {2}", result1, result2, result3));

            Assert.IsTrue(result3 == 0);
        }

        [TestMethod]
        public void TestDeleteValueIfEqual()
        {
            var p1 = createTestProduct(1000, "soda");
            _sut.Put("mysoda", p1, 0);

            var p2 = createTestProduct(1001, "coke");

            var result = _sut.DeleteValueIfEqual("mysoda",p2);

            Assert.IsFalse(result);

            result = _sut.DeleteValueIfEqual("mysoda", p1);

            Assert.IsTrue(result);

            Product p3 = _sut.Get<Product>("mysoda");

            Assert.IsTrue(p3==null);
        }

        private Product createTestProduct(Int32 id, String name)
        {
            return new Product()
            {
                Id = id,
                Name = name,
                CreationDate = DateTime.Now,
                Description = name,
            };
        }
    }

    public class Product
    {
        public Int32 Id { get; set; }
        public string Name { set; get; }
        public DateTime CreationDate { set; get; }
        public String Description { get; set; }
    }
}
