using FluentAssertions;
using NetArchTest.Rules;
using Shouldly;
using Simulation.Checkout.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ArchitectureTests
    {
        [Fact]
        public void Domain_InDependeon_Infrastructure()
        {
            var assembly = typeof(Domain.Order).Assembly;
            var result = NetArchTest.Rules.Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace("Simulation.Checkout.Domain")
                .Should()
                .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();
            result.IsSuccessful.Should().BeTrue("Domain layer should not depend on Infrastructure layer");
        }


        [Fact]
        public void Application_InDependeon_Infrastructure()
        {
            var assembly = typeof(Application.CheckoutService).Assembly;
            var result = NetArchTest.Rules.Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace("Simulation.Checkout.Application")
                .Should()
                .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();
            result.IsSuccessful.Should().BeTrue("Application layer should not depend on Infrastructure layer");
        }



        [Fact]
        public void Interfaces_Startwith_I()
        {
            var assembly = typeof(Application.CheckoutService).Assembly;
            var result = NetArchTest.Rules.Types
               .InAssembly(assembly)
               .That()
               .AreInterfaces()
               .Should()
               .HaveNameStartingWith("I")
               .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }


        [Fact]
        public void Classes_That_Implement_IOrderRepository_Endwith_Repository()
        {
            var assembly = typeof(Infrastructure.InMemoryOrderRepository).Assembly;
            var result = NetArchTest.Rules.Types
               .InAssembly(assembly)
               .That()
               .ImplementInterface(typeof(Application.Abstractions.IOrderRepository))
               .Should()
               .HaveNameEndingWith("Repository")
               .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
