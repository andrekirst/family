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
- CodeCoverage: **coverlet** für .NET Code Coverage mit automatischer PR-Integration
- Architektur: **Clean Architecture / Onion-Prinzip**
- Umsetzung von SOLID-Prinzipien
- Umsetzung von Clean-Code
- Umsetzung von CQRS
- Alle Aktionen sind Eventbasiert
- Event Sourcing dient als Datengrundlage

### Frontend

- **Angular** (für Web)
- **Flutter** (für Mobile Apps, Android & iOS)
- **Tests**: **Playwright** für End-to-End und Integration Tests (Angular)
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
- **Entity Framework Interceptors**: Verwende Interceptors für domänenübergreifende Concerns (Timestamps, Auditing, etc.) statt manueller SaveChanges-Überschreibung
- **Entity Configuration**: Verwende separate `IEntityTypeConfiguration<T>` Klassen für Entity-Konfiguration und `modelBuilder.ApplyConfigurationsFromAssembly()` statt direkter Konfiguration im DbContext
- **Factory Methods für Records**: Verwende statische Factory Methods für bessere Lesbarkeit statt unleserlicher `null, null, null` Konstruktor-Aufrufe
- **Keine Magic Numbers**: Ersetze Zahlen durch benannte Konstanten
- **MediaTypeNames verwenden**: Nutze MediaTypeNames.Application.Json statt hardcoded Strings
- **Docker Compose Updates**: Bei Infrastruktur-Änderungen (neue Services wie Redis, Kafka, etc.) ist IMMER die `docker-compose.yml` Datei anzupassen und die entsprechenden Umgebungsvariablen für die Services zu konfigurieren
- **Mehrsprachigkeits-Richtlinien**: 
  - Alle benutzerorientierten Texte (Validierungsmeldungen, Fehlermeldungen, UI-Labels) müssen mehrsprachig implementiert werden
  - Verwende das ASP.NET Core Localization System mit IStringLocalizer
  - Erstelle Resource-Dateien (.resx) für Deutsch (`*.de.resx`) und Englisch (`*.en.resx`)
  - Nutze Namespace-basierte Resource-Organisation (z.B. `Features.Users.Resources`)
  - Standard-Sprache ist Deutsch (`de`), Fallback ist Englisch (`en`)
  - Keine hardcodierten deutschen oder englischen Texte im Code - IMMER über Localization
- **Authorization Constants**: 
  - Verwende NIEMALS hardcodierte Policy- oder Role-Namen als Strings
  - Definiere alle Authorization-Konstanten in separaten Constants-Klassen im `Family.Api.Authorization` Namespace
  - Struktur: `Policies.cs` für Policy-Namen, `Roles.cs` für Role-Claim-Werte, `Claims.cs` für Claim-Type-Namen
  - Beispiel: `[Authorize(Policy = Policies.FamilyUser)]` statt `[Authorize(Policy = "FamilyUser")]`
  - Update IMMER alle vorhandenen hardcodierten Strings bei neuen Authorization-Features
- Passe CI/CD-Pipeline an, soweit nötig
- **Test-Richtlinien**:
  - **ZWINGEND**: Bei jeder Implementierung von neuem Quellcode müssen entsprechende Tests angelegt werden
  - **Ausnahme**: Nur bei reinen Bugfixes ohne neue Funktionalität können Tests optional sein
  - **Mindest-CodeCoverage**: 40% - Pipeline schlägt bei Unterschreitung fehl
  - **Test-Arten**: Unit Tests für Business Logic, Integration Tests für End-to-End-Flows, Validation Tests für Commands/Queries
  - **Test-Struktur**: Verwende AAA-Pattern (Arrange, Act, Assert), FluentAssertions für lesbare Assertions
  - **Test-Naming**: `{MethodName}_{Scenario}_{ExpectedResult}` (z.B. `CreateUser_WithValidData_ShouldReturnSuccess`)
  - **Test-Kategorien**: Erstelle Tests für DTOs, Commands/Queries, Validators, GraphQL Input/Payload Types, Authorization Constants
  - **Negative Tests**: Teste sowohl positive als auch negative Szenarien (z.B. Validierungsfehler, ungültige Eingaben)

### Pull Requests

