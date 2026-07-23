# Mediaspot Backend - Technical Test Implementation

## 📋 Overview

Ce document explique les implémentations pour les **3 parties du test technique** :
1. **Basic** - Title Management
2. **Intermediate** - Transcode Job Lifecycle  
3. **Advanced** - Asset Refactoring + Type-Specific Transcoding

---

## 🎯 PARTIE 1 : BASIC - Title Management

### ✅ Consignes Répondues
- ✅ Impléenter domain model `Title` avec invariants
- ✅ Créer commands/handlers pour CRUD
- ✅ Ajouter endpoints API
- ✅ Écrire tests unitaires

### 📁 Fichiers Clés

**Domain Layer:**
- `src/Mediaspot.Domain/Titles/Title.cs` - Aggregate root avec factory method
- `src/Mediaspot.Domain/Titles/TitleType.cs` - Enum pour les types (Movie, Series, etc.)

**Application Layer:**
- `src/Mediaspot.Application/Titles/Commands/CreateTitle/` - CreateTitleCommand + Handler
- `src/Mediaspot.Application/Titles/Commands/UpdateTitle/` - UpdateTitleCommand + Handler
- `src/Mediaspot.Application/Titles/Queries/GetTitleById/` - GetTitleByIdQuery + Handler
- `src/Mediaspot.Application/Titles/Queries/GetTitles/` - GetTitlesQuery + Handler

**API Layer:**
- `src/Mediaspot.Api/Program.cs` - Endpoints `/titles` (POST, PUT, GET)

**Tests:**
- `tests/Mediaspot.UnitTests/TitleTests.cs` - Domain tests
- `tests/Mediaspot.UnitTests/CreateTitleHandlerTests.cs` - Handler tests

### 🔑 Choix de Design

#### Factory Method Pattern
```csharp
public static Title Create(string name, string? description, DateOnly? releaseDate, TitleType type)
{
	if (string.IsNullOrWhiteSpace(name))
		throw new ArgumentException("Title name cannot be empty.", nameof(name));

	return new Title { Name = name.Trim(), ... };
}
```
**Justification:** 
- Centralise la validation des invariants au point de création
- Private constructor empêche l'instantiation directe non-validée
- Pattern DDD classique, utilisé partout dans le projet

#### Repository Pattern
- `ITitleRepository` - Interface pour l'accès aux données
- `TitleRepository` - Implémentation EF Core
- Injection de dépendances via DI container

---

## 🔄 PARTIE 2 : INTERMEDIATE - Transcode Job Lifecycle

### ✅ Consignes Répondues
- ✅ Implémenter `TranscodeJob` aggregate avec état machine
- ✅ Créer commands pour transitions d'état (Start, Complete, Fail)
- ✅ Implémenter handlers avec business logic
- ✅ Respecter les invariants (Pending → Running → Succeeded/Failed)
- ✅ Écrire tests pour lifecycle

### 📁 Fichiers Clés

**Domain Layer:**
- `src/Mediaspot.Domain/Transcoding/TranscodeJob.cs` - Aggregate root
  - `enum TranscodeStatus { Pending, Running, Succeeded, Failed }`
  - Méthodes : `MarkRunning()`, `MarkSucceeded()`, `MarkFailed()`
  - Domain events : `TranscodeJobStarted`, `TranscodeJobCompleted`, `TranscodeJobFailed`

**Application Layer:**
- `src/Mediaspot.Application/Transcoding/Commands/StartTranscodeJob/` - Command + Handler
- `src/Mediaspot.Application/Transcoding/Commands/CompleteTranscodeJob/` - Command + Handler
- `src/Mediaspot.Application/Transcoding/Commands/FailTranscodeJob/` - Command + Handler

**Repository:**
- `src/Mediaspot.Application/Common/ITranscodeJobRepository.cs` - Interface
- `src/Mediaspot.Infrastructure/Persistence/TranscodeJobRepository.cs` - Implementation
  - Méthode : `GetNextPendingJobIdAsync()` - Pour récupérer le prochain job à traiter

