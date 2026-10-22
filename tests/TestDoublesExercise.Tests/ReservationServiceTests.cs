using Moq;
using TestDoublesExercise.Tests.Doubles;

namespace TestDoublesExercise.Tests;

// Antworten zu den Aufgaben 1, 3 und 5 sowie zur Fortgeschrittenen-Aufgabe: siehe test-doubles/ANTWORTEN.md
public sealed class ReservationServiceTests
{
    private const string Email = "anna@example.com";
    private const string EventCode = "KONZERT-01";

    // ---------------------------------------------------------------
    // Aufgabe 1: Nur die Schnittstelle ist bekannt (eigener Stub)
    // ---------------------------------------------------------------

    [Fact]
    public void Reserve_WithEnoughSeats_ReturnsTrue()
    {
        // Arrange
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 3);
        var service = new ReservationService(availability, new AlwaysSuccessfulRepository(), new NoOpConfirmationSender());

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Reserve_WithTooFewSeats_ReturnsFalse()
    {
        // Arrange
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 1);
        var service = new ReservationService(availability, new AlwaysSuccessfulRepository(), new NoOpConfirmationSender());

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Reserve_WithExactlyEnoughSeats_Succeeds()
    {
        // Arrange
        var availability = new SeatAvailabilityStub(EventCode, availableSeats: 2);
        var service = new ReservationService(availability, new AlwaysSuccessfulRepository(), new NoOpConfirmationSender());

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.True(result);
    }

    // ---------------------------------------------------------------
    // Aufgabe 2: Grenzfälle und Isolation
    // Fehlende Aufrufe werden mit Spies sichtbar gemacht, die jeden Aufruf aufzeichnen.
    // ---------------------------------------------------------------

    [Fact]
    public void Reserve_WithZeroSeats_ThrowsBeforeCallingDependencies()
    {
        // Arrange
        var availability = new SeatAvailabilitySpy(EventCode, availableSeats: 10);
        var repository = new RepositorySpy();
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => service.Reserve(Email, EventCode, 0));

        // Assert
        Assert.Equal("seatCount", exception.ParamName);
        Assert.Empty(availability.RequestedEventCodes);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithNegativeSeats_ThrowsBeforeCallingDependencies()
    {
        // Arrange
        var availability = new SeatAvailabilitySpy(EventCode, availableSeats: 10);
        var repository = new RepositorySpy();
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => service.Reserve(Email, EventCode, -1));

        // Assert
        Assert.Equal("seatCount", exception.ParamName);
        Assert.Empty(availability.RequestedEventCodes);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithEmptyEventCode_ThrowsBeforeCallingDependencies()
    {
        // Arrange
        var availability = new SeatAvailabilitySpy(string.Empty, availableSeats: 10);
        var repository = new RepositorySpy();
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability, repository, sender);

        // Act
        var exception = Assert.Throws<ArgumentException>(() => service.Reserve(Email, string.Empty, 2));

        // Assert
        Assert.Equal("eventCode", exception.ParamName);
        Assert.Empty(availability.RequestedEventCodes);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    // ---------------------------------------------------------------
    // Aufgabe 3: Spy von Hand (ConfirmationSenderSpy)
    // ---------------------------------------------------------------

    [Fact]
    public void Reserve_AfterSuccessfulSave_SendsExactlyOneConfirmation()
    {
        // Arrange
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(new SeatAvailabilityStub(EventCode, 3), new RepositorySpy(saveResult: true), sender);

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.True(result);
        var confirmation = Assert.Single(sender.SentReservations);
        Assert.Equal(Email, confirmation.CustomerEmail);
        Assert.Equal(EventCode, confirmation.EventCode);
        Assert.Equal(2, confirmation.SeatCount);
    }

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotSaveOrSend()
    {
        // Arrange
        var repository = new RepositorySpy();
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(new SeatAvailabilityStub(EventCode, 1), repository, sender);

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.False(result);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WhenSavingFails_DoesNotSendConfirmation()
    {
        // Arrange
        var repository = new RepositorySpy(saveResult: false);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(new SeatAvailabilityStub(EventCode, 3), repository, sender);

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.False(result);
        Assert.Single(repository.SavedReservations);
        Assert.Empty(sender.SentReservations);
    }

    // ---------------------------------------------------------------
    // Aufgabe 4: Moq für das Repository
    // ---------------------------------------------------------------

    [Fact]
    public void Reserve_WithEnoughSeats_SavesCorrectReservationOnce()
    {
        // Arrange
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySave(It.IsAny<Reservation>())).Returns(true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(new SeatAvailabilityStub(EventCode, 3), repository.Object, sender);

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.True(result);
        repository.Verify(r => r.TrySave(It.Is<Reservation>(res =>
            res.CustomerEmail == Email &&
            res.EventCode == EventCode &&
            res.SeatCount == 2)), Times.Once());
        Assert.Equal(new Reservation(Email, EventCode, 2), Assert.Single(sender.SentReservations));
    }

    [Fact]
    public void Reserve_WhenMockedSaveFails_ReturnsFalseAndSendsNothing()
    {
        // Arrange
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySave(It.IsAny<Reservation>())).Returns(false);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(new SeatAvailabilityStub(EventCode, 3), repository.Object, sender);

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.False(result);
        repository.Verify(r => r.TrySave(It.Is<Reservation>(res =>
            res.CustomerEmail == Email &&
            res.EventCode == EventCode &&
            res.SeatCount == 2)), Times.Once());
        Assert.Empty(sender.SentReservations);
    }

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotCallRepository()
    {
        // Arrange
        var repository = new Mock<IReservationRepository>();
        repository.Setup(r => r.TrySave(It.IsAny<Reservation>())).Returns(true);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(new SeatAvailabilityStub(EventCode, 1), repository.Object, sender);

        // Act
        var result = service.Reserve(Email, EventCode, 2);

        // Assert
        Assert.False(result);
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        Assert.Empty(sender.SentReservations);
    }

    // ---------------------------------------------------------------
    // Aufgabe 5: Handgeschrieben oder Moq? (Begründung in ANTWORTEN.md)
    // Verfügbarkeit + Repository: Moq (Strict, ohne Setup) – jeder Aufruf würde sofort scheitern.
    // Sender: eigener Spy – bereits vorhanden und gut lesbar.
    // ---------------------------------------------------------------

    [Fact]
    public void Reserve_WithWhitespaceEmail_ThrowsBeforeCallingDependencies()
    {
        // Arrange
        var availability = new Mock<ISeatAvailability>(MockBehavior.Strict);
        var repository = new Mock<IReservationRepository>(MockBehavior.Strict);
        var sender = new ConfirmationSenderSpy();
        var service = new ReservationService(availability.Object, repository.Object, sender);

        // Act
        var exception = Assert.Throws<ArgumentException>(() => service.Reserve("   ", EventCode, 2));

        // Assert
        Assert.Equal("customerEmail", exception.ParamName);
        availability.VerifyNoOtherCalls();
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never());
        repository.VerifyNoOtherCalls();
        Assert.Empty(sender.SentReservations);
    }
}
