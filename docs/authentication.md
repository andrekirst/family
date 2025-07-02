# Authentication mit Keycloak

Diese Dokumentation beschreibt die Implementierung der E-Mail-basierten Authentifizierung in der Family-Anwendung mit Keycloak als Identity Provider.

## Überblick

Die Family-Anwendung nutzt Keycloak für die Benutzerauthentifizierung und bietet folgende Funktionen:

- **E-Mail/Passwort Login** über Keycloak
- **OAuth2/OpenID Connect** Flow für Web-Anwendungen
- **JWT Token** Authentifizierung für API-Zugriff
- **Automatische Benutzersynchronisation** von Keycloak
- **Rollenverwaltung** mit family-user und family-admin Rollen

## Schnellstart

### 1. Entwicklungsumgebung starten

```bash
# Keycloak und PostgreSQL starten
docker-compose up -d postgres-keycloak postgres-app keycloak

# Warten bis Keycloak bereit ist (ca. 30-60 Sekunden)
docker-compose logs -f keycloak

# Datenbank migrieren
cd src/api/Family.Api
dotnet ef database update

# API starten
dotnet run
```

### 2. Keycloak Admin Console

- **URL**: http://localhost:8080
- **Admin Benutzer**: admin / admin
- **Realm**: family

### 3. Test-Benutzer

Die Realm-Konfiguration enthält bereits Test-Benutzer:

| Email | Passwort | Rollen |
|-------|----------|--------|
| test@family.local | Test123! | family-user |
| admin@family.local | Admin123! | family-user, family-admin |

## API Endpoints

### GraphQL Playground

- **URL**: http://localhost:8081/graphql
- **Entwicklung**: Interaktive GraphQL IDE verfügbar

### Swagger Documentation

- **URL**: http://localhost:8081/swagger

## Authentifizierung Flows

### 1. Direct Login (E-Mail/Passwort)

```graphql
mutation DirectLogin {
  directLogin(input: {
    email: "test@family.local"
    password: "Test123!"
  }) {
    accessToken
    refreshToken
    user {
      id
      email
      firstName
      lastName
      fullName
    }
    errors
  }
}
```

### 2. OAuth2 Authorization Code Flow

```graphql
# 1. Login initiieren
mutation InitiateLogin {
  initiateLogin {
    loginUrl
    state
  }
}

# 2. Nach Redirect von Keycloak
mutation CompleteLogin {
  completeLogin(input: {
    authorizationCode: "received-auth-code"
    state: "received-state"
  }) {
    accessToken
    refreshToken
    user {
      email
      firstName
      lastName
    }
    errors
  }
}
```

### 3. Token Refresh

```graphql
mutation RefreshToken {
  refreshToken(input: {
    refreshToken: "your-refresh-token"
  }) {
    accessToken
    refreshToken
    errors
  }
}
```

### 4. Logout

```graphql
mutation Logout {
  logout(accessToken: "your-access-token") {
    success
    errors
  }
}
```

## Benutzerabfragen

### Aktueller Benutzer

```graphql
query CurrentUser {
  currentUser {
    id
    email
    firstName
    lastName
    fullName
    preferredLanguage
    isActive
    createdAt
    lastLoginAt
  }
}
```

### Benutzer nach ID (Admin oder eigenes Profil)

```graphql
query UserById {
  userById(id: "user-guid") {
    id
    email
    firstName
    lastName
  }
}
```

### Alle Benutzer (nur Admins)

```graphql
query AllUsers {
  users {
    id
    email
    firstName
    lastName
    isActive
  }
}
```

## Authorization

### JWT Token verwenden

Fügen Sie den Authorization Header zu Ihren API-Anfragen hinzu:

```
Authorization: Bearer <your-jwt-token>
```

### Rollen und Policies

- **FamilyUser**: Standardrolle für alle Familienmitglieder
- **FamilyAdmin**: Erweiterte Berechtigungen für Administratoren

```graphql
# Beispiel: Geschützte Query (benötigt Authentication)
query CurrentUser {
  currentUser {
    email
  }
}

# Beispiel: Admin-Query (benötigt FamilyAdmin Rolle)
query AllUsers {
  users {
    email
  }
}
```

## Konfiguration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=family;Username=family;Password=family"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/family",
    "Audience": "family-api",
    "ClientId": "family-api",
    "ClientSecret": "family-api-secret-change-in-production",
    "TokenEndpoint": "http://localhost:8080/realms/family/protocol/openid-connect/token",
    "UserInfoEndpoint": "http://localhost:8080/realms/family/protocol/openid-connect/userinfo",
    "RequireHttpsMetadata": false,
    "SaveTokens": true
  }
}
```

### Umgebungsvariablen

Für Production sollten sensible Daten als Umgebungsvariablen konfiguriert werden:

```bash
export Keycloak__ClientSecret="production-secret"
export ConnectionStrings__DefaultConnection="production-connection-string"
```

## Sicherheit

### Entwicklung vs. Production

**Entwicklung:**
- `RequireHttpsMetadata: false` - HTTP erlaubt
- Test-Credentials in Realm-Export
- Localhost URLs

**Production:**
- `RequireHttpsMetadata: true` - Nur HTTPS
- Sichere Client Secrets
- Produktions-URLs und Domains

### Passwort Policy

Die Keycloak-Konfiguration enforces folgende Passwort-Anforderungen:
- Mindestens 8 Zeichen
- Mindestens 1 Großbuchstabe
- Mindestens 1 Kleinbuchstabe
- Mindestens 1 Zahl
- Mindestens 1 Sonderzeichen

### Brute Force Protection

Keycloak ist konfiguriert mit:
- Max. 30 Fehlversuche
- 15-minütige Sperrung nach Überschreitung
- Progressive Wartezeiten

## Troubleshooting

### Häufige Probleme

**Keycloak nicht erreichbar:**
```bash
# Prüfen ob Container läuft
docker-compose ps

# Logs prüfen
docker-compose logs keycloak
```

**Token Validation Fehler:**
- Prüfen Sie die Authority URL in appsettings.json
- Stellen Sie sicher, dass Keycloak läuft
- Prüfen Sie die Client-Konfiguration

**Database Connection Fehler:**
```bash
# Migration ausführen
dotnet ef database update

# Database Container prüfen
docker-compose logs postgres-app
```

### Logs und Debugging

```bash
# API Logs
dotnet run --environment Development

# Detaillierte Entity Framework Logs
# Siehe appsettings.Development.json
```

## Entwicklung

### Neue Benutzer erstellen

1. Über Keycloak Admin Console
2. Automatische Synchronisation beim ersten Login
3. Rollen werden aus JWT Claims übernommen

### Tests ausführen

```bash
# Alle Tests
dotnet test

# Nur Integration Tests
dotnet test --filter "Category=Integration"

# Nur Unit Tests  
dotnet test --filter "Category=Unit"
```

### GraphQL Schema aktualisieren

Nach Änderungen an GraphQL Types:

```bash
# Schema regenerieren
dotnet run
# GraphQL Schema ist unter /graphql verfügbar
```

## Migration von bestehenden Systemen

Für die Migration von bestehenden Authentifizierungssystemen:

1. Benutzer in Keycloak importieren
2. Passwort-Reset für alle Benutzer
3. Claims und Rollen mapping anpassen
4. Schrittweise Migration der Client-Anwendungen

## Weiterführende Links

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [OpenID Connect Specification](https://openid.net/connect/)
- [HotChocolate GraphQL](https://chillicream.com/docs/hotchocolate/)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)