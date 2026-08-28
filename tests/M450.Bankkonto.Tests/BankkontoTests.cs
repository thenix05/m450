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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Einzahlen_MitNichtPositivemBetrag_WirftException(int betrag)
    {
        // Arrange
        var konto = new Bankkonto();

        // Act
        var exception = Record.Exception(() => konto.Einzahlen(betrag));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public void Beziehen_MitBetragUeberGuthaben_ErlaubtUeberziehung()
    {
        // Arrange
        var konto = new Bankkonto();
        konto.Einzahlen(100m);

        // Act
        konto.Beziehen(150m);

        // Assert
        Assert.Equal(-50m, konto.Guthaben);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Beziehen_MitNichtPositivemBetrag_WirftException(int betrag)
    {
        // Arrange
        var konto = new Bankkonto();

        // Act
        var exception = Record.Exception(() => konto.Beziehen(betrag));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public void NeuesKonto_MitZinssaetzen_MerktSichAktivUndPassivZins()
    {
        // Arrange

        // Act
        var konto = new Bankkonto(aktivZins: 3.6m, passivZins: 7.2m);

        // Assert
        Assert.Equal(3.6m, konto.AktivZins);
        Assert.Equal(7.2m, konto.PassivZins);
    }

    [Fact]
    public void SchreibeZinsGut_BeiPositivemGuthaben_VerwendetAktivZins()
    {
        // Arrange
        var konto = new Bankkonto(aktivZins: 3.6m, passivZins: 7.2m);
        konto.Einzahlen(1_000m);

        // Act
        konto.SchreibeZinsGut(120);

        // Assert
        Assert.Equal(12m, konto.AufgelaufenerZins);
        Assert.Equal(1_000m, konto.Guthaben);
    }
}
