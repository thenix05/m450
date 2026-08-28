namespace M450.Bankkonto;

public class Bankkonto
{
    public decimal Guthaben { get; private set; }

    public void Einzahlen(decimal betrag)
    {
        Guthaben += betrag;
    }
}
