using System;
using System.Collections.Generic;
using System.Text;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Application.Abstractions
{
    public interface INotificationService
    {
        Task SendOrderConfirmationAsync(
            CheckoutReceipt receipt,
            CancellationToken cancellationToken = default);
    }
}