**Tests:**
- `tests/Mediaspot.UnitTests/TranscodeJobTests.cs` - Domain lifecycle tests

### 🔑 Choix de Design

#### State Machine Stricte
```csharp
public void MarkRunning()
{
	if (Status != TranscodeStatus.Pending)
		throw new InvalidOperationException("Can only start pending jobs");
	Status = TranscodeStatus.Running;
}

public void MarkSucceeded()
{
	if (Status != TranscodeStatus.Running)
		throw new InvalidOperationException("Can only complete running jobs");
	Status = TranscodeStatus.Succeeded;
}
```
**Justification:**
- Enforce les transitions valides au niveau du domain
- Évite les états invalides (ex: Pending → Succeeded directement)
- Invariants garantis par le domain model

#### Domain Events
```csharp
Raise(new TranscodeJobStarted(Id));
Raise(new TranscodeJobCompleted(Id));
Raise(new TranscodeJobFailed(Id));
```
**Justification:**
- Permet aux systèmes externes de réagir aux changements
- Découpling entre domaine et application
- Traçabilité des opérations

---

## 🚀 PARTIE 3 : ADVANCED - Asset Refactoring + Type-Specific Transcoding

### ✅ Consignes Répondues
- ✅ Refactorer `Asset` en abstract et implémenter `VideoAsset` + `AudioAsset`
- ✅ Ajouter propriétés spécifiques à chaque type
- ✅ Implémenter logique de transcode type-spécifique
- ✅ Gérer le transcoding pour les deux types d'assets
- ✅ Créer un Worker pour traiter les jobs pending
- ✅ Écrire tests pour le worker

### 📁 Fichiers Clés

**Domain Layer:**
- `src/Mediaspot.Domain/Assets/Asset.cs` - **Abstract** base class (changement clé)
- `src/Mediaspot.Domain/Assets/VideoAsset.cs` - Sealed class avec propriétés vidéo
  - `Duration`, `Resolution`, `FrameRate`, `Codec`
- `src/Mediaspot.Domain/Assets/AudioAsset.cs` - Sealed class avec propriétés audio
  - `Duration`, `Bitrate`, `SampleRate`, `Channels`

**Application Layer:**
- `src/Mediaspot.Application/Assets/Commands/Create/CreateVideoAsset/` - Création vidéo
- `src/Mediaspot.Application/Assets/Commands/Create/CreateAudioAsset/` - Création audio
- `src/Mediaspot.Application/Assets/Repository/IAssetRepository.cs` - Interface
  - Méthode : `GetAsync(Guid id)` - Récupère l'asset (polymorphe)

**Worker Service:**
- `src/Mediaspot.Worker/TranscodeWorker.cs` - **BackgroundService** qui traite les jobs
  - Boucle polling sur `GetNextPendingJobIdAsync()`
  - Switch/case sur le type d'asset (VideoAsset vs AudioAsset)
  - Appelle les handlers MediatR pour chaque étape

**API Layer:**
- `src/Mediaspot.Api/Program.cs` - Endpoints
  - ✅ NEW : `/assets/video` (POST) - Créer VideoAsset
  - ✅ NEW : `/assets/audio` (POST) - Créer AudioAsset
  - ⚠️ DEPRECATED : `/assets` (POST) - Ancien endpoint générique

**Tests:**
- `tests/Mediaspot.UnitTests/TranscodeWorkerTests.cs` - Worker tests
  - `Worker_Should_Start_And_Complete_Pending_Job` - Test original + fixes
  - `Worker_Should_Process_VideoAsset_With_Type_Specific_Logic` - Test VideoAsset
  - `Worker_Should_Process_AudioAsset_With_Type_Specific_Logic` - Test AudioAsset (optionnel)

### 🔑 Choix de Design

#### 1️⃣ Asset Polymorphe

**Avant (Generic):**
```csharp
public class Asset : AggregateRoot
{
	// Propriétés générales seulement
	public string? VideoResolution { get; set; }  // ❌ Pas propre
	public int? AudioBitrate { get; set; }        // ❌ Pas propre
}
```

