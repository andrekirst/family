# Internationalization (i18n) Implementation

## Überblick

Das Family-Projekt unterstützt vollständige Mehrsprachigkeit für sowohl Backend (ASP.NET Core) als auch Frontend (Angular). Standardsprache ist Deutsch (de), mit Englisch (en) als Fallback-Sprache.

## Backend Lokalisierung (ASP.NET Core)

### Infrastruktur

Die Backend-Lokalisierung basiert auf dem ASP.NET Core Localization Framework:

```csharp
// Program.cs
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("de"),
        new CultureInfo("en")
    };
    
    options.DefaultRequestCulture = new RequestCulture("de");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});
```

### Resource Files

Resource-Dateien sind in einer strukturierten Hierarchy organisiert:

```
Family.Api/Resources/
├── Features/
│   └── Users/
│       ├── UserValidationMessages.de.resx
│       └── UserValidationMessages.en.resx
```

#### Marker Classes

Für typsichere Lokalisierung werden Marker-Klassen verwendet:

```csharp
// UserValidationMessages.cs
public class UserValidationMessages
{
}
```

### Verwendung in Validators

FluentValidation Validators nutzen IStringLocalizer für lokalisierte Nachrichten:

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IStringLocalizer<UserValidationMessages> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["EmailRequired"])
            .EmailAddress().WithMessage(localizer["EmailInvalid"])
            .MaximumLength(255).WithMessage(x => localizer["EmailMaxLength", 255]);
    }
}
```

### Command Handler Lokalisierung

Command Handlers verwenden lokalisierte Fehlermeldungen:

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CommandResult<UserDto>>
{
    private readonly IStringLocalizer<UserValidationMessages> _localizer;

    public async Task<CommandResult<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (existingUser != null)
        {
            return CommandResult<UserDto>.Failure(_localizer["EmailAlreadyExists"]);
        }
        // ...
    }
}
```

## Frontend Lokalisierung (Angular)

### Services

#### LocalizationService

Zentrale Verwaltung der Sprache und Locale-Einstellungen:

```typescript
@Injectable({ providedIn: 'root' })
export class LocalizationService {
  readonly supportedLocales: SupportedLocale[] = [
    { code: 'de', name: 'Deutsch', flag: '🇩🇪' },
    { code: 'en', name: 'English', flag: '🇺🇸' }
  ];

  setLocale(locale: string): void {
    if (this.isSupportedLocale(locale)) {
      this.setCurrentLocale(locale);
      this.saveLocale(locale);
      window.location.reload(); // Reload für Language Change
    }
  }
}
```

#### I18nService

Übersetzungslogik mit Parameter-Unterstützung:

```typescript
@Injectable({ providedIn: 'root' })
export class I18nService {
  translate(key: string, params?: { [key: string]: string | number }): string {
    let translation = this.currentTranslations[key] || key;
    
    if (params) {
      Object.keys(params).forEach(param => {
        const placeholder = `{${param}}`;
        translation = translation.replace(new RegExp(placeholder, 'g'), params[param].toString());
      });
    }
    
    return translation;
  }
}
```

### Translation Files

JSON-basierte Übersetzungsdateien:

```
src/locale/
├── messages.de.json
└── messages.en.json
```

#### Struktur

```json
{
  "locale": "de",
  "translations": {
    "@@locale": "de",
    "app.title": "Family",
    "nav.dashboard": "Dashboard",
    "common.save": "Speichern",
    "validation.required": "Dieses Feld ist erforderlich",
    "user.createSuccess": "Benutzer erfolgreich erstellt"
  }
}
```

### Components

#### LanguageSwitcherComponent

Material Design Language Switcher mit Dropdown-Menü:

```typescript
@Component({
  selector: 'app-language-switcher',
  template: `
    <button mat-button [matMenuTriggerFor]="languageMenu">
      <mat-icon>language</mat-icon>
      <span class="language-flag">{{ currentLocaleInfo.flag }}</span>
      <span class="language-name">{{ currentLocaleInfo.name }}</span>
    </button>
    <mat-menu #languageMenu="matMenu">
      <button mat-menu-item 
              *ngFor="let locale of supportedLocales" 
              (click)="switchLanguage(locale.code)">
        <span class="language-flag">{{ locale.flag }}</span>
        <span class="language-name">{{ locale.name }}</span>
      </button>
    </mat-menu>
  `
})
```

#### I18nPipe

Custom Pipe für Template-Übersetzungen:

```typescript
@Pipe({ name: 'i18n', pure: false })
export class I18nPipe implements PipeTransform {
  transform(key: string, params?: { [key: string]: string | number }): string {
    return this.i18nService.translate(key, params);
  }
}
```

### Verwendung in Templates

