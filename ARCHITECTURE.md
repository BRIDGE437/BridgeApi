# BRIDGE Platform - Mimari Dokümantasyon

**Hedef Kitle:** Clean Architecture, CQRS ve MediatR ile yeni tanışan geliştiriciler  
**Versiyon:** 1.0  
**Son Güncelleme:** 2026-02-09

---

## 📑 İçindekiler

1. [Genel Bakış](#genel-bakış)
2. [Clean Architecture Nedir?](#clean-architecture-nedir)
3. [Katman Yapısı](#katman-yapısı)
4. [CQRS Pattern](#cqrs-pattern)
5. [MediatR Pattern](#mediatr-pattern)
6. [Dependency Flow (Bağımlılık Akışı)](#dependency-flow-bağımlılık-akışı)
7. [Örnek Senaryolar](#örnek-senaryolar)
8. [Sık Sorulan Sorular](#sık-sorulan-sorular)

---

## 🎯 Genel Bakış

### Bu Dokümantasyonun Amacı

Bu dokümantasyon, BRIDGE platformunun teknik mimarisini anlamak için hazırlanmıştır. Özellikle:
- Clean Architecture ile ilk kez çalışan geliştiriciler
- CQRS pattern'ini öğrenen ekip üyeleri
- MediatR kullanımını anlamak isteyenler

için detaylı açıklamalar içerir.

### Mimari Yaklaşımımız

```
┌─────────────────────────────────────────────────────────┐
│                    PRESENTATION                         │
│              (BridgeApi.API - Controllers)             │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                    APPLICATION                          │
│    (BridgeApi.Application - Commands, Queries, DTOs)   │
│                    MediatR Pipeline                     │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                      DOMAIN                             │
│        (BridgeApi.Domain - Entities, Value Objects)     │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  INFRASTRUCTURE                         │
│  (Persistence, RealtimeCommunication, Infrastructure)  │
└─────────────────────────────────────────────────────────┘
```

### Temel Prensipler

1. **Separation of Concerns (Sorumlulukların Ayrılması)**
   - Her katman kendi sorumluluğuna odaklanır
   - Katmanlar arası bağımlılıklar tek yönlüdür

2. **Dependency Inversion (Bağımlılık Tersine Çevirme)**
   - Üst katmanlar alt katmanlara değil, abstraction'lara bağımlıdır
   - Domain katmanı hiçbir katmana bağımlı değildir

3. **Command Query Separation (CQRS)**
   - Veri okuma (Query) ve yazma (Command) işlemleri ayrılır
   - Her işlem için özel handler'lar kullanılır

---

## 🏗️ Clean Architecture Nedir?

### Klasik Yaklaşım vs Clean Architecture

#### ❌ Klasik Yaklaşım (N-Layer Architecture)

```
Controller → Service → Repository → Database
```

**Sorunlar:**
- Controller direkt Service'e bağımlı
- Service direkt Repository'ye bağımlı
- Database değiştiğinde tüm katmanlar etkilenir
- Business logic test edilemez (database'e bağımlı)
- Katmanlar arası sıkı bağlılık

#### ✅ Clean Architecture

```
Controller → Application (Commands/Queries) → Domain → Infrastructure
```

**Avantajlar:**
- Domain (business logic) hiçbir şeye bağımlı değil
- Infrastructure değiştiğinde Domain etkilenmez
- Her katman bağımsız test edilebilir
- Business logic merkezi ve korunmuş

### Clean Architecture Katmanları

#### 1. Domain (Çekirdek Katman)
- **Ne içerir?**
  - Entity'ler (User, Post, Match, Chat)
  - Value Objects (Email, PhoneNumber)
  - Domain Events
  - Business Rules (validation logic)

- **Ne içermez?**
  - Database bağımlılıkları
  - Framework bağımlılıkları
  - External service bağımlılıkları

- **Örnek:**
```csharp
// Domain/BridgeApi.Domain/Entities/User.cs
public class User
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; } // Value Object
    public string Name { get; private set; }
    
    // Business logic içerir
    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new DomainException("Email cannot be null");
            
        Email = newEmail;
        // Domain event fırlatılabilir
    }
}
```

#### 2. Application (Uygulama Katmanı)
- **Ne içerir?**
  - Commands (veri yazma işlemleri)
  - Queries (veri okuma işlemleri)
  - DTOs (Data Transfer Objects)
  - Command/Query Handlers
  - Application Services (gerekirse)

- **Ne içermez?**
  - HTTP context (Controller'dan gelir)
  - Database context (Infrastructure'dan gelir)

- **Örnek:**
```csharp
// Application/BridgeApi.Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs
public record CreateUserCommand(string Email, string Name) : IRequest<Guid>;

// Application/BridgeApi.Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    
    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);
        var user = new User(email, request.Name);
        
        await _userRepository.AddAsync(user, cancellationToken);
        
        return user.Id;
    }
}
```

#### 3. Infrastructure (Altyapı Katmanı)
- **Ne içerir?**
  - Database implementations (EF Core)
  - External service implementations (Redis, SignalR)
  - File system operations
  - Email services
  - Third-party library integrations

- **Örnek:**
```csharp
// Infrastructure/BridgeApi.Persistence/Repositories/UserRepository.cs
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

#### 4. Presentation (Sunum Katmanı)
- **Ne içerir?**
  - Controllers (API endpoints)
  - Middleware'ler
  - DTO mapping (Application'dan gelen DTO'ları HTTP response'a çevirme)

- **Örnek:**
```csharp
// Presentation/BridgeApi.API/Controllers/UsersController.cs
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateUser(CreateUserCommand command)
    {
        var userId = await _mediator.Send(command);
        return Ok(userId);
    }
}
```

---

## 📦 Katman Yapısı

### Proje Klasör Yapısı

```
BridgeApi/
├── Core/
│   ├── BridgeApi.Domain/              # Domain katmanı
│   │   ├── Entities/                  # Entity'ler
│   │   ├── ValueObjects/              # Value Objects
│   │   ├── Events/                    # Domain Events
│   │   └── Exceptions/                # Domain Exceptions
│   │
│   └── BridgeApi.Application/         # Application katmanı
│       ├── Features/                  # Feature-based organization
│       │   ├── Users/
│       │   │   ├── Commands/
│       │   │   │   └── CreateUser/
│       │   │   │       ├── CreateUserCommand.cs
│       │   │   │       ├── CreateUserCommandHandler.cs
│       │   │   │       └── CreateUserCommandValidator.cs
│       │   │   └── Queries/
│       │   │       └── GetUser/
│       │   │           ├── GetUserQuery.cs
│       │   │           └── GetUserQueryHandler.cs
│       │   ├── Posts/
│       │   └── Matches/
│       ├── Common/                    # Ortak kullanılan sınıflar
│       │   ├── Behaviors/             # MediatR behaviors
│       │   ├── Mappings/              # AutoMapper profiles
│       │   └── Interfaces/            # Repository interfaces
│       └── DTOs/                      # Data Transfer Objects
│
├── Infrastructure/
│   ├── BridgeApi.Persistence/         # Database katmanı
│   │   ├── Repositories/              # Repository implementations
│   │   ├── Configurations/            # EF Core configurations
│   │   └── ApplicationDbContext.cs
│   │
│   ├── BridgeApi.Infrastructure/      # Genel infrastructure
│   │   ├── Services/                  # External services
│   │   └── ServiceRegistration.cs
│   │
│   └── BridgeApi.RealtimeCommunication/ # SignalR
│       └── Hubs/
│
└── Presentation/
    └── BridgeApi.API/                 # API katmanı
        ├── Controllers/
        ├── Middleware/
        └── Program.cs
```

### Katman Bağımlılıkları

```
Presentation (API)
    ↓ (bağımlı)
Application
    ↓ (bağımlı)
Domain
    ↑ (bağımlı)
Infrastructure (Persistence, Infrastructure, RealtimeCommunication)
```

**Kural:** 
- Domain hiçbir katmana bağımlı değil
- Application sadece Domain'e bağımlı
- Infrastructure hem Domain hem Application'a bağımlı
- Presentation Application'a bağımlı

---

## 🔄 CQRS Pattern

### CQRS Nedir?

**CQRS = Command Query Responsibility Segregation**

Basitçe: **Veri okuma (Query) ve yazma (Command) işlemlerini ayırma**

### Neden CQRS?

#### ❌ Klasik Yaklaşım

```csharp
// Tek bir service hem okuma hem yazma yapar
public class UserService
{
    public async Task<User> GetUser(Guid id) { ... }      // Query
    public async Task CreateUser(User user) { ... }       // Command
    public async Task UpdateUser(User user) { ... }       // Command
    public async Task<List<User>> GetUsers() { ... }      // Query
}
```

**Sorunlar:**
- Service çok fazla sorumluluk alır
- Query ve Command için farklı optimizasyonlar yapılamaz
- Test yazmak zorlaşır
- Kod karmaşıklaşır

#### ✅ CQRS Yaklaşımı

```csharp
// Query - Sadece okuma
public class GetUserQuery : IRequest<UserDto>
{
    public Guid UserId { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    // Sadece okuma işlemi
}

// Command - Sadece yazma
public class CreateUserCommand : IRequest<Guid>
{
    public string Email { get; set; }
    public string Name { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    // Sadece yazma işlemi
}
```

**Avantajlar:**
- Her handler tek sorumluluğa sahip
- Query'ler için farklı optimizasyonlar (caching, read models)
- Command'ler için farklı optimizasyonlar (validation, transactions)
- Test yazmak kolay
- Kod daha okunabilir

### Command vs Query

| Özellik | Command | Query |
|---------|---------|-------|
| **Amaç** | Veri değiştirme | Veri okuma |
| **Return Type** | Genelde void veya ID | DTO veya Entity |
| **Side Effect** | Var (database write) | Yok (sadece read) |
| **Validation** | Zorunlu | Opsiyonel |
| **Transaction** | Genelde var | Yok |
| **Caching** | Cache invalidation | Cache kullanılabilir |

### Command Örneği

```csharp
// 1. Command tanımı
public record CreateUserCommand(string Email, string Name) : IRequest<Guid>;

// 2. Command Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    
    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validation (FluentValidation ile yapılabilir)
        if (string.IsNullOrEmpty(request.Email))
            throw new ValidationException("Email is required");
        
        // Business logic
        var email = new Email(request.Email);
        var user = new User(email, request.Name);
        
        // Persistence
        await _userRepository.AddAsync(user, cancellationToken);
        
        // Return
        return user.Id;
    }
}

// 3. Controller'da kullanım
[HttpPost]
public async Task<ActionResult<Guid>> CreateUser(CreateUserCommand command)
{
    var userId = await _mediator.Send(command);
    return Ok(userId);
}
```

### Query Örneği

```csharp
// 1. Query tanımı
public record GetUserQuery(Guid UserId) : IRequest<UserDto>;

// 2. Query Handler
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    
    public GetUserQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Database'den okuma
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");
        
        // Entity'yi DTO'ya çevirme
        return _mapper.Map<UserDto>(user);
    }
}

// 3. Controller'da kullanım
[HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUser(Guid id)
{
    var query = new GetUserQuery(id);
    var userDto = await _mediator.Send(query);
    return Ok(userDto);
}
```

### CQRS Avantajları (Bizim Projede)

1. **Feed Generation**
   - Query: Feed okuma → Cache'lenebilir, optimize edilebilir
   - Command: Post oluşturma → Cache invalidation

2. **Match Detection**
   - Command: Like işlemi → Event fırlatır
   - Query: Match listesi → Farklı optimizasyonlar

3. **Chat System**
   - Command: Mesaj gönderme → SignalR notification
   - Query: Chat history → Pagination, caching

---

## 📨 MediatR Pattern

### MediatR Nedir?

**MediatR = Mediator Pattern implementation**

Basitçe: **Controller'lar direkt service'lere değil, MediatR'a istek gönderir. MediatR uygun handler'ı bulur ve çalıştırır.**

### Neden MediatR?

#### ❌ Klasik Yaklaşım

```csharp
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILoggingService _loggingService;
    
    public UsersController(
        IUserService userService,
        IEmailService emailService,
        ILoggingService loggingService)
    {
        // Çok fazla dependency!
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateUser(CreateUserRequest request)
    {
        // Controller'da business logic?
        await _loggingService.Log("Creating user");
        var user = await _userService.CreateUser(request);
        await _emailService.SendWelcomeEmail(user.Email);
        return Ok(user);
    }
}
```

**Sorunlar:**
- Controller çok fazla dependency alır
- Business logic controller'da olabilir
- Test yazmak zor
- Cross-cutting concerns (logging, validation) her yerde tekrarlanır

#### ✅ MediatR Yaklaşımı

```csharp
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public UsersController(IMediator mediator)
    {
        // Sadece MediatR!
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateUser(CreateUserCommand command)
    {
        var userId = await _mediator.Send(command);
        return Ok(userId);
    }
}
```

**Avantajlar:**
- Controller sadece MediatR'a bağımlı
- Business logic handler'larda
- Cross-cutting concerns pipeline'da (behavior'lar)
- Test yazmak kolay

### MediatR Nasıl Çalışır?

```
1. Controller → MediatR.Send(command)
2. MediatR → Handler'ı bulur
3. MediatR → Pipeline behaviors çalıştırır (validation, logging, transaction)
4. MediatR → Handler'ı çalıştırır
5. Handler → Business logic'i yürütür
6. Handler → Result döner
7. MediatR → Controller'a result döner
```

### MediatR Pipeline (Behaviors)

Pipeline behaviors, her command/query çalışmadan önce ve sonra çalışan middleware'lerdir.

**Örnek Pipeline:**

```
Request → LoggingBehavior → ValidationBehavior → TransactionBehavior → Handler → Response
```

#### Logging Behavior Örneği

```csharp
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Request öncesi
        _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Handler'ı çalıştır
            var response = await next();
            
            // Response sonrası
            stopwatch.Stop();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms", 
                typeof(TRequest).Name, 
                stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
```

#### Validation Behavior Örneği

```csharp
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Validation kontrolü
        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();
        
        if (failures.Any())
            throw new ValidationException(failures);
        
        // Validation başarılı, handler'ı çalıştır
        return await next();
    }
}
```

#### Transaction Behavior Örneği

```csharp
public class TransactionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ApplicationDbContext _context;
    
    public TransactionBehavior(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Transaction başlat
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Handler'ı çalıştır
            var response = await next();
            
            // Commit
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return response;
        }
        catch
        {
            // Rollback
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

### MediatR Notifications (Events)

MediatR sadece Command/Query için değil, event'ler için de kullanılır.

**Örnek: Match Created Event**

```csharp
// 1. Event tanımı
public class MatchCreatedEvent : INotification
{
    public Guid MatchId { get; set; }
    public Guid UserId1 { get; set; }
    public Guid UserId2 { get; set; }
}

// 2. Event Handler
public class MatchCreatedEventHandler : INotificationHandler<MatchCreatedEvent>
{
    private readonly IMediator _mediator;
    
    public MatchCreatedEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task Handle(MatchCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Match oluştuğunda chat oluştur
        var createChatCommand = new CreateChatCommand(notification.UserId1, notification.UserId2);
        await _mediator.Send(createChatCommand, cancellationToken);
        
        // Notification gönder
        // ...
    }
}

// 3. Command Handler'da event fırlatma
public class LikeProfileCommandHandler : IRequestHandler<LikeProfileCommand, Unit>
{
    private readonly IMediator _mediator;
    
    public async Task<Unit> Handle(LikeProfileCommand request, CancellationToken cancellationToken)
    {
        // Like işlemi
        // ...
        
        // Match kontrolü
        if (isMutualLike)
        {
            // Event fırlat
            await _mediator.Publish(new MatchCreatedEvent 
            { 
                UserId1 = request.UserId, 
                UserId2 = request.TargetUserId 
            }, cancellationToken);
        }
        
        return Unit.Value;
    }
}
```

---

## 🔀 Dependency Flow (Bağımlılık Akışı)

### Dependency Inversion Principle (DIP)

**Temel Kural:** Üst katmanlar alt katmanlara değil, abstraction'lara (interface'lere) bağımlı olmalıdır.

### Örnek: User Repository

#### 1. Interface (Application Katmanında)

```csharp
// Application/Common/Interfaces/IUserRepository.cs
namespace BridgeApi.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User> GetByEmailAsync(Email email, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
    Task DeleteAsync(User user, CancellationToken cancellationToken);
}
```

**Neden Application'da?**
- Application katmanı Domain entity'lerini kullanır
- Application katmanı Infrastructure'ın nasıl implement edildiğini bilmez
- Application sadece interface'e bağımlıdır

#### 2. Implementation (Infrastructure Katmanında)

```csharp
// Infrastructure/BridgeApi.Persistence/Repositories/UserRepository.cs
namespace BridgeApi.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
    
    // Diğer metodlar...
}
```

**Neden Infrastructure'da?**
- Infrastructure, database (EF Core) detaylarını bilir
- Infrastructure, Application'ın interface'ini implement eder
- Database değişirse sadece Infrastructure değişir

#### 3. Dependency Injection (Program.cs)

```csharp
// Presentation/BridgeApi.API/Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

**Sonuç:**
- Application → IUserRepository interface'ine bağımlı
- Infrastructure → IUserRepository interface'ini implement eder
- Database değişirse → Sadece Infrastructure değişir, Application etkilenmez

### Dependency Flow Diyagramı

```
┌─────────────────────────────────────────┐
│         PRESENTATION (API)              │
│  Controllers → IMediator (MediatR)      │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         APPLICATION                      │
│  Commands/Queries → IUserRepository     │
│  (Interface'ler burada tanımlı)         │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│           DOMAIN                         │
│  Entities, Value Objects, Business Logic│
│  (Hiçbir şeye bağımlı değil)            │
└─────────────────────────────────────────┘
               ▲
               │
┌──────────────┴──────────────────────────┐
│         INFRASTRUCTURE                   │
│  UserRepository → IUserRepository        │
│  (Implementation burada)                 │
└──────────────────────────────────────────┘
```

---

## 💡 Örnek Senaryolar

### Senaryo 1: Kullanıcı Oluşturma

**Akış:**

1. **Client → API**
```
POST /api/users
{
  "email": "john@example.com",
  "name": "John Doe"
}
```

2. **Controller**
```csharp
[HttpPost]
public async Task<ActionResult<Guid>> CreateUser(CreateUserCommand command)
{
    var userId = await _mediator.Send(command);
    return Ok(userId);
}
```

3. **MediatR Pipeline**
```
CreateUserCommand 
  → LoggingBehavior (log başladı)
  → ValidationBehavior (email format kontrolü)
  → TransactionBehavior (transaction başlat)
  → CreateUserCommandHandler
```

4. **Command Handler**
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Domain entity oluştur
        var email = new Email(request.Email);
        var user = new User(email, request.Name);
        
        // Repository'ye kaydet
        await _userRepository.AddAsync(user, ct);
        
        return user.Id;
    }
}
```

5. **Repository (Infrastructure)**
```csharp
public async Task AddAsync(User user, CancellationToken cancellationToken)
{
    await _context.Users.AddAsync(user, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
}
```

6. **Pipeline Devam**
```
  → TransactionBehavior (commit)
  → LoggingBehavior (log bitti)
  → Response döner
```

### Senaryo 2: Feed Okuma

**Akış:**

1. **Client → API**
```
GET /api/feed?intentId=123&cursor=abc
```

2. **Controller**
```csharp
[HttpGet]
public async Task<ActionResult<FeedDto>> GetFeed([FromQuery] GetFeedQuery query)
{
    var feed = await _mediator.Send(query);
    return Ok(feed);
}
```

3. **Query Handler**
```csharp
public class GetFeedQueryHandler : IRequestHandler<GetFeedQuery, FeedDto>
{
    private readonly IFeedRepository _feedRepository;
    private readonly ICacheService _cacheService;
    
    public async Task<FeedDto> Handle(GetFeedQuery request, CancellationToken ct)
    {
        // Cache kontrolü
        var cacheKey = $"feed:{request.UserId}:{request.IntentId}:{request.Cursor}";
        var cachedFeed = await _cacheService.GetAsync<FeedDto>(cacheKey);
        
        if (cachedFeed != null)
            return cachedFeed;
        
        // Database'den okuma
        var feed = await _feedRepository.GetFeedAsync(request, ct);
        
        // Cache'e kaydet
        await _cacheService.SetAsync(cacheKey, feed, TimeSpan.FromMinutes(5));
        
        return feed;
    }
}
```

### Senaryo 3: Match Oluşturma (Event-Driven)

**Akış:**

1. **User A, User B'yi like eder**
```csharp
POST /api/profiles/{userId}/like
```

2. **LikeProfileCommandHandler**
```csharp
public async Task<Unit> Handle(LikeProfileCommand request, CancellationToken ct)
{
    // Like kaydet
    var like = new Like(request.UserId, request.TargetUserId);
    await _likeRepository.AddAsync(like, ct);
    
    // Mutual like kontrolü
    var isMutualLike = await _likeRepository.ExistsAsync(
        request.TargetUserId, 
        request.UserId, 
        ct);
    
    if (isMutualLike)
    {
        // Event fırlat
        await _mediator.Publish(new MatchCreatedEvent 
        { 
            UserId1 = request.UserId,
            UserId2 = request.TargetUserId 
        }, ct);
    }
    
    return Unit.Value;
}
```

3. **MatchCreatedEventHandler**
```csharp
public async Task Handle(MatchCreatedEvent notification, CancellationToken ct)
{
    // Match oluştur
    var match = new Match(notification.UserId1, notification.UserId2);
    await _matchRepository.AddAsync(match, ct);
    
    // Chat oluştur
    var createChatCommand = new CreateChatCommand(
        notification.UserId1, 
        notification.UserId2);
    await _mediator.Send(createChatCommand, ct);
    
    // Notification gönder (SignalR)
    await _notificationService.SendMatchNotification(
        notification.UserId1, 
        notification.UserId2);
}
```

---

## ❓ Sık Sorulan Sorular

### 1. Neden Bu Kadar Katman Var?

**Cevap:** Her katmanın sorumluluğu farklıdır:
- **Domain:** Business logic (en önemli, değişmemeli)
- **Application:** Use case'ler (ne yapılacağı)
- **Infrastructure:** Nasıl yapılacağı (database, external services)
- **Presentation:** HTTP, API endpoints

**Avantaj:** Database değişirse sadece Infrastructure değişir, business logic etkilenmez.

### 2. Her Command/Query İçin Ayrı Handler Yazmak Zor Değil Mi?

**Cevap:** İlk başta zor görünebilir ama:
- **Avantajlar:**
  - Her handler tek sorumluluğa sahip
  - Test yazmak çok kolay
  - Kod daha okunabilir
  - Cross-cutting concerns (logging, validation) otomatik

- **Örnek:** 10 endpoint için 10 handler = 10 küçük, test edilebilir sınıf

### 3. MediatR Olmadan Yapamaz mıyız?

**Cevap:** Yapabilirsiniz ama:
- **MediatR Olmadan:**
```csharp
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILoggingService _loggingService;
    // ... 10 tane daha service
    
    [HttpPost]
    public async Task<ActionResult> CreateUser(CreateUserRequest request)
    {
        // Logging
        // Validation
        // Business logic
        // Email gönderme
        // ...
    }
}
```

- **MediatR İle:**
```csharp
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<ActionResult> CreateUser(CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

**Sonuç:** MediatR ile controller çok daha temiz ve test edilebilir.

### 4. Repository Pattern Gerekli Mi?

**Cevap:** Bu projede **tam gerekli değil** ama **interface abstraction gerekli**.

- **EF Core DbContext zaten repository pattern'i sağlar**
- **Ama interface abstraction önemli** (test için, database değişikliği için)

**Yaklaşımımız:**
- Generic repository kullanmıyoruz
- Her aggregate için özel repository interface'i
- Implementation Infrastructure'da

### 5. DTO Neden Gerekli?

**Cevap:** 
- **Entity'ler Domain'de, Presentation'a gitmemeli**
- **DTO'lar:**
  - Sadece gerekli field'ları içerir
  - Sensitive data'yı gizler
  - API versioning için kullanılabilir

**Örnek:**
```csharp
// Domain Entity
public class User
{
    public Guid Id { get; set; }
    public Email Email { get; set; }
    public string PasswordHash { get; set; } // Sensitive!
    public DateTime CreatedAt { get; set; }
}

// DTO (API'ye dönen)
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    // PasswordHash yok!
}
```

### 6. Transaction Nasıl Yönetiliyor?

**Cevap:** TransactionBehavior ile otomatik.

```csharp
// Her command otomatik transaction içinde çalışır
public class TransactionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    // Command başlamadan transaction başlat
    // Command bitince commit
    // Hata olursa rollback
}
```

**Avantaj:** Her handler'da transaction yazmaya gerek yok.

### 7. Validation Nerede Yapılıyor?

**Cevap:** İki seviyede:

1. **FluentValidation (Application seviyesi)**
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

2. **Domain Validation (Business rules)**
```csharp
public class User
{
    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new DomainException("Email cannot be null");
        // ...
    }
}
```

### 8. Error Handling Nasıl Çalışıyor?

**Cevap:** Global exception handler middleware.

```csharp
public class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            // 400 Bad Request
        }
        catch (NotFoundException ex)
        {
            // 404 Not Found
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            // Log error
        }
    }
}
```

**Avantaj:** Her handler'da try-catch yazmaya gerek yok.

### 9. Test Nasıl Yazılır?

**Cevap:** Her katman bağımsız test edilebilir.

**Unit Test (Handler):**
```csharp
[Fact]
public async Task Handle_CreateUserCommand_ReturnsUserId()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var handler = new CreateUserCommandHandler(mockRepository.Object);
    var command = new CreateUserCommand("test@example.com", "Test");
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.NotEqual(Guid.Empty, result);
    mockRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

**Integration Test (API):**
```csharp
[Fact]
public async Task CreateUser_ReturnsOk()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreateUserCommand("test@example.com", "Test");
    
    // Act
    var response = await client.PostAsJsonAsync("/api/users", command);
    
    // Assert
    response.EnsureSuccessStatusCode();
}
```

### 10. Yeni Feature Nasıl Eklenir?

**Adımlar:**

1. **Domain Entity oluştur** (Domain katmanında)
2. **Repository interface oluştur** (Application/Common/Interfaces)
3. **Repository implementation oluştur** (Infrastructure/Persistence)
4. **Command/Query oluştur** (Application/Features)
5. **Handler yaz** (Application/Features)
6. **Controller endpoint ekle** (Presentation/API)
7. **Dependency Injection kaydet** (Program.cs)

**Örnek: Post Feature**

```
1. Domain/Entities/Post.cs
2. Application/Common/Interfaces/IPostRepository.cs
3. Infrastructure/Persistence/Repositories/PostRepository.cs
4. Application/Features/Posts/Commands/CreatePost/
5. Application/Features/Posts/Queries/GetPost/
6. Presentation/API/Controllers/PostsController.cs
7. Program.cs → services.AddScoped<IPostRepository, PostRepository>();
```

---

## 📚 Ek Kaynaklar

### Öğrenme Kaynakları

1. **Clean Architecture**
   - Robert C. Martin - Clean Architecture (Kitap)
   - [Microsoft Docs - Clean Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

2. **CQRS**
   - [Martin Fowler - CQRS](https://martinfowler.com/bliki/CQRS.html)
   - [Microsoft Docs - CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)

3. **MediatR**
   - [MediatR GitHub](https://github.com/jbogard/MediatR)
   - [MediatR Wiki](https://github.com/jbogard/MediatR/wiki)

### Proje İçi Dokümantasyon

- `ROADMAP.md` - 11 haftalık geliştirme planı
- Bu dokümantasyon - Mimari açıklamaları

---

## 🎯 Özet

### Temel Prensipler

1. **Clean Architecture:** Katmanlar arası bağımlılıklar tek yönlü
2. **CQRS:** Command (yazma) ve Query (okuma) ayrı
3. **MediatR:** Controller'lar handler'lara direkt değil, MediatR üzerinden erişir
4. **Dependency Inversion:** Üst katmanlar interface'lere bağımlı

### Katman Sorumlulukları

- **Domain:** Business logic, entities, value objects
- **Application:** Use cases, commands, queries, DTOs
- **Infrastructure:** Database, external services, implementations
- **Presentation:** API endpoints, HTTP concerns

### Çalışma Akışı

```
Client Request 
  → Controller 
  → MediatR 
  → Pipeline Behaviors 
  → Handler 
  → Repository 
  → Database
  → Response
```

---

**Sorularınız için:** Ekip lead'i ile iletişime geçin veya bu dokümantasyonu güncelleyin.

**Son Güncelleme:** 2026-02-09
