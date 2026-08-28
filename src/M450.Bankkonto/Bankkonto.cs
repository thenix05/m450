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
