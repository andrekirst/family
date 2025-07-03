# Workflow Guidelines für AI-Entwicklung

## Überblick

Diese Guidelines stellen sicher, dass bei der AI-gestützten Entwicklung ein konsistenter und nachvollziehbarer Workflow eingehalten wird. Das Ziel ist es, klare Trennungen zwischen verschiedenen Features zu gewährleisten und parallele Entwicklung zu ermöglichen.

## Mandatory Feature Branch Workflow

### 1. Branch-Erstellung für jede Issue-Implementierung

**ZWINGEND**: Für jede Issue-Implementierung MUSS ein separater Feature-Branch erstellt werden.

```bash
# 1. Zum main Branch wechseln
git checkout main

# 2. Neueste Änderungen holen
git pull origin main

# 3. Feature-Branch erstellen
git checkout -b feature/issue-XX-kurze-beschreibung

# 4. Branch zum Remote-Repository pushen
git push -u origin feature/issue-XX-kurze-beschreibung
```

### 2. Branch-Naming-Konventionen

- **Features**: `feature/issue-XX-kurze-beschreibung`
- **Bugfixes**: `bugfix/issue-XX-kurze-beschreibung`
- **Dokumentation**: `docs/issue-XX-kurze-beschreibung`
- **Hotfixes**: `hotfix/issue-XX-kurze-beschreibung`

#### Beispiele für gute Branch-Namen:
- `feature/issue-33-cloud-design-patterns`
- `bugfix/issue-25-authentication-error`
- `docs/issue-40-api-documentation`
- `hotfix/issue-42-critical-security-fix`

### 3. Verbotene Praktiken

**NIEMALS folgende Aktionen durchführen:**

❌ **Direkt auf `main` arbeiten**
```bash
# FALSCH - Niemals direkt auf main committen
git checkout main
git add .
git commit -m "Feature implementation"
```

❌ **Auf bestehenden Feature-Branches arbeiten**
```bash
# FALSCH - Niemals auf fremden Feature-Branches arbeiten
git checkout feature/angular-web-app
git add .
git commit -m "New feature"
```

❌ **Lange lebende Feature-Branches wiederverwenden**
```bash
# FALSCH - Feature-Branches nicht für mehrere Issues verwenden
git checkout feature/old-branch
# Implementierung von neuem Issue #35
```

## Issue-Workflow Integration

### 1. Issue-Auswahl und Vorbereitung

1. **Issue auswählen** mit Label `ready-to-ai-dev`
2. **Aufwandsschätzung** als Kommentar hinzufügen
3. **Feature-Branch erstellen** (siehe oben)
4. **Issue-Status** auf `in progress` setzen

### 2. Entwicklungsprozess

1. **Todo-Liste** für strukturierte Implementierung erstellen
2. **Granulare Commits** nach jedem abgeschlossenen Todo-Punkt
3. **Immediate Push** nach jedem Commit
4. **Conventional Commit Messages** verwenden

### 3. Pull Request und Review

1. **Build-Validierung** vor PR-Erstellung
2. **Pull Request** mit Issue-Referenz (`Fixes #XX`)
3. **Review abwarten** - NIEMALS selbst mergen
4. **Branch-Cleanup** nach erfolgreichem Merge

## Commit-Guidelines

### Conventional Commit Format

```
type(scope): description

body (optional)

footer (optional)
```

#### Commit-Types:
- `feat`: Neue Features
- `fix`: Bugfixes
- `docs`: Dokumentationsänderungen
- `style`: Code-Formatierung
- `refactor`: Code-Refactoring
- `test`: Test-Hinzufügungen/Änderungen
- `chore`: Build-Prozess, Dependency-Updates

#### Beispiele:
```bash
feat(auth): add Keycloak OAuth2 integration

fix(api): resolve null reference in family service

docs(workflow): update branch creation guidelines

test(family): add unit tests for member validation
```

## Workflow-Validierung

### Pre-Commit Checks

Vor jedem Commit sollten folgende Validierungen erfolgen:

1. **Korrekte Branch-Namen** entsprechend der Konvention
2. **Build-Fähigkeit** des Codes
3. **Test-Ausführung** (falls vorhanden)
4. **Lint-Validierung** (falls konfiguriert)

### Pre-PR Checks

Vor PR-Erstellung:

1. **Vollständige Todo-Liste** abgearbeitet
2. **Alle Tests** erfolgreich
3. **Build** erfolgreich
4. **Issue-Aktualisierung** mit Implementierungsdetails

## Troubleshooting

### Häufige Probleme und Lösungen

#### Problem: Falscher Branch für Implementierung

**Situation**: Implementierung bereits auf falschem Branch begonnen

**Lösung**:
```bash
# 1. Aktuellen Stand sichern
git stash

# 2. Korrekten Feature-Branch erstellen
git checkout main
git pull origin main
git checkout -b feature/issue-XX-korrekte-beschreibung

# 3. Änderungen wiederherstellen
git stash pop

# 4. Weitermachen mit korrektem Branch
```

#### Problem: Branch von falschem Base-Branch erstellt

**Situation**: Feature-Branch von anderem Feature-Branch erstellt

**Lösung**:
```bash
# 1. Neuen Branch von main erstellen
git checkout main
git pull origin main
git checkout -b feature/issue-XX-neu

# 2. Commits vom alten Branch cherry-picken
git cherry-pick <commit-hash>

# 3. Alten Branch löschen
git branch -D alter-falscher-branch
```

## Monitoring und Compliance

### Branch-Monitoring

Regelmäßige Überprüfung auf:
- Lange lebende Feature-Branches
- Direkte Commits auf `main`
- Nicht-konforme Branch-Namen
- Verwaiste Branches

### Automated Checks

Empfohlene GitHub Actions für Workflow-Compliance:
- Branch-Naming Validation
- PR-Titel Validation (Issue-Referenz)
- Automatic Branch Cleanup nach Merge

## Best Practices

### Do's ✅

- **Ein Feature-Branch pro Issue**
- **Kleine, atomare Commits**
- **Aussagekräftige Commit-Messages**
- **Regelmäßige Pushes zum Remote-Branch**
- **Issue-Updates mit Implementierungsfortschritt**

### Don'ts ❌

- **Keine direkten main-Commits**
- **Keine Wiederverwendung von Feature-Branches**
- **Keine großen, monolithischen Commits**
- **Keine unleserlichen Commit-Messages**
- **Kein Self-Merging von Pull Requests**

## Fazit

Die Einhaltung dieser Workflow-Guidelines gewährleistet:

1. **Klare Trennung** zwischen verschiedenen Features
2. **Nachvollziehbare Git-Historie**
3. **Parallele Entwicklungsmöglichkeiten**
4. **Effektive Code-Reviews**
5. **Reduzierte Merge-Konflikte**

Alle AI-Entwicklungsaktivitäten MÜSSEN diesen Guidelines folgen, um eine professionelle und wartbare Codebasis zu gewährleisten.