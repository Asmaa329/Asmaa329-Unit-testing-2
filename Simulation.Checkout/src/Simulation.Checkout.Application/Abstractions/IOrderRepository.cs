using System;
using System.Collections.Generic;
using System.Text;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Application.Abstractions
{
    public interface IOrderRepository
    {
        Task SaveAsync(Order order, CancellationToken cancellationToken = default);
    }
}
