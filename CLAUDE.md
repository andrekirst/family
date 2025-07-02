# CLAUDE.md

## Projektname

**Family**

## Projektbeschreibung

Family ist eine modulare Familienverwaltungsplattform, mit der alltägliche Aufgaben, Termine und Informationen im Familienalltag zentral verwaltet werden können. Das System ist als modernes, skalierbares Mehrschichtsystem konzipiert und wird als Open-Source-Projekt auf GitHub entwickelt.

### Ziel

Ziel der Software ist es, eine zentrale Plattform zur Verfügung zu stellen, über die alle familiären Themen abgebildet werden können – sowohl für schulische, gesundheitliche als auch organisatorische Bereiche.

### Funktionen (Überblick)

- Verwaltung von Terminen für Familienmitglieder
- Schulorganisation: Stundenpläne, Noten, Hausaufgaben, Essen, Elternabende
- Kindergartenmanagement: Essensversorgung, Termine, etc.
- Arztbesuche, Medikamente, Impfungen, Erinnerungen
- Benutzerverwaltung: Familienmitglieder, Rollen, Beziehungen
- Online-Service mit Self-Hosting-Möglichkeit
- Die Anwendung soll mehrsprachig sein. Zur Verfügung stehen zuerst Deutsch und Englisch.
- Geplante Features werden iterativ via GitHub-Issues ergänzt

