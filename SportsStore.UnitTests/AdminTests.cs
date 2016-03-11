using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using System.Web.Mvc;


namespace SportsStore.UnitTests
{
    [TestClass]
    public class AdminTests
    {

        [TestMethod]
        public void Index_Contains_All_Products()
        {
            // przygotowanie - tworzenie imitacji repozytorium
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product{ProductID = 1, Name = "P1"},
                    new Product{ProductID = 2, Name = "P2"},
                    new Product{ProductID = 3, Name = "P3"}
                });

            // przygotowanie - utworzenie kontrolera
            AdminController target = new AdminController(mock.Object);

            // działanie
            Product[] result = ((IEnumerable<Product>)target.Index().ViewData.Model).ToArray();

            // asercje
            Assert.AreEqual(result.Length, 3);
            Assert.AreEqual("P1", result[0].Name);
            Assert.AreEqual("P2", result[1].Name);
            Assert.AreEqual("P3", result[2].Name);
        }

        [TestMethod]
        public void Can_Edit_Product()
        {
            // przygotowanie - tworzenie imitacji repozytorium
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] 
            {
                new Product{ProductID = 1, Name = "P1"},
                new Product{ProductID = 2, Name = "P2"},
                new Product{ProductID = 3, Name = "P3"}
            });

            // przygotowanie - utworzenie kontrolera
            AdminController target = new AdminController(mock.Object);

            // dzianie
            Product p1 = target.Edit(1).ViewData.Model as Product;
            Product p2 = target.Edit(2).ViewData.Model as Product;
            Product p3 = target.Edit(3).ViewData.Model as Product;

            // asercje
            Assert.AreEqual(1, p1.ProductID);
            Assert.AreEqual(2, p2.ProductID);
            Assert.AreEqual(3, p3.ProductID);
        }

        [TestMethod]
        public void Cannot_Edit_NonExisted_Product()
        {
            // przygotowanie - tworzenie imitacji repozytorium
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] 
            {
                new Product{ProductID = 1, Name = "P1"},
                new Product{ProductID = 2, Name = "P2"},
                new Product{ProductID = 3, Name = "P3"}
            });

            // przygotowanie - utworzenie kontrolera
            AdminController target = new AdminController(mock.Object);

            // działanie
            Product result = target.Edit(4).ViewData.Model as Product;

            // asercje
            Assert.IsNull(result);
        }

        [TestMethod]
        public void 
    }
}
