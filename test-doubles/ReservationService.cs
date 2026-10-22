namespace TestDoublesExercise;

public sealed record Reservation(string CustomerEmail, string EventCode, int SeatCount);

// Für diese Schnittstelle existiert in der Übung noch keine produktive Implementierung.
public interface ISeatAvailability
{
    int GetAvailableSeats(string eventCode);
}

public interface IReservationRepository
{
    // Gibt false zurück, wenn die Reservierung nicht gespeichert werden konnte.
    bool TrySave(Reservation reservation);

    // Speichert alle Reservierungen atomar: entweder alle oder keine.
    // Gibt false zurück, wenn nichts gespeichert werden konnte.
    bool TrySaveBatch(IReadOnlyList<Reservation> reservations);
}

public interface IConfirmationSender
{
    void Send(Reservation reservation);
}

public sealed class ReservationService
{
    private readonly ISeatAvailability _availability;
    private readonly IReservationRepository _repository;
    private readonly IConfirmationSender _sender;

    public ReservationService(
        ISeatAvailability availability,
        IReservationRepository repository,
        IConfirmationSender sender)
    {
        _availability = availability ?? throw new ArgumentNullException(nameof(availability));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public bool Reserve(string customerEmail, string eventCode, int seatCount)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new ArgumentException("Eine E-Mail-Adresse ist erforderlich.", nameof(customerEmail));
        }

        if (string.IsNullOrWhiteSpace(eventCode))
        {
            throw new ArgumentException("Ein Veranstaltungscode ist erforderlich.", nameof(eventCode));
        }

        if (seatCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seatCount));
        }

        if (_availability.GetAvailableSeats(eventCode) < seatCount)
        {
            return false;
        }

        var reservation = new Reservation(customerEmail, eventCode, seatCount);
        if (!_repository.TrySave(reservation))
        {
            return false;
        }

        _sender.Send(reservation);
        return true;
    }

    public bool ReserveBatch(IReadOnlyList<Reservation> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        // Kopie, damit Validierung, Speichern und Versand mit denselben Einträgen arbeiten.
        var reservations = requests.ToArray();
        if (reservations.Length == 0)
        {
            throw new ArgumentException("Mindestens eine Reservierung ist erforderlich.", nameof(requests));
        }

        foreach (var reservation in reservations)
        {
            Validate(reservation, nameof(requests));
        }

        var requestedSeatsPerEvent = reservations
            .GroupBy(reservation => reservation.EventCode)
            .Select(group => (EventCode: group.Key, Seats: group.Sum(reservation => reservation.SeatCount)));

        foreach (var (eventCode, seats) in requestedSeatsPerEvent)
        {
            if (_availability.GetAvailableSeats(eventCode) < seats)
            {
                return false;
            }
        }

        if (!_repository.TrySaveBatch(reservations))
        {
            return false;
        }

        foreach (var reservation in reservations)
        {
            _sender.Send(reservation);
        }

        return true;
    }

    private static void Validate(Reservation? reservation, string paramName)
    {
        if (reservation is null)
        {
            throw new ArgumentException("Eine Reservierung darf nicht null sein.", paramName);
        }

        if (string.IsNullOrWhiteSpace(reservation.CustomerEmail))
        {
            throw new ArgumentException("Eine E-Mail-Adresse ist erforderlich.", paramName);
        }

        if (string.IsNullOrWhiteSpace(reservation.EventCode))
        {
            throw new ArgumentException("Ein Veranstaltungscode ist erforderlich.", paramName);
        }

        if (reservation.SeatCount <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Die Platzanzahl muss positiv sein.");
        }
    }
}