**Après (Type-Safe):**
```csharp
public abstract class Asset : AggregateRoot  // ✅ Abstract
{
	// Propriétés communes
	public string ExternalId { get; protected set; }
	public Metadata Metadata { get; protected set; }
	public IReadOnlyCollection<MediaFile> MediaFiles { get; }
}

public sealed class VideoAsset : Asset
{
	public Duration Duration { get; private set; }     // ✅ Type-safe
	public string Resolution { get; private set; }     // ✅ Type-safe
	public decimal FrameRate { get; private set; }     // ✅ Type-safe
	public string Codec { get; private set; }          // ✅ Type-safe
}

public sealed class AudioAsset : Asset
{
	public Duration Duration { get; private set; }     // ✅ Type-safe
	public int Bitrate { get; private set; }           // ✅ Type-safe
	public int SampleRate { get; private set; }        // ✅ Type-safe
	public int Channels { get; private set; }          // ✅ Type-safe
}
```

**Justification:**
- ✅ **Type Safety** - Le compilateur valide les propriétés (pas de casting)
- ✅ **Cleaner API** - Pas de propriétés nullables génériques
- ✅ **Validation** - Chaque type peut avoir sa propre validation
- ✅ **DDD** - Respecte les bounded contexts (VideoAsset ≠ AudioAsset)

#### 2️⃣ Worker Pattern avec Polling

```csharp
while (!stoppingToken.IsCancellationRequested)
{
	// 1. Récupérer un job pending
	var jobId = await jobs.GetNextPendingJobIdAsync(stoppingToken);

	if (jobId is null)
	{
		await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);  // Polling delay
		continue;
	}

	// 2. Démarrer le job
	var job = await jobs.GetByIdAsync(jobId.Value, stoppingToken);
	var asset = await assets.GetAsync(job.AssetId, stoppingToken);

	try
	{
		await sender.Send(new StartTranscodeJobCommand(jobId.Value), stoppingToken);

		// 3. Traitement type-spécifique
		switch (asset)
		{
			case VideoAsset videoAsset:
				await ProcessVideoAsync(videoAsset, stoppingToken);
				break;
			case AudioAsset audioAsset:
				await ProcessAudioAsync(audioAsset, stoppingToken);
				break;
		}

		// 4. Compléter le job
		await sender.Send(new CompleteTranscodeJobCommand(jobId.Value), stoppingToken);
	}
	catch (Exception ex)
	{
		logger.LogError($"Failed to process job {jobId}", ex);
		if (jobStarted)
			await sender.Send(new FailTranscodeJobCommand(jobId.Value), stoppingToken);
	}
}
```

