namespace M450.Bankkonto;

public class Bankkonto
{
    public Bankkonto(decimal aktivZins = 0m, decimal passivZins = 0m)
    {
        AktivZins = aktivZins;
        PassivZins = passivZins;
    }

    public decimal Guthaben { get; private set; }

    public decimal AktivZins { get; }

    public decimal PassivZins { get; }

    public decimal AufgelaufenerZins { get; private set; }

    private const int BankjahrInTagen = 360;

    public void SchreibeZinsGut(int anzTage)
    {
        var zinsSatz = Guthaben >= 0m ? AktivZins : PassivZins;
        AufgelaufenerZins += Guthaben * zinsSatz * anzTage / (100m * BankjahrInTagen);
    }

    public void SchliesseKontoAb()
    {
        Guthaben += AufgelaufenerZins;
        AufgelaufenerZins = 0m;
    }

    public void Einzahlen(decimal betrag)
    {
        StellePositivenBetragSicher(betrag);
        Guthaben += betrag;
    }

    public void Beziehen(decimal betrag)
    {
        StellePositivenBetragSicher(betrag);
        Guthaben -= betrag;
    }

    private static void StellePositivenBetragSicher(decimal betrag)
    {
        if (betrag <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(betrag), "Der Betrag muss groesser als null sein.");
        }
    }
}
