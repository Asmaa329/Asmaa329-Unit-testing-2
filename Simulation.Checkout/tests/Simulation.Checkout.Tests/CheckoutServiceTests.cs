using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using Shouldly;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
namespace Simulation.Checkout.Tests
{
    public class CheckoutServiceTests
    {
        private readonly Mock<IOrderRepository> orderRepoMock = new Mock<IOrderRepository>();
        private readonly Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
        private readonly Mock<INotificationService> notificationMock = new Mock<INotificationService>();
        private readonly CheckoutService check;
        public CheckoutServiceTests()
        {
            check = new CheckoutService(
            orderRepoMock.Object,
            inventoryMock.Object,
            notificationMock.Object);

            inventoryMock
                .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        // ==========================================
        // TASK 1: BUSINESS RULES TESTS
        // ==========================================

        [Fact]
        public async Task Order_Empty()
        {
            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { }
            };
            await Should.ThrowAsync<EmptyOrderException>(async () => await check.CheckoutAsync(order));

        }



        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Order_Quantity_greater_than_zero(int quantity)
        {
            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = quantity, UnitPrice = 50 } }
            };
            var ex = await Should.ThrowAsync<InvalidQuantityException>(async () => await check.CheckoutAsync(order));
            ex.Message.ShouldBe($"Quantity for '{order.Items[0].ProductName}' must be greater than zero.");

        }


        [Fact]
        public async Task Order_Price_Lower_Than_Zero()
        {
            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = 2, UnitPrice = -0.01m } }
            };
            await Should.ThrowAsync<InvalidPriceException>(async () => await check.CheckoutAsync(order));

        }


        [Fact]
        public async Task Order_Price_Equal_Zero()
        {
            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = 2, UnitPrice = 0 } }
            };
            var receipt = await check.CheckoutAsync(order);
            receipt.Should().NotBeNull();
        }

        [Theory]
        [InlineData(999.99, 0)]
        [InlineData(1000, 100)]
        [InlineData(1500, 100)]
        public async Task Check_Discount(decimal subtotal, decimal expect)
        {

            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = 1, UnitPrice = subtotal } }
            };


            var receipt = await check.CheckoutAsync(order);
            receipt.Discount.Should().Be(expect);



        }
        

        //[Fact]
        //public async Task Order_Receipt_Check_Subtotal_Total()
        //{
        //    var order = new Order() { CustomerEmail = "asmaa@gmail.com", CustomerName = "asmaa", 
        //        Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = 3, UnitPrice = 500m } } };
        //    var orderRepoMock = new Mock<IOrderRepository>();
        //    var inventoryMock = new Mock<IInventoryService>();
        //    inventoryMock
        //          .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        //          .ReturnsAsync(true);
        //    var notificationMock = new Mock<INotificationService>();
        //    var check = new CheckoutService(
        //        orderRepoMock.Object,
        //        inventoryMock.Object,
        //        notificationMock.Object
        //    );
        //    var receipt =  await check.CheckoutAsync(order);
        //    receipt.Subtotal.ShouldBe(1500m);
        //    receipt.Discount.ShouldBe(100m);
        //    receipt.Total.ShouldBe(1400m);
        //}
        [Theory]
        [InlineData(3, 1, 500, 0, 1500, 100, 1400)]
        [InlineData(2, 1, 150, 400, 700, 0, 700)]
        public async Task Order_Receipt_Check_Subtotal_Total(int quantity1, int quantity2, decimal unitprice1, decimal unitprice2, decimal subtotal, decimal discount, decimal total)
        {
            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>()
                { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = quantity1, UnitPrice = unitprice1 },
                  new OrderItem { ProductId = Guid.NewGuid(), ProductName = "sandawtsh", Quantity = quantity2, UnitPrice = unitprice2 }}
            };

            var receipt = await check.CheckoutAsync(order);
            receipt.Subtotal.ShouldBe(subtotal);
            receipt.Discount.ShouldBe(discount);
            receipt.Total.ShouldBe(total);
        }

        // ==========================================
        // TASK 2: MOCKS TESTS (MOQ)
        // ==========================================

        [Fact]
        public async Task Order_Validate()
        {
            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = 2, UnitPrice = 0 } }
            };
            var receipt = await check.CheckoutAsync(order);
            receipt.Should().NotBeNull();

            orderRepoMock.Verify(x => x.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
            notificationMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Once);


        }


        [Fact]
        public async Task Check_WhenItemOutOfStock()
        {

            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "Out of stock Item", Quantity = 5, UnitPrice = 10m } }
            };

            inventoryMock
                .Setup(x => x.IsAvailableAsync(order.Items[0].ProductId, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var act = async () => await check.CheckoutAsync(order);

            await act.Should().ThrowAsync<InsufficientInventoryException>();
            orderRepoMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
            notificationMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Check_WhenOrderFailsValidation()
        {

            var order = new Order()
            {
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = 0, UnitPrice = 10m } }
            };


            var act = async () => await check.CheckoutAsync(order);
            await act.Should().ThrowAsync<InvalidQuantityException>();

            inventoryMock.Verify(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            orderRepoMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
            notificationMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ==========================================
        // TASK 3: TEST DATA TESTS (AUTOFIXTURE & BOGUS)
        // ==========================================

        [Fact]
        public async Task Fixture_Price_Quantity_Check()
        {
            var fixture = new Fixture();
            fixture.Customize<OrderItem>(c => c
             .With(x => x.Quantity, 2)
             .With(x => x.UnitPrice, 150m));
            var order = fixture.Create<Order>();
            var orderRepoMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            inventoryMock
                  .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);
            var notificationMock = new Mock<INotificationService>();
            var check = new CheckoutService(
                orderRepoMock.Object,
                inventoryMock.Object,
                notificationMock.Object
            );
            var receipt = await check.CheckoutAsync(order);
            receipt.Should().NotBeNull();
            receipt.Total.Should().BeGreaterThan(0);
        }


        static readonly Faker<Customer> Customers = new Faker<Customer>()
            .UseSeed(2026)
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Email, f => f.Internet.Email());
        [Fact]
        public async Task Bogus_check_Name_Email()
        {
            var customer = Customers.Generate();
            var order = new Order()
            {
                CustomerEmail = customer.Email,
                CustomerName = customer.Name,
                Items = new List<OrderItem>() { new OrderItem { ProductId = Guid.NewGuid(), ProductName = "pizaa", Quantity = 3, UnitPrice = 500m } }
            };
            var orderRepoMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            inventoryMock
                  .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);
            var notificationMock = new Mock<INotificationService>();
            var check = new CheckoutService(
                orderRepoMock.Object,
                inventoryMock.Object,
                notificationMock.Object
            );
            var receipt = await check.CheckoutAsync(order);
            customer.Name.ShouldBe(receipt.CustomerName);
            customer.Email.ShouldBe(receipt.CustomerEmail);


        }



    }
    public class Customer
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }





}
