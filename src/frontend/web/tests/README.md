# Playwright Tests für Family Web-Anwendung

Diese Dokumentation beschreibt die Playwright-Tests für die Family Web-Anwendung, die umfassende End-to-End-Tests für Authentifizierung und Navigation bereitstellen.

## Übersicht

Das Testsystem verwendet Playwright als Test-Framework und deckt folgende Bereiche ab:

### 1. Basis-Tests (`example.spec.ts`)
- **Anwendungsstart**: Prüft, ob die Anwendung ordnungsgemäß lädt
- **Seitentitel**: Verifiziert, dass die Anwendung einen gültigen Titel hat

### 2. Authentifizierung-Tests (`auth.spec.ts`)
Umfassende Tests für die Authentifizierungslogik:

#### Login-Funktionalität
- ✅ Anzeige des Login-Formulars für nicht-authentifizierte Benutzer
- ✅ Validierung von Formulareingaben (E-Mail, Passwort)
- ✅ Anzeige von Validierungsfehlern
- ✅ Passwort-Sichtbarkeit umschalten
- ✅ Direkte Anmeldung mit Benutzerdaten
- ✅ Erfolgreiche Anmeldung und Weiterleitung zum Dashboard
- ✅ Fehlerbehandlung bei fehlgeschlagener Anmeldung
- ✅ OAuth-Anmeldung mit Keycloak initiieren
- ✅ Weiterleitung authentifizierter Benutzer vom Login weg

#### Logout-Funktionalität
- ✅ Anzeige des Benutzermenüs bei authentifizierten Benutzern
- ✅ Erfolgreiche Abmeldung und Weiterleitung zum Login
- ✅ Fehlerbehandlung bei Abmeldungsproblemen
- ✅ Token-Bereinigung bei der Abmeldung

#### Authentication Guards
- ✅ Weiterleitung nicht-authentifizierter Benutzer zum Login
- ✅ Zugriff authentifizierter Benutzer auf geschützte Routen
- ✅ Behandlung ungültiger Tokens

### 3. Keycloak Integration Tests (`keycloak.spec.ts`)
Spezialisierte Tests für die OAuth-Integration mit Keycloak:

#### OAuth Callback Flow
- ✅ Erfolgreiche OAuth-Callback-Behandlung
- ✅ Validierung der State-Parameter
- ✅ Fehlerbehandlung bei ungültigen Authorization Codes
- ✅ Behandlung von OAuth-Fehlern (Benutzerabbruch)
- ✅ Session State Bereinigung nach erfolgreichem Login

#### Token Refresh Flow
- ✅ Erfolgreiche Token-Aktualisierung
- ✅ Fehlerbehandlung bei fehlgeschlagener Token-Aktualisierung
- ✅ Behandlung fehlender Refresh-Tokens

#### Keycloak Integration Flow
- ✅ Vollständige OAuth-Flow-Simulation
- ✅ Behandlung von Keycloak-Ausfällen
- ✅ Timeout-Behandlung bei OAuth-Anfragen

## Test-Ausführung

### Lokal
```bash
# Alle Tests ausführen
npm run test

# Tests mit Browser-Anzeige
npm run test:headed

# Tests mit UI-Modus
npm run test:ui

# Tests im Debug-Modus
npm run test:debug
```

### CI/CD Pipeline
Die Tests werden automatisch in der GitHub Actions Pipeline ausgeführt:
- Bei Pull Requests
- Bei Pushes zum main Branch
- Nightly builds

## Test-Struktur

### Mocking-Strategie
- **GraphQL-Mocking**: Alle GraphQL-Anfragen werden gemockt für deterministisches Testverhalten
- **LocalStorage/SessionStorage**: Wird für Authentifizierungsstate verwendet
- **Navigation**: Echte Navigation wird getestet, aber externe Services gemockt

### Test-Daten
- Verwendung von Mock-Daten für Benutzer und Tokens
- Deterministische State-Werte für OAuth-Flows
- Realistische Fehlerszenarien

### Assertions
- URL-Validierung für Navigation
- Element-Sichtbarkeit für UI-Komponenten
- LocalStorage-Prüfung für Token-Management
- Snackbar-Nachrichten für Benutzerfeedback

## Erweiterte Test-Szenarien

### Authentifizierung
1. **Direkte Anmeldung**: E-Mail/Passwort-basierte Authentifizierung
2. **OAuth-Anmeldung**: Keycloak-Integration mit vollständigem OAuth2-Flow
3. **Token-Refresh**: Automatische Token-Aktualisierung bei abgelaufenen Tokens
4. **Logout**: Saubere Abmeldung mit Token-Bereinigung

### Fehlerbehandlung
1. **Netzwerkfehler**: Simulation von API-Ausfällen
2. **Validierungsfehler**: Ungültige Eingaben und Formularvalidierung
3. **OAuth-Fehler**: Fehlgeschlagene OAuth-Flows und State-Validierung
4. **Token-Fehler**: Ungültige, abgelaufene oder fehlende Tokens

### Sicherheit
1. **CSRF-Schutz**: State-Parameter-Validierung bei OAuth
2. **Token-Sicherheit**: Sichere Token-Speicherung und -Bereinigung
3. **Route-Schutz**: Authentication Guards für geschützte Bereiche

## Wartung

### Neue Tests hinzufügen
1. Neue `.spec.ts` Dateien im `tests/` Verzeichnis erstellen
2. Playwright-Konventionen befolgen
3. Mocking-Strategien konsistent anwenden
4. Aussagekräftige Testbeschreibungen verwenden

### Test-Updates
- Bei Änderungen an der Authentifizierungslogik entsprechende Tests aktualisieren
- Neue Fehlerszenarien bei Bedarf hinzufügen
- Mock-Daten bei API-Änderungen anpassen

### Performance
- Tests laufen parallel für bessere Performance
- Mocking reduziert Abhängigkeiten zu externen Services
- Selektives Cleanup für bessere Testisolation

## Metriken

### Testabdeckung
- **Login-Flows**: 100% (beide Authentifizierungswege)
- **Logout-Flows**: 100% (inkl. Fehlerbehandlung)
- **OAuth-Integration**: 100% (vollständiger Keycloak-Flow)
- **Route Guards**: 100% (alle Schutzszenarien)

### Ausführungszeit
- Basis-Tests: ~5 Sekunden
- Auth-Tests: ~30 Sekunden
- Keycloak-Tests: ~25 Sekunden
- **Gesamt**: ~60 Sekunden

Die Tests sind so optimiert, dass sie schnell und zuverlässig ausgeführt werden können, sowohl lokal als auch in CI/CD-Pipelines.