﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.Models;
using SportsStore.WebUI.HtmlHelpers;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            // przygotowanie
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product{ProductID = 1, Name = "P1"},
                new Product{ProductID = 2, Name = "P2"},
                new Product{ProductID = 3, Name = "P3"},
                new Product{ProductID = 4, Name = "P4"},
                new Product{ProductID = 5, Name = "P5"},
            });

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // działanie
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            // assercje
            Product[] prodArray = result.Products.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "P4");
            Assert.AreEqual(prodArray[1].Name, "P5");
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            // przygotowanie - definiowanie metody pomocniczej html. Potrzebujemy tego,
            // aby użyć metody rozszerzającej
            HtmlHelper myHelper = null;

            // przygotowanie - tworzenie danych PagingInfo
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            // przygotowanie - konfiguracja delegata z użyciem wyrażenia lambda
            Func<int, string> pageUrlDelegate = i => "Strona" + i;

            // działanie
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            // assercje
            Assert.AreEqual(@"<a class=""btn btn-default"" href=""Strona1"">1</a><a class=""btn btn-default btn-primary selected"" href=""Strona2"">2</a><a class=""btn btn-default"" href=""Strona3"">3</a>", result.ToString());
        }

        // powyższe - crash na poziomie maszyny. Bez netu nie ogarnę. Wrócić tutaj! Trzeba ogarnąć!!!
        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            // przygotowanie
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product{ProductID = 1, Name = "P1"},
                new Product{ProductID = 2, Name = "P2"},
                new Product{ProductID = 3, Name = "P3"},
                new Product{ProductID = 4, Name = "P4"},
                new Product{ProductID = 5, Name = "P5"},
            });

            // przygotowanie
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // działanie
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            // assercje
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, 3);
            Assert.AreEqual(pageInfo.TotalItems, 5);
            Assert.AreEqual(pageInfo.TotalPages, 2);
        }

        [TestMethod]
        public void Can_Filter_Products()
        {
            // przygotowanie
            // -- utworzenie imitacji repozytorium
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product{ProductID = 1, Name = "P1", Category = "Cat1"},
                new Product{ProductID = 2, Name = "P2", Category = "Cat2"},
                new Product{ProductID = 3, Name = "P3", Category = "Cat1"},
                new Product{ProductID = 4, Name = "P4", Category = "Cat2"},
                new Product{ProductID = 5, Name = "P5", Category = "Cat3"}
            });

            // przygotowanie - utworzenie kontrolera i ustawienie 3-elementowej strony
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // działanie
            Product[] result = ((ProductsListViewModel)controller.List("Cat2", 1).Model)
                .Products.ToArray();

            // asercje
            Assert.AreEqual(result.Length, 2);
            Assert.IsTrue(result[0].Name == "P2" && result[0].Category == "Cat2");
            Assert.IsTrue(result[1].Name == "P4" && result[1].Category == "Cat2");
        }
    }
}
