using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void CreateMoney_WithValidData_ShouldSucceed()
    {
        var money = new Money(100m, "USD");

        Assert.Equal(100m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Add_TwoSameCurrency_ShouldReturnSum()
    {
        var m1 = new Money(50m, "USD");
        var m2 = new Money(30m, "USD");

        var result = m1.Add(m2);

        Assert.Equal(80m, result.Amount);
    }

    [Fact]
    public void Add_DifferentCurrencies_ShouldThrow()
    {
        var m1 = new Money(50m, "USD");
        var m2 = new Money(30m, "EUR");

        Assert.Throws<InvalidOperationException>(() => m1.Add(m2));
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_ShouldBeEqual()
    {
        var m1 = new Money(100m, "USD");
        var m2 = new Money(100m, "USD");

        Assert.Equal(m1, m2);
    }
}
