namespace TestDoublesExercise.Tests.Doubles;

// Spy: verschickt nichts, sondern zeichnet alle übergebenen Reservierungen in ihrer Reihenfolge auf.
internal sealed class ConfirmationSenderSpy : IConfirmationSender
{
    private readonly List<Reservation> _sentReservations = [];

    public IReadOnlyList<Reservation> SentReservations => _sentReservations;

    public void Send(Reservation reservation) => _sentReservations.Add(reservation);
}