- Jeder PR sollte:
  - Eine prägnante Beschreibung enthalten
  - Das referenzierte Issue in der PR-Beschreibung erwähnen
  - Gut strukturierte Commits enthalten
  - Mit passenden Tests kommen
  - per CI/CD durchlaufen. Das heißt, buildfähig und alle Tests grün
  - **CodeCoverage-Report als Kommentar anzeigen** mit aktueller Coverage und Trends
  - **Mindest-CodeCoverage von 40%** einhalten (Pipeline schlägt fehl bei Unterschreitung)
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
3. **Feature-Branch erstellen** - ZWINGEND für jede Issue-Implementierung:
   ```bash
   git checkout main
   git pull origin main
   git checkout -b feature/issue-XX-kurze-beschreibung
   git push -u origin feature/issue-XX-kurze-beschreibung
   ```
   - **Branch-Naming-Konvention**: `feature/issue-XX-kurze-beschreibung`, `bugfix/issue-XX-kurze-beschreibung`, `docs/issue-XX-kurze-beschreibung`
   - **WICHTIG**: Jede Issue-Implementierung erfolgt in einem separaten Feature-Branch
   - **NIEMALS** auf bestehenden Feature-Branches oder direkt auf `main` arbeiten
4. Setze das Label des GitHub-Issue auf `in progress`
5. Erstelle für die Implementierung eine strukturierte Todo-Liste mit allen erforderlichen Aufgaben
6. Implementiere die Lösung schrittweise anhand der Todo-Liste:
   6.1. **Nach jedem abgeschlossenen Todo-Punkt einen Commit durchführen**
   6.2. **Nach jedem Commit einen Push zum Remote-Branch durchführen**
   6.3. **Bei Abschluss einer Aufgabe diese im GitHub-Issue abhaken**
   6.4. Es sollen positive und negative Tests erstellt werden
   6.5. Die CodeCoverage sollte immer über 40% liegen und wird automatisch in PRs gemessen
   6.6. Verwende möglichst kleine und sinnvolle Commits mit conventional commit messages
7. Bevor der **Pull Request** erstellt wird, muss der gesamte Code buildfähig sein und lokal alle Tests erfolgreich sein
8. Erstelle einen **Pull Request**, der das Issue referenziert (`Fixes #123`)
9. **Pull Request Build-Validierung**:
   - **ZWINGEND**: Überprüfe nach PR-Erstellung den Build-Status der CI/CD-Pipeline
   - Verwende `gh pr view <PR-Nummer> --json statusCheckRollup` zur Status-Überprüfung
   - Bei fehlgeschlagenen Builds: Analysiere Fehler mit `gh run view <run-id>` und behebe diese umgehend
   - **Geskippte Checks ignorieren**: Falls ein Check als "SKIPPED" markiert ist, ignoriere diesen
   - **Alle FAILURE-Status müssen behoben werden** bevor der PR für Review bereit ist
   - Committe und pushe Build-Fixes sofort nach der Fehlerbehebung
10. Warte auf menschliches Review – merge **nicht selbst**

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

## Cloud Design Patterns

Die Family-Plattform nutzt bewährte Cloud Design Patterns für eine skalierbare, resiliente und wartbare Architektur. Diese Patterns basieren auf dem [Microsoft Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/) und unterstützen die Säulen: Reliability, Security, Cost Optimization, Operational Excellence und Performance Efficiency.

### Datenmanagement Patterns

#### Cache-Aside
**Zweck**: Load data on demand into a cache from a data store  
**Anwendung**: Redis für häufig abgefragte Daten (Benutzerprofile, Konfigurationen)  
**Implementierung**:
- C#: `IMemoryCache` und Redis mit `StackExchange.Redis`
- Automatisches Cache-Invalidation bei Datenänderungen
- TTL-basierte Expiration für verschiedene Datentypen

#### CQRS (Command Query Responsibility Segregation)
**Zweck**: Separate read and write operations for better performance and scalability  
**Anwendung**: GraphQL Mutations (Commands) und Queries getrennt behandeln  
**Implementierung**:
- Command-Handler für Schreiboperationen mit Event-Erzeugung
- Query-Handler für Leseoperationen mit optimierten Read-Models
- MediatR für Command/Query-Dispatching

#### Event Sourcing
**Zweck**: Append-only store for complete event history  
**Anwendung**: Familienereignisse, Terminänderungen, Medikamentenverlauf  
**Implementierung**:
- Event Store mit PostgreSQL und Kafka
- Domain Events als unveränderliche Records
- Event-Replay für Zustandsrekonstruktion und Debugging

