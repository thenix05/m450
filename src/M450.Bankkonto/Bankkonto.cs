namespace M450.Bankkonto;

public class Bankkonto
{
    public decimal Guthaben { get; private set; }

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
