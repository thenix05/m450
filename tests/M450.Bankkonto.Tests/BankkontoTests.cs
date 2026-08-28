using M450.Bankkonto;

namespace M450.Bankkonto.Tests;

public class BankkontoTests
{
    [Fact]
    public void NeuesKonto_OhneEinzahlung_HatGuthabenNull()
    {
        // Arrange

        // Act
        var konto = new Bankkonto();

        // Assert
        Assert.Equal(0m, konto.Guthaben);
    }
}