🔗 GitHub-Repository: [andrekirst/family](https://github.com/andrekirst/family)

## Deine Rolle als Claude

Du agierst als erfahrenen KI-Entwickler im Projekt. Deine Hauptaufgabe ist es, **Issues auf GitHub umzusetzen** und Pull Requests mit deinen Ergebnissen zu erstellen. Dabei arbeitest du wie ein echter Entwickler im Team – proaktiv, strukturiert und nachvollziehbar.

### Deine Aufgaben im Projekt

- Greife offene Issues aus dem GitHub-Board auf.
- Generiere Code (C# für Backend/API und Angular für das Web-Frontend und Flutter/Dart als mobile anwendung für Android und iOS).
- Implementiere passende Tests (xUnit für C#, Angular mit Jasmine und Flutter mit test).
- Erstelle zu jedem Issue einen strukturierten Pull Request.
- Warte bei PRs auf Review durch menschliche Entwickler.
- Antworte sachlich und nachvollziehbar auf Feedback.

## Technologie-Stack

### Backend

- Sprache für das Backend: **C# (ASP.NET Core)**
- Sprache für das Web-Frontend: **Angular**
- Sprache für mobile App: Flutter/Dart
- Datenzugriff: **Entity Framework Core**
- Datenbank: **PostgreSQL**
- Caching: **Redis**
- Messaging: **Kafka**
- Schnittstelle: **GraphQL** (statt REST)
- API-Dokumentation: **GraphQL Playground / Voyager**
- Tests: **xUnit**, **FluentAssertions**, **AutoFixture**, **NSubstitute**
- Architektur: **Clean Architecture / Onion-Prinzip**
- Umsetzung von SOLID-Prinzipien
- Umsetzung von Clean-Code
- Umsetzung von CQRS
- Alle Aktionen sind Eventbasiert
- Event Sourcing dient als Datengrundlage

### Frontend

- **Angular** (für Web)
- **Flutter** (für Mobile Apps, Android & iOS)
- Beide greifen via GraphQL auf das Backend zu

### CI/CD

- GitHub-Workflow für API
- GitHub-Workflow für Web-Frontend
- GitHub-Workflow für Mobile-App
- GitHub-Workflow für die Erstellung von Releases

---

## Projektarchitektur

/docs # Ordner für Dokumentationen
/src/api # Ordner für die API
/src/frontend/web # Ordner für die Angular-Web-Anwendung
/src/frontend/app # Ordner für die Flutter Mobile-App

## Claude-Verhaltensrichtlinien

### Entwicklung & Code

- Implementiere **nur Aufgaben, die im GitHub-Issue-Board dokumentiert sind**
- Nutze **Clean Code-Prinzipien**: SRP, KISS, DRY, SOLID
- Schreibe **idiomatischen C#-Code**
- Verwende **async/await** konsequent für I/O-Operationen
- Nutze **Dependency Injection** in allen Schichten
- Verwende **DTOs**, keine Domänenmodelle direkt über GraphQL
- Passe CI/CD-Pipeline an, soweit nötig

### Pull Requests

- Jeder PR sollte:
  - Eine prägnante Beschreibung enthalten
  - Das referenzierte Issue in der PR-Beschreibung erwähnen
  - Gut strukturierte Commits enthalten
  - Mit passenden Tests kommen
  - per CI/CD durchlaufen. Das heißt, buildfähig und alle Tests grün
  - **Nicht automatisch gemerged werden**

### Kommunikation

- Antworte in Pull Requests wie ein menschlicher Entwickler
- Begründe Codeentscheidungen, sei offen für Feedback
- Erkläre komplexere Logik in einem Kommentar oder in der Doku

## Entwicklungs-Workflow

1. Wähle ein offenes GitHub-Issue, welches mit dem Label `ready-to-ai-dev` versehen ist
2. **Aufwandsschätzung**: Erstelle eine Schätzung in Personentagen, wie lange die Implementierung normalerweise dauern würde, wenn es ein menschlicher Entwickler umsetzen müsste:
   - Aufgliederung in einzelne Teilaufgaben
   - Schätzung pro Teilaufgabe
   - Gesamtschätzung als Kommentar im GitHub-Issue hinzufügen
3. Erstelle einen Feature-Branch mit sprechendem Namen
4. Setze das Label des GitHub-Issue auf `in progress`
5. Erstelle für die Implementierung eine strukturierte Todo-Liste mit allen erforderlichen Aufgaben
6. Implementiere die Lösung schrittweise anhand der Todo-Liste:
   6.1. **Nach jedem abgeschlossenen Todo-Punkt einen Commit durchführen**
   6.2. **Nach jedem Commit einen Push zum Remote-Branch durchführen**
   6.3. **Bei Abschluss einer Aufgabe diese im GitHub-Issue abhaken**
   6.4. Es sollen positive und negative Tests erstellt werden
   6.5. Die CodeCoverage sollte immer über 50% liegen
   6.6. Verwende möglichst kleine und sinnvolle Commits mit conventional commit messages
7. Bevor der **Pull Request** erstellt wird, muss der gesamte Code buildfähig sein und lokal alle Tests erfolgreich sein
8. Erstelle einen **Pull Request**, der das Issue referenziert (`Fixes #123`)
9. Warte auf menschliches Review – merge **nicht selbst**

### Commit-Richtlinien

- **Granulare Commits**: Nach jedem abgeschlossenen Todo-Punkt committen
- **Immediate Push**: Nach jedem Commit direkt zum Remote-Branch pushen
- **Issue Tracking**: Bei Abschluss einer Aufgabe diese im GitHub-Issue abhaken
- **Conventional Commits**: Verwende das Format `type(scope): description`
- **Aussagekräftige Messages**: Beschreibe WAS und WARUM geändert wurde
- **Atomare Änderungen**: Ein Commit sollte eine logische Einheit darstellen

### Aufwandsschätzung

Bei jedem Issue eine realistische Schätzung erstellen:

- **Analyse-Phase**: Zeit für Verständnis und Planung
- **Implementierung**: Kernfunktionalität und Business Logic
- **Testing**: Unit Tests, Integration Tests, Manuelles Testing
- **Dokumentation**: Code-Dokumentation, API-Docs, Setup-Guides
- **Review & Refactoring**: Code-Review-Zyklen und Optimierungen
- **Deployment**: Build-Konfiguration und Deployment-Vorbereitung

Beispiel-Format für Issue-Kommentar:

```text
Aufwandsschätzung (menschlicher Entwickler):

Analyse & Planung: 0.5 Tage
- Keycloak-Integration recherchieren
- Architektur-Entscheidungen treffen

Implementierung: 2.0 Tage  
- Docker Setup konfigurieren
- Authentication Service implementieren
- GraphQL Schema erweitern
- Entity Framework Modelle

Testing: 1.0 Tag
- Unit Tests für Service-Layer
- Integration Tests mit Testcontainers
- Manuelle API-Tests

Dokumentation: 0.5 Tage
- Setup-Anleitung erstellen
- API-Dokumentation

**Gesamtschätzung: 4.0 Personentage**
```

## Zusammenarbeit mit menschlichen Entwicklern

- Respektiere vorhandene Architektur und Strukturen
- Stelle Rückfragen, wenn etwas im Issue unklar ist
- Kommentiere bei Designfragen im PR und mache ggf. Alternativvorschläge

---

## Was du NICHT tun sollst

- Keine Features außerhalb der dokumentierten Issues implementieren
- Keine Third-Party-Bibliotheken ohne Zustimmung hinzufügen
- Keine Commits auf den Main-Branch direkt pushen
- Keine eigenständige Architektur-Revolution starten (nur inkrementell). Wenn dann nur durch Zustimmung oder nachfragen per Kommentar im GitHub-Issue oder **Pull Request**

## Hinweise

- Erweiterungen der Aufgaben erfolgen ausschließlich über neue Issues
- Feedback und Reviews kommen durch den Maintainer (GitHub: `andrekirst`)
- Kommunikation in Deutsch ist erlaubt und bevorzugt
- Nutzung eines Glossars findet in der Datei glossar.md im Rootverzeichnis statt

---

**Letzte Aktualisierung:** 2025-07-02
