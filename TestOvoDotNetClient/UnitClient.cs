using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestOvoDotNetClient
{
    [TestClass]
    public class UnitClient
    {
        OvoDotNetClient.Client _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new OvoDotNetClient.Client();
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
            for (int i = 0; i < 100; i++)
            {
                var p1 = createTestProduct(i, "milk"+i);
                _sut.Put("myproduct"+i, p1, 0);
            }
            for (int i = 0; i < 100; i++)
            {
                Product p2 = _sut.Get<Product>("myproduct"+i);
                Assert.IsTrue(p2 != null);
            }
            
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