#### Sharding
**Zweck**: Partition data across multiple databases for scalability  
**Anwendung**: Aufteilung von Familiendaten nach Familie-ID  
**Implementierung**:
- PostgreSQL-Partitionierung nach Family-Tenant
- Entity Framework mit Custom DbContext pro Shard
- Routing-Layer für Shard-Selection basierend auf User-Context

### Messaging Patterns

#### Asynchronous Request-Reply
**Zweck**: Decouple backend processing from frontend hosts  
**Anwendung**: Lange laufende Operationen (Datenimport, Berichte)  
**Implementierung**:
- Kafka für asynchrone Nachrichten
- Correlation-IDs für Request-Reply-Matching
- SignalR für Real-time Updates an Frontend

#### Choreography
**Zweck**: Coordinate service interactions through events  
**Anwendung**: Familienworkflows (Terminbuchung → Erinnerung → Benachrichtigung)  
**Implementierung**:
- Domain Events über Kafka Topics
- Event-Handler in verschiedenen Bounded Contexts
- Saga-Pattern für komplexe Geschäftsprozesse

#### Claim Check
**Zweck**: Split large messages into claim and payload for efficiency  
**Anwendung**: Große Dateiuploads (Bilder, Dokumente)  
**Implementierung**:
- Metadata in Kafka-Message, Datei in Blob Storage
- Azure Blob Storage oder lokale File Storage
- Referenz-URLs mit begrenzter Gültigkeit

#### Competing Consumers
**Zweck**: Multiple concurrent consumers process messages from queue  
**Anwendung**: Email-Versand, Push-Notifications, Datenverarbeitung  
**Implementierung**:
- Kafka Consumer Groups für parallele Verarbeitung
- Idempotente Message-Handler
- Dead Letter Queue für fehlgeschlagene Nachrichten

#### Publisher/Subscriber
**Zweck**: Decouple applications that require asynchronous communication  
**Anwendung**: Domain Events zwischen Bounded Contexts  
**Implementierung**:
- Kafka Topics für Event-Publishing
- Multiple Subscriber pro Event-Type
- Event Schema Registry für Versionierung

#### Queue-Based Load Leveling
**Zweck**: Use queue to smooth intermittent heavy loads  
**Anwendung**: Batch-Import von Kalenderdaten, Massive Benachrichtigungen  
**Implementierung**:
- Kafka für Message-Buffering
- Rate-Limited Consumers
- Auto-Scaling basierend auf Queue-Depth

### Design & Implementation Patterns

#### Ambassador
**Zweck**: Helper services that send network requests on behalf of consumers  
**Anwendung**: External API-Integration (Schul-APIs, Kalender-Services)  
**Implementierung**:
- HttpClient mit Polly für Retry/Circuit-Breaker
- API-Gateway als Ambassador für externe Services
- Request/Response-Transformation und -Validierung

#### Anti-Corruption Layer
**Zweck**: Façade between modern applications and legacy systems  
**Anwendung**: Integration mit bestehenden Schulsystemen oder Kalendern  
**Implementierung**:
- Adapter-Pattern für externe API-Integration
- DTOs für Datenkonvertierung
- Separate Bounded Context für Legacy-Integration

#### External Configuration Store
**Zweck**: Centralize configuration management  
**Anwendung**: Feature Flags, API-Endpoints, Umgebungskonfiguration  
**Implementierung**:
- Azure App Configuration oder PostgreSQL
- IConfiguration mit Hot-Reload
- Feature Flag Management für A/B-Testing

#### Gateway Offloading
**Zweck**: Offload shared functionality to a gateway proxy  
**Anwendung**: Authentication, Rate Limiting, Request Logging  
**Implementierung**:
- Ocelot oder YARP als API Gateway
- JWT-Validierung im Gateway
- Cross-cutting Concerns zentral behandelt

### Resilienz Patterns

#### Circuit Breaker
**Zweck**: Handle faults when connecting to remote services/resources  
**Anwendung**: Externe API-Calls, Datenbankverbindungen  
**Implementierung**:
- Polly Circuit Breaker für HttpClient
- Fallback-Mechanismen für kritische Services
- Health Checks für Circuit Breaker State

