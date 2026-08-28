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

    [Fact]
    public void Einzahlen_MitPositivemBetrag_ErhoehtGuthaben()
    {
        // Arrange
        var konto = new Bankkonto();

        // Act
        konto.Einzahlen(100m);

        // Assert
        Assert.Equal(100m, konto.Guthaben);
    }
}
