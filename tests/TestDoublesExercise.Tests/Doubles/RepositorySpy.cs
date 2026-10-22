namespace TestDoublesExercise.Tests.Doubles;

// Spy für das Repository: liefert ein festgelegtes Ergebnis und merkt sich alle Speicherversuche.
internal sealed class RepositorySpy(bool saveResult = true) : IReservationRepository
{
    private readonly List<Reservation> _savedReservations = [];
    private readonly List<IReadOnlyList<Reservation>> _savedBatches = [];

    public IReadOnlyList<Reservation> SavedReservations => _savedReservations;

    public IReadOnlyList<IReadOnlyList<Reservation>> SavedBatches => _savedBatches;

    public bool TrySave(Reservation reservation)
    {
        _savedReservations.Add(reservation);
        return saveResult;
    }

    public bool TrySaveBatch(IReadOnlyList<Reservation> reservations)
    {
        _savedBatches.Add(reservations.ToArray());
        return saveResult;
    }
}
