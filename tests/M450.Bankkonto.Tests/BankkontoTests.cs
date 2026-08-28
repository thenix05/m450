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

    [Fact]
    public void SchreibeZinsGut_BeiNegativemGuthaben_VerwendetPassivZins()
    {
        // Arrange
        var konto = new Bankkonto(aktivZins: 3.6m, passivZins: 7.2m);
        konto.Beziehen(1_000m);

        // Act
        konto.SchreibeZinsGut(30);

        // Assert
        Assert.Equal(-6m, konto.AufgelaufenerZins);
        Assert.Equal(-1_000m, konto.Guthaben);
    }

    [Fact]
    public void SchliesseKontoAb_VerbuchtAufgelaufenenZinsUndSetztIhnZurueck()
    {
        // Arrange
        var konto = new Bankkonto(aktivZins: 3.6m, passivZins: 7.2m);
        konto.Einzahlen(1_000m);
        konto.SchreibeZinsGut(30);

        // Act
        konto.SchliesseKontoAb();

        // Assert
        Assert.Equal(1_003m, konto.Guthaben);
        Assert.Equal(0m, konto.AufgelaufenerZins);
    }

    /// <summary>
    /// Validiert den in der Aufgabenstellung vorgegebenen Jahresablauf:
    /// Aktivzins 3.6 %, Passivzins 7.2 %, Kontoeroeffnung am 1. Maerz mit
    /// Buchungen bis zum Kontoabschluss am 31. Dezember. Die Bank rechnet
    /// mit Monaten zu 30 Tagen (360-Tage-Jahr).
    /// </summary>
    [Fact]
    public void Kontoabschluss_NachJahresablaufMitVorgegebenenBuchungen_VerbuchtGesamtzins()
    {
        // Arrange
        var konto = new Bankkonto(aktivZins: 3.6m, passivZins: 7.2m);

        // Act
        // 1. Maerz: Konto eroeffnen und CHF 1'000 einzahlen.
        konto.Einzahlen(1_000m);

        // 1. Juli (120 Tage spaeter): Zins auf CHF 1'000 gutschreiben, dann CHF 1'000 einzahlen.
        konto.SchreibeZinsGut(120);
        konto.Einzahlen(1_000m);

        // 1. August (30 Tage spaeter): Zins auf CHF 2'000 gutschreiben, dann CHF 3'000 beziehen.
        konto.SchreibeZinsGut(30);
        konto.Beziehen(3_000m);

        // 1. Oktober (60 Tage spaeter): Zins auf CHF -1'000 gutschreiben, dann CHF 2'000 einzahlen.
        konto.SchreibeZinsGut(60);
        konto.Einzahlen(2_000m);

        // 31. Dezember (90 Tage spaeter): Zins auf CHF 1'000 gutschreiben und Konto abschliessen.
        konto.SchreibeZinsGut(90);
        konto.SchliesseKontoAb();

        // Assert
        // Buchungssaldo CHF 1'000 + Gesamtzins CHF 15 = CHF 1'015.
        Assert.Equal(1_015m, konto.Guthaben);
        Assert.Equal(0m, konto.AufgelaufenerZins);
    }
}
