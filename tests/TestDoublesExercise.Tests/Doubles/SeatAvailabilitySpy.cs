namespace TestDoublesExercise.Tests.Doubles;

// Spy: liefert dieselben Werte wie der Stub, merkt sich aber zusätzlich jede Abfrage.
// So wird sichtbar, ob (und wie oft) die Verfügbarkeit abgefragt wurde.
internal sealed class SeatAvailabilitySpy : ISeatAvailability
{
    private readonly SeatAvailabilityStub _stub;
    private readonly List<string> _requestedEventCodes = [];

    public SeatAvailabilitySpy(string eventCode, int availableSeats)
    {
        _stub = new SeatAvailabilityStub(eventCode, availableSeats);
    }

    public IReadOnlyList<string> RequestedEventCodes => _requestedEventCodes;

    public SeatAvailabilitySpy With(string eventCode, int availableSeats)
    {
        _stub.With(eventCode, availableSeats);
        return this;
    }

    public int GetAvailableSeats(string eventCode)
    {
        _requestedEventCodes.Add(eventCode);
        return _stub.GetAvailableSeats(eventCode);
    }
}
