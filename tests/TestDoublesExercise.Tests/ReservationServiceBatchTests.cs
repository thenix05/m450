using Moq;
using TestDoublesExercise.Tests.Doubles;

namespace TestDoublesExercise.Tests;

// Fortgeschrittene Aufgabe: Gruppenreservierung mit ReserveBatch.
public sealed class ReservationServiceBatchTests
{
    private const string Concert = "KONZERT-01";
    private const string Theater = "THEATER-07";

    // ---------------------------------------------------------------
    // Ungültige Eingaben: Exception, bevor eine Abhängigkeit aufgerufen wird
    // ---------------------------------------------------------------

    [Fact]
    public void ReserveBatch_WithNullRequests_ThrowsArgumentNullException()
    {
        // Arrange
        var (service, availability, repository, sender) = CreateServiceWithSpies(new SeatAvailabilitySpy(Concert, 10));

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => service.ReserveBatch(null!));

        // Assert
        Assert.Equal("requests", exception.ParamName);
        AssertNoDependencyWasCalled(availability, repository, sender);
    }

    [Fact]
    public void ReserveBatch_WithEmptyRequests_ThrowsArgumentException()
    {
        // Arrange
        var (service, availability, repository, sender) = CreateServiceWithSpies(new SeatAvailabilitySpy(Concert, 10));

        // Act
        var exception = Assert.Throws<ArgumentException>(() => service.ReserveBatch([]));

        // Assert
        Assert.Equal("requests", exception.ParamName);
        AssertNoDependencyWasCalled(availability, repository, sender);
    }

    [Theory]
    [InlineData("", Concert)]
    [InlineData("   ", Concert)]
    [InlineData("anna@example.com", "")]
    [InlineData("anna@example.com", "   ")]
    public void ReserveBatch_WithBlankEmailOrEventCode_ThrowsBeforeCallingDependencies(string email, string eventCode)
    {
        // Arrange: Der ungültige Eintrag steht bewusst an zweiter Stelle.
        var (service, availability, repository, sender) = CreateServiceWithSpies(new SeatAvailabilitySpy(Concert, 10));
        Reservation[] requests =
        [
            new("ben@example.com", Concert, 1),
            new(email, eventCode, 1),
        ];

        // Act
        var exception = Assert.Throws<ArgumentException>(() => service.ReserveBatch(requests));

        // Assert
        Assert.Equal("requests", exception.ParamName);
        AssertNoDependencyWasCalled(availability, repository, sender);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReserveBatch_WithNonPositiveSeatCount_ThrowsBeforeCallingDependencies(int seatCount)
    {
        // Arrange
        var (service, availability, repository, sender) = CreateServiceWithSpies(new SeatAvailabilitySpy(Concert, 10));
        Reservation[] requests =
        [
            new("ben@example.com", Concert, 1),
            new("anna@example.com", Concert, seatCount),
        ];

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => service.ReserveBatch(requests));

        // Assert
        Assert.Equal("requests", exception.ParamName);
        AssertNoDependencyWasCalled(availability, repository, sender);
    }

    [Fact]
    public void ReserveBatch_WithNullEntry_ThrowsBeforeCallingDependencies()
    {
        // Arrange
        var (service, availability, repository, sender) = CreateServiceWithSpies(new SeatAvailabilitySpy(Concert, 10));
        Reservation[] requests = [new("ben@example.com", Concert, 1), null!];

        // Act
        var exception = Assert.Throws<ArgumentException>(() => service.ReserveBatch(requests));

        // Assert
        Assert.Equal("requests", exception.ParamName);
        AssertNoDependencyWasCalled(availability, repository, sender);
    }

    // ---------------------------------------------------------------
    // Verfügbarkeit pro Veranstaltung
    // ---------------------------------------------------------------

    [Fact]
    public void ReserveBatch_WhenCombinedSeatsExceedCapacity_DoesNotSaveOrSend()
    {
        // Arrange: Je 2 Plätze passen einzeln in 3 freie Plätze, zusammen (4) aber nicht.
        var availability = new SeatAvailabilityStub(Concert, 3);
        var repository = new Mock<IReservationRepository>();
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Concert, 2),
        ];

        // Act
        var result = service.ReserveBatch(requests);

        // Assert
        Assert.False(result);
        repository.Verify(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>()), Times.Never());
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WhenCombinedSeatsExactlyFitCapacity_ReturnsTrue()
    {
        // Arrange
        var service = new ReservationService(
            new SeatAvailabilityStub(Concert, 4),
            new AlwaysSuccessfulRepository(),
            new NoOpConfirmationSender());
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Concert, 2),
        ];

        // Act
        var result = service.ReserveBatch(requests);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ReserveBatch_QueriesAvailabilityOncePerDistinctEventCode()
    {
        // Arrange
        var availability = new SeatAvailabilitySpy(Concert, 10).With(Theater, 10);
        var service = new ReservationService(availability, new AlwaysSuccessfulRepository(), new NoOpConfirmationSender());
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 1),
            new("ben@example.com", Theater, 1),
            new("carla@example.com", Concert, 1),
        ];

        // Act
        service.ReserveBatch(requests);

        // Assert
        Assert.Equal(2, availability.RequestedEventCodes.Count);
        Assert.Single(availability.RequestedEventCodes, code => code == Concert);
        Assert.Single(availability.RequestedEventCodes, code => code == Theater);
    }

    [Fact]
    public void ReserveBatch_WhenOneOfSeveralEventsHasTooFewSeats_DoesNotSaveOrSend()
    {
        // Arrange: Konzert hat genug Plätze, Theater nicht.
        var availability = new SeatAvailabilityStub(Concert, 10).With(Theater, 1);
        var repository = new Mock<IReservationRepository>();
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Theater, 2),
        ];

        // Act
        var result = service.ReserveBatch(requests);

        // Assert
        Assert.False(result);
        repository.Verify(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>()), Times.Never());
        Assert.Empty(sender.SentReservations);
    }

    // ---------------------------------------------------------------
    // Speichern und Bestätigen
    // ---------------------------------------------------------------

    [Fact]
    public void ReserveBatch_WithEnoughSeats_SavesAllInOneBatchAndSendsConfirmationsInInputOrder()
    {
        // Arrange
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 2),
            new("ben@example.com", Theater, 1),
            new("carla@example.com", Concert, 1),
        ];
        var availability = new SeatAvailabilityStub(Concert, 3).With(Theater, 1);
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>())).Returns(true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository.Object, sender);

        // Act
        var result = service.ReserveBatch(requests);

        // Assert
        Assert.True(result);
        repository.Verify(r => r.TrySaveBatch(It.Is<IReadOnlyList<Reservation>>(saved =>
            saved.Count == 3 &&
            saved[0].CustomerEmail == "anna@example.com" && saved[0].EventCode == Concert && saved[0].SeatCount == 2 &&
            saved[1].CustomerEmail == "ben@example.com" && saved[1].EventCode == Theater && saved[1].SeatCount == 1 &&
            saved[2].CustomerEmail == "carla@example.com" && saved[2].EventCode == Concert && saved[2].SeatCount == 1)),
            Times.Once());
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        Assert.Equal<Reservation>(requests, sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WhenBatchSaveFails_ReturnsFalseAndSendsNothing()
    {
        // Arrange
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 1),
            new("ben@example.com", Concert, 1),
        ];
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySaveBatch(It.IsAny<IReadOnlyList<Reservation>>())).Returns(false);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(new SeatAvailabilityStub(Concert, 5), repository.Object, sender);

        // Act
        var result = service.ReserveBatch(requests);

        // Assert
        Assert.False(result);
        repository.Verify(r => r.TrySaveBatch(It.Is<IReadOnlyList<Reservation>>(saved =>
            saved.Count == 2 && saved[0] == requests[0] && saved[1] == requests[1])), Times.Once());
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void ReserveBatch_WithEnoughSeats_PassesAllReservationsToRepositorySpy()
    {
        // Arrange
        Reservation[] requests =
        [
            new("anna@example.com", Concert, 1),
            new("ben@example.com", Concert, 1),
        ];
        var repository = new RepositorySpy(saveResult: true);
        var service = new ReservationService(new SeatAvailabilityStub(Concert, 2), repository, new NoOpConfirmationSender());

        // Act
        var result = service.ReserveBatch(requests);

        // Assert
        Assert.True(result);
        var batch = Assert.Single(repository.SavedBatches);
        Assert.Equal<Reservation>(requests, batch);
        Assert.Empty(repository.SavedReservations);
    }

    private static (ReservationService Service, SeatAvailabilitySpy Availability, RepositorySpy Repository, ConfirmationSenderSpy Sender)
        CreateServiceWithSpies(SeatAvailabilitySpy availability)
    {
        var repository = new RepositorySpy();
        var sender = new ConfirmationSenderSpy();
        return (new ReservationService(availability, repository, sender), availability, repository, sender);
    }

    private static void AssertNoDependencyWasCalled(
        SeatAvailabilitySpy availability,
        RepositorySpy repository,
        ConfirmationSenderSpy sender)
    {
        Assert.Empty(availability.RequestedEventCodes);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(repository.SavedBatches);
        Assert.Empty(sender.SentReservations);
    }
}
