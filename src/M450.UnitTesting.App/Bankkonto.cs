namespace M450.UnitTesting.App;

using System;

public class Bankkonto
{
    public string KontoNummer { get; } = Guid.NewGuid().ToString("N");
    public decimal Guthaben { get; private set; }

    public decimal AktivZins { get; }
    public decimal PassivZins { get; }
    public KontoStatus Status { get; }

    public decimal AufgelaufenerZins;

    public Bankkonto( decimal aktivZins, decimal passivZins, decimal guthaben = 0, KontoStatus status = KontoStatus.Standard )
    {
        ArgumentOutOfRangeException.ThrowIfNegative(aktivZins);
        ArgumentOutOfRangeException.ThrowIfNegative(passivZins);

        AktivZins = aktivZins;
        PassivZins = passivZins;
        Guthaben = guthaben;
        Status = status;
    }

    public decimal ZahleEin(decimal betrag)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(betrag);

        Guthaben += betrag;
        return Guthaben;
    }

    public decimal Beziehe(decimal betrag)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(betrag);

        Guthaben -= betrag;
        return Guthaben;
    }

    public decimal Transferiere(Bankkonto konto, decimal betrag)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(betrag);
        if (konto == this)
        {
            throw new ArgumentException("Quell- und Zielkonto dürfen nicht identisch sein.", nameof(konto));
        }

        Guthaben -= betrag;
        konto.ZahleEin(betrag);
        return Guthaben;
    }

    public decimal SchreibeZinsGut(int anzTage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(anzTage);

        decimal zinssatz = ErmittleZinssatz();
        AufgelaufenerZins += Guthaben * (zinssatz / 100) * anzTage / 360;

        return AufgelaufenerZins;
    }

    // Äquivalenzklassen für das Guthaben: [<0), [0, 10'000), [10'000, 50'000),
    // [50'000, 100'000) und [>=100'000). Ab 50'000 unterscheidet sich der
    // Zinszuschlag zusätzlich nach Status. Für Guthaben >= 100'000 gilt mangels
    // eigener Vorgabe weiterhin der Zuschlag der höchsten Stufe.
    private decimal ErmittleZinssatz()
    {
        if (Guthaben < 0)
        {
            return PassivZins;
        }

        if (Guthaben < 10_000m)
        {
            return AktivZins;
        }

        if (Guthaben < 50_000m)
        {
            return AktivZins + 0.5m;
        }

        return Status == KontoStatus.VIP
            ? AktivZins + 1.5m
            : AktivZins + 0.75m;
    }

    public void SchliesseKontoAb()
    {
        Guthaben += AufgelaufenerZins;
        AufgelaufenerZins = 0;
    }
}
