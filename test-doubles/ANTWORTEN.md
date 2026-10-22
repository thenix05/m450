# M450 – Test Doubles: Antworten

Gewähltes Framework: **xUnit** (`tests/TestDoublesExercise.Tests`)

```bash
dotnet test tests/TestDoublesExercise.Tests/TestDoublesExercise.Tests.csproj
```

## Übersicht der Doubles (`tests/TestDoublesExercise.Tests/Doubles`)

| Double | Rolle | Verwendung |
|---|---|---|
| `SeatAvailabilityStub` | Stub | liefert pro Veranstaltungscode eine fixe Anzahl freier Plätze |
| `SeatAvailabilitySpy` | Spy (+ Stub) | wie der Stub, zeichnet zusätzlich jede Abfrage auf |
| `AlwaysSuccessfulRepository` | Stub | `TrySave`/`TrySaveBatch` liefern immer `true` |
| `RepositorySpy` | Spy (+ Stub) | festgelegtes Ergebnis, zeichnet Speicherversuche auf |
| `NoOpConfirmationSender` | Dummy | `Send` tut nichts |
| `ConfirmationSenderSpy` | Spy | zeichnet alle gesendeten Reservierungen in Reihenfolge auf |

Moq wird für `IReservationRepository` (Aufgabe 4, Batch) und in Aufgabe 5 eingesetzt.

## Aufgabe 1 – Nur die Schnittstelle ist bekannt

`ReservationService` hängt nur von der Schnittstelle `ISeatAvailability` ab, nicht von einer Implementierung. Deshalb können wir im Test einen eigenen Stub übergeben und den Service schon jetzt testen.

Die Schnittstelle sagt uns bereits: Es gibt eine Methode `GetAvailableSeats`, die einen Veranstaltungscode (`string`) entgegennimmt und eine Anzahl (`int`) zurückgibt. Sie sagt nichts darüber, woher die Zahl kommt, wie unbekannte Codes behandelt werden oder ob Fehler geworfen werden.

**Fachliche Annahme:** `GetAvailableSeats` liefert die Anzahl der aktuell noch freien Plätze (≥ 0) für genau diese Veranstaltung. Für einen unbekannten Veranstaltungscode nehmen wir 0 freie Plätze an. Der Stub berechnet nichts, sondern gibt nur den im Test festgelegten Wert zurück.

## Aufgabe 2 – Fehlende Aufrufe sichtbar machen

Ohne Produktionscode zu ändern, übergeben wir Spies (`SeatAvailabilitySpy`, `RepositorySpy`, `ConfirmationSenderSpy`), die jeden Aufruf aufzeichnen. Nach der Exception prüfen wir, dass alle aufgezeichneten Listen leer sind. Alternativ ginge ein Double, das bei jedem Aufruf selbst eine Exception wirft (z. B. Moq mit `MockBehavior.Strict`).

## Aufgabe 3 – Unterschied Stub / Spy

Unser Stub (`SeatAvailabilityStub`) liefert dem Service nur kontrollierte Eingabedaten (freie Plätze), während unser Spy (`ConfirmationSenderSpy`) aufzeichnet, wie der Service ihn aufruft, damit der Test diese Aufrufe danach prüfen kann.

## Aufgabe 5 – Handgeschrieben oder Moq?

Für `ISeatAvailability` und `IReservationRepository` verwenden wir Moq mit `MockBehavior.Strict` ohne Setup: Jeder unerwartete Aufruf würde sofort scheitern, und mit `Verify(..., Times.Never())` bzw. `VerifyNoOtherCalls()` ist das Ausbleiben der Aufrufe in einer Zeile prüfbar – ohne eine weitere Klasse zu schreiben. Für den Sender nehmen wir unseren bereits vorhandenen `ConfirmationSenderSpy`, weil `Assert.Empty(sender.SentReservations)` gut lesbar ist und wir den Spy ohnehin haben.

## Fortgeschrittene – Was passiert, wenn `Send` nach dem Speichern eine Exception wirft?

Die Reservierung(en) sind dann bereits gespeichert, aber die Exception geht an den Aufrufer weiter. Der Aufrufer meint, die Reservierung sei fehlgeschlagen, obwohl die Plätze belegt sind; der Kunde erhält keine (bzw. beim Batch nur einen Teil der) Bestätigungen. Versucht er es erneut, entstehen womöglich doppelte Reservierungen.

**Mögliche fachliche Lösung:** Speichern und Bestätigen trennen – eine erfolgreich gespeicherte Reservierung gilt als gültig, die Bestätigung wird als «ausstehend» markiert und später automatisch erneut versendet (z. B. über eine Warteschlange/Outbox). Der Fehler wird protokolliert, die Reservierung wird nicht rückgängig gemacht, und der Kunde kann die Bestätigung bei Bedarf erneut anfordern.
