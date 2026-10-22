namespace TestDoublesExercise.Tests.Doubles;

// Stub: liefert für festgelegte Veranstaltungscodes eine fixe Anzahl freier Plätze.
// Er berechnet keine echte Verfügbarkeit und zeichnet keine Aufrufe auf.
// Annahme: Für einen unbekannten Veranstaltungscode sind 0 Plätze frei.
internal sealed class SeatAvailabilityStub : ISeatAvailability
{
    private readonly Dictionary<string, int> _availableSeats = new();

    public SeatAvailabilityStub(string eventCode, int availableSeats)
    {
        With(eventCode, availableSeats);
    }

    public SeatAvailabilityStub With(string eventCode, int availableSeats)
    {
        _availableSeats[eventCode] = availableSeats;
        return this;
    }

    public int GetAvailableSeats(string eventCode) =>
        _availableSeats.TryGetValue(eventCode, out var seats) ? seats : 0;
}