#### Health Endpoint Monitoring
**Zweck**: Implement functional checks for applications and services  
**Anwendung**: Kubernetes Liveness/Readiness Probes, Load Balancer Health Checks  
**Implementierung**:
- ASP.NET Core Health Checks
- Custom Health Checks für Datenbank, Redis, Kafka
- Health Dashboard für Monitoring

#### Quarantine
**Zweck**: Isolate instances that are failing or behaving abnormally  
**Anwendung**: Fehlerhafte Nachrichten, problematische User-Requests  
**Implementierung**:
- Dead Letter Queue für fehlgeschlagene Messages
- Request-Blacklisting für anomales Verhalten
- Automatic Retry mit Exponential Backoff

#### Retry
**Zweck**: Enable applications to handle transient failures transparently  
**Anwendung**: Netzwerk-Timeouts, temporäre Service-Ausfälle  
**Implementierung**:
- Polly Retry Policies mit Exponential Backoff
- Jitter für Thundering Herd Prevention
- Max Retry Count und Circuit Breaker Integration

#### Saga
**Zweck**: Manage data consistency across microservices in distributed transactions  
**Anwendung**: Familienregistrierung, Komplexe Buchungsprozesse  
**Implementierung**:
- Choreography-based Sagas mit Domain Events
- Compensation Actions für Rollback
- Saga State Management mit Event Sourcing

### Sicherheits Patterns

#### Federated Identity
**Zweck**: Delegate authentication to external identity provider  
**Anwendung**: Keycloak für zentrale Authentifizierung  
**Implementierung**:
- OpenID Connect/OAuth2 mit Keycloak
- JWT Token-Validierung in ASP.NET Core
- Multi-Tenant Support über Keycloak Realms

#### Gatekeeper
**Zweck**: Protect applications using dedicated host instance as broker  
**Anwendung**: API Gateway als Security-Proxy  
**Implementierung**:
- YARP Gateway mit Authentication-Middleware
- Request-Validation und Authorization
- Rate Limiting und DDoS-Protection

#### Rate Limiting
**Zweck**: Control consumption of resources by applications, tenants, or services  
**Anwendung**: API-Call Limits pro Familie/Benutzer  
**Implementierung**:
- Redis-basierte Rate Limiting
- Different Limits pro API-Endpoint
- Graceful Degradation bei Limit-Überschreitung

### Performance & Skalierungs Patterns

#### Deployment Stamps
**Zweck**: Deploy multiple independent copies of application components  
**Anwendung**: Multi-Tenant Deployment pro Familie oder Region  
**Implementierung**:
- Kubernetes Namespaces pro Tenant
- Separate Database-Instances für große Familien
- Load Balancer für Stamp-Selection

#### Geode
**Zweck**: Deploy backend services in geographical regions close to frontend users  
**Anwendung**: CDN für statische Assets, regionale API-Deployments  
**Implementierung**:
- Azure CDN oder CloudFlare für Assets
- Geo-DNS für regionale API-Endpoints
- Data Replication zwischen Regionen

### Pattern-Integration in Family-Architektur

#### Technologie-Zuordnung
- **Cache-Aside**: Redis mit ASP.NET Core
- **CQRS/Event Sourcing**: MediatR + Kafka + PostgreSQL
- **Circuit Breaker/Retry**: Polly
- **Pub/Sub**: Kafka
- **Health Monitoring**: ASP.NET Core Health Checks
- **Rate Limiting**: Redis + Custom Middleware
- **Configuration**: ASP.NET Core IConfiguration

#### Testing-Strategien
- **Unit Tests**: Pattern-spezifische Handler und Services
- **Integration Tests**: End-to-End Pattern-Flows mit Testcontainers
- **Performance Tests**: Load Testing für Resilienz-Patterns
- **Chaos Engineering**: Fault Injection für Resilienz-Validierung

#### Monitoring & Observability
- **Metrics**: Pattern-spezifische Metriken (Circuit Breaker State, Cache Hit Rate)
- **Distributed Tracing**: Request-Flow über Pattern-Grenzen hinweg
- **Structured Logging**: Pattern-Context in Log-Events
- **Alerting**: Pattern-basierte Alerts (Circuit Breaker Open, High Error Rate)

### Referenzen
- [Microsoft Cloud Design Patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)
- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/)
- [.NET Application Architecture Guides](https://learn.microsoft.com/en-us/dotnet/architecture/)

---

**Letzte Aktualisierung:** 2025-07-03
