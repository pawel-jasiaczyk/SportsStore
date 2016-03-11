﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportsStore.Domain.Entities;
using System.Linq;
using System.Collections.Generic;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.WebUI.Controllers;
using System.Web.Mvc;
using SportsStore.WebUI.Models;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            // przygotowanie - utworzenie produktów testowych
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            // przygotowanie - utworzenie nowego koszyka
            Cart target = new Cart();

            // działanie
            target.AddItem(p1, 1);
            target.AddItem(p2, 2);
            CartLine[] result = target.Lines.ToArray();

            // asercje
            Assert.AreEqual(result.Length, 2);
            Assert.AreEqual(result[0].Product, p1);
            Assert.AreEqual(result[1].Product, p2);
        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            // przygotowanie - utworzenie produktów testowych
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            // przygotowanie - utworzenie nowego koszyka
            Cart target = new Cart();

            // działanie
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);
            CartLine[] result = target.Lines.OrderBy(l => l.Product.ProductID).ToArray();

            // asercje
            Assert.AreEqual(result.Length, 2);
            Assert.AreEqual(result[0].Quantity, 11);
            Assert.AreEqual(result[1].Quantity, 1);
        }

        [TestMethod]
        public void Can_Remove_Item()
        {
            // przygotowanie
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };

            // przygotowanie - utworzenie nowego koszyka
            Cart target = new Cart();

            // przygotowanie - dodanie kilku produktów do koszyka
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            // działanie - usunięcie produktu
            target.RemoveLine(p2);

            // asercje
            Assert.AreEqual(target.Lines.Where(c => c.Product == p2).Count(), 0);
            Assert.AreEqual(target.Lines.Count(), 2);
        }

        [TestMethod]
        public void Can_Calculate_Cart_Total()
        {
            // przygotowanie - utworzenie produktów testowych
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            // przygotowanie - utworzenie nowego koszyka
            Cart target = new Cart();

            // działanie
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal result = target.ComputeTotalValue();

            // asercje
            Assert.AreEqual(450M, result);
        }

        [TestMethod]
        public void Can_Clear_Contents()
        {
            // przygotowanie - utworzenie produktów testowych
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            // przygotowanie - utworzenie nowego koszyka
            Cart target = new Cart();

            // przygotowanie - dodanie produktów do koszyka
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);

            // działanie - czyszczenie koszyka
            target.Clear();

            // assercje
            Assert.AreEqual(target.Lines.Count(), 0);
        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            // przygotowanie - tworzenie imitacji repozytorium
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product{ProductID = 1, Name = "P1", Category = "Jab"}
            }.AsQueryable());

            // przygotowanie - utworzenie koszyka
            Cart cart = new Cart();

            // przygotowanie - utworzenie kontrolera
            CartController target = new CartController(mock.Object, null);

            // działanie - dodanie produktu do koszyka
            target.AddToCart(cart, 1, null);

            // assercje
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            // przygotowanie - utworzenie imitacji repozytorium
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product{ProductID = 1, Name = "P1", Category = "Jabłka"}
            }.AsQueryable());

            // przygotowanie - utworzenie koszyka
            Cart cart = new Cart();

            // przygotowanie - utworzenie kontrolera
            CartController target = new CartController(mock.Object, null);

            // działanie - dodanie produktu do koszyka
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            // assercje
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            // przygotowanie - utworzenie koszyka
            Cart cart = new Cart();

            // przygotowanie - utworzenie kontrolera
            CartController target = new CartController(null, null);

            // działanie - wywołanie metody akcji Index
            CartIndexViewModel result
                = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            // assercje
            Assert.AreEqual(result.Cart, cart);
            Assert.AreEqual(result.RreturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            // przygotowanie - tworzenie imitacji procesora zamówiania
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // przygotowanie - tworzenie pustego koszka
            Cart cart = new Cart();

            // przygotowanie - tworzenie danych do wysyłki
            ShippingDetails shippingDetails = new ShippingDetails();

            // przygotowanie - tworzenie egzemplarza kontrolera
            CartController target = new CartController(null, mock.Object);

            // działanie
            ViewResult result = target.Checkout(cart, shippingDetails);

            // asercje - sprawdzenie, czy zamówienie zostało przekazane do procesora
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never());

            // asercje - sprawdzenie, czu metoda zwraca domyślny widok
            Assert.AreEqual("", result.ViewName);

            // asercje - sprawdzenie, czy przekazujemy prawidłowy model do widoku
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            // przygotowanie - tworzenie imitacji procesora zamówień
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // przygotowanie - tworzenie koszyka z produktem
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            // przygotowanie - tworzenie egzemplarza kontrolera
            CartController target = new CartController(null, mock.Object);

            // przygotowanie - dodanie błędu do modelu
            target.ModelState.AddModelError("error", "error");

            // działanie - próba zakończenia zamówienia
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // assercje - sprawdzenie, czy zamówienie zostało przekazane do procesora
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never());

            // assercje - sprawdzenie, czy metoda zwraca domyślny widok
            Assert.AreEqual("", result.ViewName);

            // asercje - sprawdzenie, czy przekazujemy nieprawidłowy model do widoku
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Can_Checkout_And_Submit_Order()
        {
            // przygotowanie - tworzenie imitacji procesora zamówień
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // przygotowanie - tworzenie koszyka z produktem
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            // przygotowanie - tworzenie egzemplarza kontrolera
            CartController target = new CartController(null, mock.Object);

            // działanie - próba zakończenia zamówienia
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // asercje - sprawdzenie, czy zamówienie zostało przekazane do procesora
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Once());

            // asercje - sprawdzenie, czy metoda zwraca widok Completed
            Assert.AreEqual("Completed", result.ViewName);

            // asercje - sprawdzenie, czy przekazujemy prawidłowy model do widoku
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }
    }
}
