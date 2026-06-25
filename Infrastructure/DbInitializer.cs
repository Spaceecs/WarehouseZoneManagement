using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class DbInitializer
{
    public static async Task SeedData(AppDbContext context)
    {
        if (await context.Warehouses.AnyAsync())
            return;

        var warehouses = new List<Warehouse>
        {
            new()
            {
                Code = "WH-KYIV-01",
                Name = "Головний Київський Склад",
                Address = "м. Київ, вул. Польова, 21"
            },
            new()
            {
                Code = "WH-KHM-02",
                Name = "Регіональний Хмельницький Склад",
                Address = "м. Хмельницький, вул. Заводська, 4"
            }
        };

        var zones = new List<WarehouseZone>
        {
            new()
            {
                Warehouse = warehouses[0],
                Code = "ZONE-A",
                Name = "Зона приймання А",
                ZoneType = ZoneType.Receiving,
                Description = "Основна зона для розвантаження транспорту"
            },
            new()
            {
                Warehouse = warehouses[0],
                Code = "ZONE-B",
                Name = "Стелажна зона зберігання B",
                ZoneType = ZoneType.Storage,
                Description = "Сухий склад для довготривалого зберігання"
            },
            new()
            {
                Warehouse = warehouses[0],
                Code = "ZONE-C",
                Name = "Холодильна камера C",
                ZoneType = ZoneType.Cold,
                Description = "Температурний режим від +2 до +8 градусів"
            },
            new()
            {
                Warehouse = warehouses[1],
                Code = "ZONE-A",
                Name = "Загальна зона зберігання",
                ZoneType = ZoneType.Storage,
                Description = "Універсальна зона Хмельницького філіалу"
            }
        };

        var slots = new List<ZoneSlot>
        {
            new() { Zone = zones[1], Code = "B-01-01" },
            new() { Zone = zones[1], Code = "B-01-02" },
            new() { Zone = zones[1], Code = "B-02-01" },
            new() { Zone = zones[2], Code = "C-01-01" },
            new() { Zone = zones[2], Code = "C-01-02" }
        };

        var pallets = new List<Pallet>
        {
            new()
            {
                Code = "PLT-000192",
                Description = "Електроніка: Ноутбуки ASUS",
                ZoneSlot = slots[0]
            },
            new()
            {
                Code = "PLT-000193",
                Description = "Побутова техніка: Чайники",
                ZoneSlot = slots[1]
            },
            new()
            {
                Code = "PLT-000540",
                Description = "Продукти: Напої енергетичні",
                ZoneSlot = slots[3]
            },
            new()
            {
                Code = "PLT-UNPLACED",
                Description = "Новий товар на прийманні",
                ZoneSlot = null
            }
        };

        await context.Warehouses.AddRangeAsync(warehouses);
        await context.WarehouseZones.AddRangeAsync(zones);
        await context.ZoneSlots.AddRangeAsync(slots);
        await context.Pallets.AddRangeAsync(pallets);

        await context.SaveChangesAsync();
    }
}