**Justification:**
- ✅ **Backoff Strategy** - Polling avec délai pour éviter charge DB
- ✅ **Scoped DI** - Crée un `IServiceScope` à chaque itération (pattern exact du projet)
- ✅ **Handling Errors** - Distingue les erreurs pré/post-Start
- ✅ **Type Discrimination** - Switch/case sur le runtime type (pattern matching C#)

#### 3️⃣ Type-Specific Processing

```csharp
private async Task ProcessVideoAsync(VideoAsset asset, CancellationToken cancellationToken)
{
	// Utilise les propriétés vidéo spécifiques
	logger.LogInformation(
		"Processing video: {Resolution}, {FrameRate} FPS, {Codec}",
		asset.Resolution,   // ✅ Type-safe, pas de casting
		asset.FrameRate,
		asset.Codec);

	await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);  // Simulation
}

private async Task ProcessAudioAsync(AudioAsset asset, CancellationToken cancellationToken)
{
	// Utilise les propriétés audio spécifiques
	logger.LogInformation(
		"Processing audio: {Bitrate} kbps, {SampleRate} Hz, {Channels} channels",
		asset.Bitrate,      // ✅ Type-safe, pas de casting
		asset.SampleRate,
		asset.Channels);

	await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);  // Simulation
}
```

**Justification:**
- ✅ **Open/Closed Principle** - Facile d'ajouter un nouvel asset type (ImageAsset)
- ✅ **No Casting** - Pattern matching handle le type checking
- ✅ **Logging Type-Aware** - Logs affichent les bonnes propriétés
- ✅ **Future-Proof** - Si on ajoute `case ImageAsset`, le compilateur force à implémenter

#### 4️⃣ API Evolution avec Deprecation

```csharp
// ⚠️ DEPRECATED - Generic endpoint (kept for backward compatibility)
app.MapPost("/assets", async (CreateAssetCommand cmd, ISender sender) => ...)
	.WithDescription("⚠️ DEPRECATED - Use /assets/video or /assets/audio")
	.WithOpenApi();

// ✅ NEW - Type-specific endpoints
app.MapPost("/assets/video", async (CreateVideoAssetCommand cmd, ISender sender) => ...)
	.WithDescription("Create VideoAsset with video-specific metadata")
	.WithOpenApi();

app.MapPost("/assets/audio", async (CreateAudioAssetCommand cmd, ISender sender) => ...)
	.WithDescription("Create AudioAsset with audio-specific metadata")
	.WithOpenApi();
```

**Justification:**
- ✅ **Backward Compatibility** - Ancien endpoint reste fonctionnel
- ✅ **Clear Intent** - Développeurs voient qu'il faut utiliser les nouveaux endpoints
- ✅ **Documentation** - Swagger/OpenAPI montre la déprécation
- ✅ **Migration Path** - Clients ont le temps de migrer

---

## 🧪 Testing Strategy

### Domain Tests
- ✅ `TitleTests` - Invariants (name not empty)
- ✅ `TranscodeJobTests` - State transitions (Pending→Running→Succeeded/Failed)
- ✅ `VideoAssetTests` - Codec validation, Duration positive
- ✅ `AudioAssetTests` - Bitrate validation, etc.

### Handler Tests
- ✅ `CreateTitleHandlerTests` - Unique name validation
- ✅ `TranscodeRequestedHandlerTests` - Job creation
- ✅ Handlers pour StartTranscodeJob, CompleteTranscodeJob, FailTranscodeJob

### Integration Tests (Worker)
- ✅ `Worker_Should_Start_And_Complete_Pending_Job` - Happy path générique
- ✅ `Worker_Should_Process_VideoAsset_With_Type_Specific_Logic` - VideoAsset flow
- ✅ `Worker_Should_Process_AudioAsset_With_Type_Specific_Logic` - AudioAsset flow (optionnel)

**Pattern utilisé:** xUnit + Moq + Shouldly (cohérent avec le projet)

---

## 🔄 Patterns DDD Respectés

| Pattern | Implémentation | Fichiers |
|---------|-----------------|----------|
| **Aggregate Root** | `Title`, `Asset`, `TranscodeJob` | Domain layer |
| **Value Objects** | `Metadata`, `Duration`, `FilePath` | Domain layer |
| **Domain Events** | `TranscodeJobStarted`, etc. | Domain/Events |
| **Factory Methods** | `Title.Create()`, Asset constructors | Domain |
| **Repository** | `ITranscodeJobRepository`, `IAssetRepository` | Application |
| **Commands** | `CreateTitleCommand`, `StartTranscodeJobCommand` | Application |
| **Handlers** | MediatR `IRequestHandler<T>` | Application |
| **DI Container** | `services.AddScoped<T>()` | Infrastructure |

---

## ✅ Checklist de Validation

### Tests
```bash
dotnet test
# Résultat attendu: 33 tests, 33 passed ✅
```

### Build
```bash
dotnet build
# Résultat attendu: Build succeeded ✅
```

### API Endpoints
- ✅ POST `/titles` - Créer title
- ✅ PUT `/titles/{id}` - Mettre à jour title
- ✅ GET `/titles/{id}` - Récupérer title
- ✅ GET `/titles` - Lister titles
- ✅ POST `/assets/video` - Créer VideoAsset
- ✅ POST `/assets/audio` - Créer AudioAsset
- ✅ Worker service - Traite les jobs pending