# Family Application

Family ist eine modulare Familienverwaltungsplattform f�r die zentrale Verwaltung allt�glicher Aufgaben, Termine und Informationen im Familienalltag.

## Features

-  **E-Mail Authentifizierung** mit Keycloak Identity Provider
-  **GraphQL API** f�r moderne Client-Anwendungen  
-  **Rollenverwaltung** (Family User, Family Admin)
-  **PostgreSQL** Database mit Entity Framework
-  **Docker Compose** f�r einfache lokale Entwicklung
- =� **Angular Web Frontend** (geplant)
- =� **Flutter Mobile App** (geplant)
- =� **Schulorganisation** (geplant)
- =� **Gesundheitsmanagement** (geplant)

## 🚀 Quick Start

### Verschiedene Deployment-Modi

```bash
# Repository klonen
git clone https://github.com/andrekirst/family.git
cd family
```

**Option 1: Nur Infrastructure (PostgreSQL + Keycloak)**
```bash
docker-compose up -d
# Zugriff: Keycloak http://localhost:8080 (admin/admin)
```

**Option 2: Infrastructure + API**
```bash
docker-compose --profile api up -d
# Zugriff: API http://localhost:8081/graphql
```

**Option 3: Komplette Anwendung**
```bash
docker-compose --profile full-stack up -d
# Zugriff: Web-App http://localhost:4200
```

**Zugriff auf alle Services:**
- 🌐 **Web-App**: http://localhost:4200
- 🔧 **API GraphQL**: http://localhost:8081/graphql  
- 🔐 **Keycloak Admin**: http://localhost:8080 (admin/admin)
- 🗄️ **PostgreSQL**: localhost:5432 (family/family)

### Entwicklungsmodus (lokal)

```bash
# 1. Nur Infrastructure starten
docker-compose up -d

# 2. API starten
cd src/api/Family.Api
dotnet restore
dotnet ef database update
dotnet run

# 3. Frontend starten (separates Terminal)
cd src/frontend/web
npm install
npm start
```

### Voraussetzungen (nur für lokale Entwicklung)

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker & Docker Compose](https://docs.docker.com/compose/install/)

### Zugriff

- **API GraphQL Playground**: http://localhost:8081/graphql
- **API Swagger**: http://localhost:8081/swagger  
- **Keycloak Admin**: http://localhost:8080 (admin/admin)

### Test Login

```graphql
mutation {
  directLogin(input: {
    email: "test@family.local"
    password: "Test123!"
  }) {
    accessToken
    user {
      email
      fullName
    }
  }
}
```

## Dokumentation

- [Authentication Setup](docs/authentication.md) - Keycloak Integration
- [API Documentation](src/api/README.md) - GraphQL Schema
- [Database Schema](src/api/Family.Api/Data/README.md) - Entity Models

## Architektur

```
                                                           
                                                           
  Angular Web          Flutter App          Other Clients  
                                                           
         ,                    ,                    ,       
                                                      
                                <                      
                                 
                                 �               
                                                 
                         Family GraphQL API     
                         (ASP.NET Core 9.0)     
                                                 
                                 ,               
                                  
                                 �               
                                                 
                          Keycloak IdP          
                       (Authentication)         
                                                 
                                                 
                                  
                                 �               
                                                 
                         PostgreSQL Database    
                                                 
                                                 
```

## Technologie-Stack

### Backend
- **ASP.NET Core 9.0** - Web API Framework
- **HotChocolate** - GraphQL Server
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **Keycloak** - Identity Provider

### Frontend (geplant)
- **Angular** - Web Application
- **Flutter** - Mobile Application (iOS/Android)

### DevOps
- **Docker Compose** - Container Orchestration
- **GitHub Actions** - CI/CD Pipeline
- **xUnit** - Testing Framework

## Entwicklung

### Projekt Structure

```
family/
   src/
      api/                    # Backend API
         Family.Api/         # Hauptprojekt
         Family.Api.Tests/   # Unit & Integration Tests
      frontend/
         web/               # Angular Web App (geplant)
         app/               # Flutter Mobile App (geplant)
   docs/                      # Dokumentation
   keycloak/                  # Keycloak Konfiguration
   docker-compose.yml         # Entwicklungsumgebung
   README.md
```

### Entwicklung starten

```bash
# Dependencies installieren
dotnet restore

# Code formatieren
dotnet format

# Tests ausf�hren
dotnet test

# API starten (mit Hot Reload)
cd src/api/Family.Api
dotnet watch run
```

### Contributing

1. Fork das Repository
2. Feature Branch erstellen (`git checkout -b feature/amazing-feature`)
3. �nderungen committen (`git commit -m 'Add amazing feature'`)
4. Push to Branch (`git push origin feature/amazing-feature`)
5. Pull Request �ffnen

## Testing

```bash
# Alle Tests
dotnet test

# Mit Coverage (automatisch in CI/CD Pipeline)
dotnet test --collect:"XPlat Code Coverage"

# Strukturierte Test-Ausführung mit Kategorisierung
./scripts/run-structured-tests.sh

# Lokale Coverage-Generierung 
./scripts/generate-coverage.sh

# Nur Unit Tests
dotnet test --filter "Category=Unit"

# Nur Integration Tests
dotnet test --filter "Category=Integration"
```

## Deployment

### Lokale Entwicklung
```bash
docker-compose up -d
```

### Production
Siehe [Deployment Guide](docs/deployment.md) (geplant)

## Lizenz

Dieses Projekt ist Open Source und unter der [MIT License](LICENSE) verf�gbar.

## Support

- **Issues**: [GitHub Issues](https://github.com/andrekirst/family/issues)
- **Discussions**: [GitHub Discussions](https://github.com/andrekirst/family/discussions)
- **Maintainer**: [@andrekirst](https://github.com/andrekirst)

---

P **Star das Projekt** wenn es dir gef�llt!