```html
<!-- Einfache Übersetzung -->
<span>{{ 'nav.dashboard' | i18n }}</span>

<!-- Mit Parametern -->
<span>{{ 'validation.maxLength' | i18n:{ max: 255 } }}</span>

<!-- In Attributen -->
<button [attr.aria-label]="'common.save' | i18n">
```

## Features

### Automatische Spracherkennung

1. **Browser-Sprache**: Automatische Erkennung der Browser-Sprache
2. **LocalStorage**: Persistierung der Benutzerauswahl
3. **Accept-Language Header**: Backend-Integration für API-Calls
4. **Fallback**: Automatischer Fallback auf Deutsch wenn Sprache nicht unterstützt

### Responsive Design

- Desktop: Vollständige Sprach-Informationen (Flag + Name)
- Mobile: Nur Flag und Icon für platzsparende Darstellung

### Parameter-Unterstützung

Dynamische Übersetzungen mit Platzhaltern:

```typescript
// Translation: "Mindestens {min} Zeichen erforderlich"
this.i18nService.translate('validation.minLength', { min: 8 });
// Result: "Mindestens 8 Zeichen erforderlich"
```

## Build-Konfiguration

### Angular Build

```json
{
  "configurations": {
    "de": {
      "aot": true,
      "outputPath": "dist/de/",
      "i18nMissingTranslation": "error"
    },
    "en": {
      "aot": true,
      "outputPath": "dist/en/",
      "i18nMissingTranslation": "error"
    }
  }
}
```

### Polyfills

```json
{
  "polyfills": [
    "zone.js",
    "@angular/localize/init"
  ]
}
```

## Testing

### Unit Tests

Tests für alle Lokalisierungs-Services und -Components:

- `LocalizationService.spec.ts`: Sprach-Management
- `I18nService.spec.ts`: Übersetzungslogik
- `I18nPipe.spec.ts`: Pipe-Funktionalität
- `LanguageSwitcherComponent.spec.ts`: UI-Component

### Test-Patterns

```typescript
it('should translate with parameters', () => {
  const params = { name: 'John' };
  spyOn(i18nService, 'translate').and.returnValue('Hello John');
  
  const result = pipe.transform('welcome.message', params);
  expect(i18nService.translate).toHaveBeenCalledWith('welcome.message', params);
  expect(result).toBe('Hello John');
});
```

## Performance

### Lazy Loading

- Translation Files werden lazy geladen
- Separate Chunks für jede Sprache
- Optimierte Bundle-Größe

### Caching

- Browser-LocalStorage für Sprach-Präferenzen
- In-Memory Caching von Übersetzungen
- Observable-Pattern für Change Detection

## Erweiterung

### Neue Sprachen hinzufügen

1. **Backend**: Neue .resx Files erstellen
2. **Frontend**: Neue JSON Translation Files
3. **Service**: SupportedLocales Array erweitern
4. **Build**: Angular-Konfiguration anpassen

### Neue Übersetzungsschlüssel

1. **Backend**: Resource Files aktualisieren
2. **Frontend**: JSON Files ergänzen
3. **Usage**: IStringLocalizer oder I18nPipe verwenden

## Konventionen

### Naming

- **Keys**: Dot-notation (`nav.dashboard`, `user.createSuccess`)
- **Namespaces**: Feature-basiert (`nav`, `common`, `user`, `validation`)
- **Parameters**: CamelCase (`{maxLength}`, `{userName}`)

### Resource Files

- **Locale-suffix**: `.de.resx`, `.en.resx`
- **Namespace-Organisation**: Feature-bezogene Ordnerstruktur
- **Marker Classes**: Eine pro Resource-File

### Code Standards

- **Keine hardcodierten Strings** in User-Interfaces
- **Typsichere Lokalisierung** mit Marker Classes
- **Parameter-Validation** für dynamische Übersetzungen
- **Fallback-Handling** für fehlende Übersetzungen

## Troubleshooting

### Häufige Probleme

1. **Fehlende Übersetzungen**: Prüfe ob alle Keys in beiden Sprachen vorhanden
2. **Parameter nicht ersetzt**: Überprüfe Parameter-Namen und Syntax
3. **Sprache wird nicht gewechselt**: LocalStorage und Browser-Cache leeren
4. **Build-Fehler**: Verifiziere Angular i18n Konfiguration

### Debug-Tipps

- Browser DevTools für LocalStorage-Inspektion
- Network-Tab für Accept-Language Headers
- Console-Logs für Translation-Loading

## Migration Guide

### Bestehende Texte lokalisieren

1. Hardcodierte Strings identifizieren
2. Translation Keys definieren
3. Resource Files erstellen/erweitern
4. Code auf Lokalisierung umstellen
5. Tests aktualisieren

### Breaking Changes vermeiden

- Bestehende Keys beibehalten
- Fallback-Mechanismen nutzen
- Schrittweise Migration durchführen