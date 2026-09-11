namespace M450.UnitTesting.App.Tests;

public sealed class BankkontoTests
{
    [Fact]
    public void Konstruktor_ErstelltKontenMitEindeutigenNummern()
    {
        // Arrange

        // Act
        var erstesKonto = NeuesKonto();
        var zweitesKonto = NeuesKonto();

        // Assert
        Assert.NotEqual(erstesKonto.KontoNummer, zweitesKonto.KontoNummer);
    }

    [Fact]
    public void ZahleEin_MitPositivemBetrag_ErhoehtGuthaben()
    {
        // Arrange
        var konto = NeuesKonto(startGuthaben: 100m);

        // Act
        konto.ZahleEin(25m);

        // Assert
        Assert.Equal(125m, konto.Guthaben);
    }

    [Fact]
    public void Beziehe_MehrAlsGuthaben_ErlaubtUeberziehung()
    {
        // Arrange
        var konto = NeuesKonto(startGuthaben: 100m);

        // Act
        konto.Beziehe(125m);

        // Assert
        Assert.Equal(-25m, konto.Guthaben);
    }

    [Fact]
    public void Transferiere_MitPositivemBetrag_BelastetUndBeguenstigtKonten()
    {
        // Arrange
        var quellKonto = NeuesKonto(startGuthaben: 100m);
        var zielKonto = NeuesKonto(startGuthaben: 10m);

        // Act
        quellKonto.Transferiere(zielKonto, 40m);

        // Assert
        Assert.Equal(60m, quellKonto.Guthaben);
        Assert.Equal(50m, zielKonto.Guthaben);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GeldOperationen_MitNichtPositivemBetrag_WerfenException(int betrag)
    {
        // Arrange
        var konto = NeuesKonto();
        var zielKonto = NeuesKonto();

        // Act
        var einzahlenException = Record.Exception(() => konto.ZahleEin(betrag));
        var beziehenException = Record.Exception(() => konto.Beziehe(betrag));
        var transferException = Record.Exception(
            () => konto.Transferiere(zielKonto, betrag));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(einzahlenException);
        Assert.IsType<ArgumentOutOfRangeException>(beziehenException);
        Assert.IsType<ArgumentOutOfRangeException>(transferException);
    }

    [Fact]
    public void Transferiere_AufDasselbeKonto_WirftExceptionUndAendertNichts()
    {
        // Arrange
        var konto = NeuesKonto(startGuthaben: 100m);

        // Act
        var exception = Record.Exception(() => konto.Transferiere(konto, 10m));

        // Assert
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal(100m, konto.Guthaben);
    }

    [Fact]
    public void SchreibeZinsGut_BeiPositivemGuthaben_VerwendetAktivZins()
    {
        // Arrange
        var konto = NeuesKonto(aktivZins: 3.6m, startGuthaben: 1_000m);

        // Act
        konto.SchreibeZinsGut(30);

        // Assert
        Assert.Equal(3m, konto.AufgelaufenerZins);
        Assert.Equal(1_000m, konto.Guthaben);
    }

    [Fact]
    public void SchreibeZinsGut_BeiNegativemGuthaben_VerwendetPassivZins()
    {
        // Arrange
        var konto = NeuesKonto(passivZins: 7.2m, startGuthaben: -1_000m);

        // Act
        konto.SchreibeZinsGut(30);

        // Assert
        Assert.Equal(-6m, konto.AufgelaufenerZins);
        Assert.Equal(-1_000m, konto.Guthaben);
    }

    [Fact]
    public void Konstruktor_OhneStatusAngabe_SetztStandardAlsStatus()
    {
        // Arrange & Act
        var konto = new Bankkonto(1.5m, 5m);

        // Assert
        Assert.Equal(KontoStatus.Standard, konto.Status);
    }

    [Fact]
    public void Konstruktor_MitVipStatus_SetztStatusAufVip()
    {
        // Arrange & Act
        var konto = new Bankkonto(1.5m, 5m, status: KontoStatus.VIP);

        // Assert
        Assert.Equal(KontoStatus.VIP, konto.Status);
    }

    [Theory]
    [InlineData(0, KontoStatus.Standard, 2)]
    [InlineData(9999.99, KontoStatus.Standard, 2)]
    [InlineData(10000, KontoStatus.Standard, 2.5)]
    [InlineData(49999.99, KontoStatus.Standard, 2.5)]
    [InlineData(50000, KontoStatus.Standard, 2.75)]
    [InlineData(99999.99, KontoStatus.Standard, 2.75)]
    [InlineData(100000, KontoStatus.Standard, 2.75)]
    [InlineData(50000, KontoStatus.VIP, 3.5)]
    [InlineData(99999.99, KontoStatus.VIP, 3.5)]
    [InlineData(100000, KontoStatus.VIP, 3.5)]
    public void SchreibeZinsGut_BeiGuthabenKlassenUndStatus_WendetErwartetenZinssatzAn(
        decimal guthaben, KontoStatus status, decimal erwarteterZinssatz)
    {
        // Arrange
        var konto = NeuesKonto(aktivZins: 2m, passivZins: 5m, startGuthaben: guthaben, status: status);
        var erwarteterZins = guthaben * erwarteterZinssatz / 100m;

        // Act
        konto.SchreibeZinsGut(360);

        // Assert
        Assert.Equal(erwarteterZins, konto.AufgelaufenerZins);
    }

    [Theory]
    [InlineData(KontoStatus.Standard)]
    [InlineData(KontoStatus.VIP)]
    public void SchreibeZinsGut_BeiNegativemGuthabenGrenzwert_VerwendetPassivZinsUnabhaengigVomStatus(
        KontoStatus status)
    {
        // Arrange
        var konto = NeuesKonto(aktivZins: 2m, passivZins: 5m, startGuthaben: -0.01m, status: status);
        var erwarteterZins = -0.01m * 5m / 100m;

        // Act
        konto.SchreibeZinsGut(360);

        // Assert
        Assert.Equal(erwarteterZins, konto.AufgelaufenerZins);
    }

    [Fact]
    public void SchreibeZinsGut_Mehrmals_BerechnetKeinenZinseszins()
    {
        // Arrange
        var konto = NeuesKonto(aktivZins: 3.6m, startGuthaben: 1_000m);

        // Act
        konto.SchreibeZinsGut(30);
        konto.SchreibeZinsGut(30);

        // Assert
        Assert.Equal(6m, konto.AufgelaufenerZins);
        Assert.Equal(1_000m, konto.Guthaben);
    }

    [Fact]
    public void SchliesseKontoAb_VerbuchtenZinsUndSetztZinsspeicherZurueck()
    {
        // Arrange
        var konto = NeuesKonto(aktivZins: 3.6m, startGuthaben: 1_000m);
        konto.SchreibeZinsGut(30);

        // Act
        konto.SchliesseKontoAb();

        // Assert
        Assert.Equal(1_003m, konto.Guthaben);
        Assert.Equal(0m, konto.AufgelaufenerZins);
    }

    [Fact]
    public void Konstruktor_MitNegativemZinssatz_WirftException()
    {
        // Arrange

        // Act
        var aktivZinsException = Record.Exception(() => new Bankkonto(-0.1m, 5m));
        var passivZinsException = Record.Exception(() => new Bankkonto(1m, -0.1m));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(aktivZinsException);
        Assert.IsType<ArgumentOutOfRangeException>(passivZinsException);
    }

    [Fact]
    public void SchreibeZinsGut_MitNegativenTagen_WirftException()
    {
        // Arrange
        var konto = NeuesKonto();

        // Act
        var exception = Record.Exception(() => konto.SchreibeZinsGut(-1));

        // Assert
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public void Kontoabschluss_NachBuchungenImJahresverlauf_VerbuchtenGesamtzins()
    {
        // Arrange
        var konto = NeuesKonto(aktivZins: 3.6m, passivZins: 7.2m);

        // Act
        // 1. März: Konto eröffnen und CHF 1'000 einzahlen.
        konto.ZahleEin(1_000m);

        // 1. Juli: Zins für 120 Tage berechnen und CHF 1'000 einzahlen.
        konto.SchreibeZinsGut(120);
        konto.ZahleEin(1_000m);

        // 1. August: Zins für 30 Tage berechnen und CHF 3'000 beziehen.
        konto.SchreibeZinsGut(30);
        konto.Beziehe(3_000m);

        // 1. Oktober: Passivzins für 60 Tage berechnen und CHF 2'000 einzahlen.
        konto.SchreibeZinsGut(60);
        konto.ZahleEin(2_000m);

        // 31. Dezember: Zins für 90 Tage berechnen und das Konto abschliessen.
        konto.SchreibeZinsGut(90);
        konto.SchliesseKontoAb();

        // Assert
        // Buchungssaldo CHF 1'000 + Gesamtzins CHF 15 = CHF 1'015.
        Assert.Equal(1_015m, konto.Guthaben);
        Assert.Equal(0m, konto.AufgelaufenerZins);
    }

    // Diese Hilfsmethode vermeidet wiederholte Konstruktoraufrufe mit denselben
    // Standardwerten. Einzelne Tests können benötigte Werte über benannte Parameter
    // gezielt ändern und ihr Arrange-Abschnitt bleibt kurz und gut lesbar.
    private static Bankkonto NeuesKonto(
        decimal aktivZins = 1.5m,
        decimal passivZins = 5m,
        decimal startGuthaben = 0m,
        KontoStatus status = KontoStatus.Standard)
    {
        return new Bankkonto(aktivZins, passivZins, startGuthaben, status);
    }
}
