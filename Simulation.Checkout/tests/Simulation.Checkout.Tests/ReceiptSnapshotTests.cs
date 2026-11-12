using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ReceiptSnapshotTests
    {
        [Fact]
        public Task Receipt_Check()
        {
            var date = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var orderid = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var productid = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var receipt = new CheckoutReceipt()
            {
                CreatedAt = date,
                OrderId = orderid,
                CustomerEmail = "asmaa@gmail.com",
                CustomerName = "asmaa",
                Items = new List<ReceiptLine>() { new ReceiptLine { ProductId = productid, ProductName = "pizaa", Quantity = 3, UnitPrice = 500m, LineTotal = 1400m } },
                Discount = 100m,
                Subtotal = 1500m,
                Total = 1400m
            };
            return Verify(receipt);
        }

    }
}
