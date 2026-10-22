namespace TestDoublesExercise.Tests.Doubles;

// Einfaches Repository-Double für Aufgabe 1: Speichern gelingt immer, nichts wird aufgezeichnet.
internal sealed class AlwaysSuccessfulRepository : IReservationRepository
{
    public bool TrySave(Reservation reservation) => true;

    public bool TrySaveBatch(IReadOnlyList<Reservation> reservations) => true;
}
