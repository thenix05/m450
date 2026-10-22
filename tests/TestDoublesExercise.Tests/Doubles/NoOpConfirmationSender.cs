namespace TestDoublesExercise.Tests.Doubles;

// Sender-Double für Aufgabe 1: tut nichts und verschickt keine echte Nachricht.
internal sealed class NoOpConfirmationSender : IConfirmationSender
{
    public void Send(Reservation reservation)
    {
    }
}
