# Warehouse Zone Management

Модуль управління складськими зонами — ASP.NET Core Web API (.NET 10) за Clean Architecture.

## Технології

- ASP.NET Core Web API (.NET 10)
- EF Core 10 + MS SQL Server
- CQRS через MediatR
- FluentValidation
- AutoMapper
- xUnit + EF Core InMemory для тестів

## Структура

```text
WarehouseZoneTask
├── API            — контролери, конфігурація хоста
├── Application    — CQRS-хендлери (MediatR), DTO, валідатори
├── Domain         — сутності, enum'и
└── Infrastructure — DbContext, EF Core конфігурація, міграції
```

## Як запустити

1. Налаштуй рядок підключення у `API/appsettings.Development.json` (або через `dotnet user-secrets`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=WarehouseZoneManagement;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
  }
}
```

2. Застосувати міграції (також виконується автоматично при старті `API`, але можна й вручну):

```bash
cd API
dotnet ef database update
```

3. Запустити API:

```bash
dotnet run --project API
```

При першому старті база автоматично заповнюється демо-даними (`DbInitializer`).

4. Swagger доступний за `/swagger` у Development-режимі.

## Тести

```bash
cd Tests
dotnet test
```

Тести використовують EF Core InMemory-провайдер (ізольована база на кожен тест) та реальний AutoMapper-профіль проєкту — без моків бізнес-логіки.

## Архітектурні рішення

- **Без репозиторіїв/UoW поверх EF Core.** MediatR-хендлери звертаються до `AppDbContext` напряму — для проєкту такого розміру окремий шар репозиторіїв був би зайвою абстракцією без практичної користі.
- **`IsOccupied` (слот) і `Status` (палет) не зберігаються в БД** — обчислюються в DTO/мапінгу на основі наявності зв'язаного `Pallet`, як і вказано в ТЗ.
- **Код слоту генерується як `{ZoneCode}-S{NNN}`**, нумерація продовжується від максимального існуючого індексу в зоні (а не з нуля), щоб bulk-create можна було викликати кілька разів без колізій кодів.
- **Унікальність коду зони — в межах складу**, а не глобально (`HasIndex(z => new { z.WarehouseId, z.Code }).IsUnique()`), згідно з вимогою ТЗ.
- **Усі `DateTime` зберігаються в UTC** через `ValueConverter`, що застосовується глобально до всіх `DateTime`-властивостей у `OnModelCreating`.
- **Валідація через `FluentValidation` + `MediatR`-pipeline** (`ValidationBehavior`) — невалідний запит ніколи не доходить до хендлера, виняток `ValidationException` мапиться в `400 Bad Request` на рівні API.