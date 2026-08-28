using M450.UnitTesting.App;

namespace M450.UnitTesting.App.Tests;

public class PriceCalculatorTests
{
    [Fact]
    public void CalculateTotal_WithoutDiscount_ReturnsExpectedTotal()
    {
        // Arrange
        var calculator = new PriceCalculator();
        var unitPrice = 25.50m;
        var quantity = 3;
        var expectedTotal = 76.50m;

        // Act
        var actualTotal = calculator.CalculateTotal(unitPrice, quantity);

        // Assert
        Assert.Equal(expectedTotal, actualTotal);
    }

    [Fact]
    public void CalculateTotal_WithDiscount_ReturnsExpectedTotal()
    {
        // Arrange
        var calculator = new PriceCalculator();
        var unitPrice = 25.50m;
        var quantity = 3;
        var discountPercentage = 10m;
        var expectedTotal = 68.85m;

        // Act
        var actualTotal = calculator.CalculateTotal(unitPrice, quantity, discountPercentage);

        // Assert
        Assert.Equal(expectedTotal, actualTotal);
    }

    [Fact]
    public void CalculateTotal_WithQuantityOfZero_ReturnsZero()
    {
        // Arrange
        var calculator = new PriceCalculator();
        var unitPrice = 25.50m;
        var quantity = 0;
        var expectedTotal = 0m;

        // Act
        var actualTotal = calculator.CalculateTotal(unitPrice, quantity);

        // Assert
        Assert.Equal(expectedTotal, actualTotal);
    }

    [Fact]
    public void CalculateTotal_WithFullDiscount_ReturnsZero()
    {
        // Arrange
        var calculator = new PriceCalculator();
        var unitPrice = 25.50m;
        var quantity = 3;
        var discountPercentage = 100m;
        var expectedTotal = 0m;

        // Act
        var actualTotal = calculator.CalculateTotal(unitPrice, quantity, discountPercentage);

        // Assert
        Assert.Equal(expectedTotal, actualTotal);
    }

    // Mehrere Beispiele fuer dasselbe Verhalten: eine Theory statt vier Facts.
    // Achtung: decimal ist in C# als Attribut-Wert nicht erlaubt, darum stehen
    // hier double-Werte, die im Test nach decimal umgewandelt werden.
    [Theory]
    [InlineData(10.00, 1, 50, 5.00)]
    [InlineData(19.99, 2, 0, 39.98)]
    [InlineData(100.00, 4, 25, 300.00)]
    public void CalculateTotal_WithValidValues_ReturnsExpectedTotal(
        double unitPrice,
        int quantity,
        double discountPercentage,
        double expectedTotal)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act
        var actualTotal = calculator.CalculateTotal(
            (decimal)unitPrice,
            quantity,
            (decimal)discountPercentage);

        // Assert
        Assert.Equal((decimal)expectedTotal, actualTotal);
    }

    [Fact]
    public void CalculateTotal_WithNegativeUnitPrice_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var calculator = new PriceCalculator();
        var unitPrice = -1.00m;
        var quantity = 3;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.CalculateTotal(unitPrice, quantity));
    }

    [Fact]
    public void CalculateTotal_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var calculator = new PriceCalculator();
        var unitPrice = 25.50m;
        var quantity = -1;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.CalculateTotal(unitPrice, quantity));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    [InlineData(101)]
    public void CalculateTotal_WithInvalidDiscount_ThrowsArgumentOutOfRangeException(
        double discountPercentage)
    {
        // Arrange
        var calculator = new PriceCalculator();
        var unitPrice = 25.50m;
        var quantity = 3;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.CalculateTotal(unitPrice, quantity, (decimal)discountPercentage));
    }
}
