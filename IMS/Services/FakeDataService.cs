using IMS.Models;
using IMS.Models.Enums;

namespace IMS.Services;

public class FakeDataService : IFakeDataService
{
    private bool _initialized;
    private readonly object _lock = new();

    public List<Organization> Organizations { get; private set; } = new();
    public List<TenantUser> TenantUsers { get; private set; } = new();
    public List<AssetCategory> AssetCategories { get; private set; } = new();
    public List<ProductCatalog> ProductCatalog { get; private set; } = new();
    public List<SerializedAsset> SerializedAssets { get; private set; } = new();
    public List<BulkQuantityPool> BulkQuantityPools { get; private set; } = new();
    public List<Customer> Customers { get; private set; } = new();
    public List<Project> Projects { get; private set; } = new();
    public List<ScheduleAllocation> ScheduleAllocations { get; private set; } = new();
    public List<LeaseTransactionLedger> LeaseTransactions { get; private set; } = new();
    public List<SmrServiceTicket> SmrTickets { get; private set; } = new();
    public List<SmrLaborLineItem> SmrLaborLines { get; private set; } = new();
    public List<SmrPartsUsageLedger> SmrPartsUsage { get; private set; } = new();
    public List<AuditLogEntry> AuditLogs { get; private set; } = new();

    private readonly Random _rng = new(42); // deterministic seed

    public FakeDataService()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;
            _initialized = true;
        }

        CreateOrganizations();
        CreateTenantUsers();
        CreateAssetCategories();
        CreateProductCatalog();
        CreateSerializedAssets();
        CreateBulkPools();
        CreateCustomers();
        CreateProjects();
        CreateScheduleAllocations();
        CreateLeaseTransactions();
        CreateSmrTickets();
        CreateAuditLogs();
    }

    private void CreateOrganizations()
    {
        Organizations.Add(new Organization { OrgId = Guid.Parse("10000000-0000-0000-0000-000000000001"), OrgName = "Acme Cinematic Rentals", SubTier = SubTier.Enterprise });
        Organizations.Add(new Organization { OrgId = Guid.Parse("10000000-0000-0000-0000-000000000002"), OrgName = "StageLight Productions", SubTier = SubTier.Standard });
    }

    private void CreateTenantUsers()
    {
        var org1 = Organizations[0].OrgId;
        var org2 = Organizations[1].OrgId;

        TenantUsers.Add(new TenantUser { UserId = Guid.Parse("20000000-0000-0000-0000-000000000001"), OrgId = org1, UserType = UserType.System_Admin, Email = "admin@acme.com", Username = "admin", PasswordHash = "admin", DisplayName = "Sarah Chen", InternalHourlyRate = 75 });
        TenantUsers.Add(new TenantUser { UserId = Guid.Parse("20000000-0000-0000-0000-000000000002"), OrgId = org1, UserType = UserType.Service_Specialist, Email = "service@acme.com", Username = "service", PasswordHash = "service", DisplayName = "Mike Torres", InternalHourlyRate = 45 });
        TenantUsers.Add(new TenantUser { UserId = Guid.Parse("20000000-0000-0000-0000-000000000003"), OrgId = org1, UserType = UserType.Logistics_Desk, Email = "logistics@acme.com", Username = "logistics", PasswordHash = "logistics", DisplayName = "Jamie Wilson", InternalHourlyRate = 35 });
        TenantUsers.Add(new TenantUser { UserId = Guid.Parse("20000000-0000-0000-0000-000000000004"), OrgId = org1, UserType = UserType.Customer_Contact, Email = "customer@acme.com", Username = "customer", PasswordHash = "customer", DisplayName = "Alex Rivera" });
        TenantUsers.Add(new TenantUser { UserId = Guid.Parse("20000000-0000-0000-0000-000000000005"), OrgId = org1, UserType = UserType.Service_Specialist, Email = "tech2@acme.com", Username = "tech2", PasswordHash = "tech2", DisplayName = "Diana Park", InternalHourlyRate = 50 });

        TenantUsers.Add(new TenantUser { UserId = Guid.Parse("20000000-0000-0000-0000-000000000010"), OrgId = org2, UserType = UserType.System_Admin, Email = "admin@stagelight.com", Username = "sl_admin", PasswordHash = "sl_admin", DisplayName = "Tom Briggs", InternalHourlyRate = 70 });
        TenantUsers.Add(new TenantUser { UserId = Guid.Parse("20000000-0000-0000-0000-000000000011"), OrgId = org2, UserType = UserType.Logistics_Desk, Email = "logistics@stagelight.com", Username = "sl_logistics", PasswordHash = "sl_logistics", DisplayName = "Emma Frost", InternalHourlyRate = 30 });
    }

    private void CreateAssetCategories()
    {
        var org1 = Organizations[0].OrgId;

        // Level 0 - Root categories
        var cameras = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000001"), OrgId = org1, Name = "Cameras" };
        var lighting = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000002"), OrgId = org1, Name = "Lighting" };
        var audio = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000003"), OrgId = org1, Name = "Audio" };
        var grip = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000004"), OrgId = org1, Name = "Grip & Electric" };
        var support = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000005"), OrgId = org1, Name = "Support & Rigging" };

        // Level 1 - Camera subcategories
        var cinemaCam = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000010"), OrgId = org1, Name = "Cinema Cameras", ParentCategoryId = cameras.CategoryId };
        var dslrMirrorless = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000011"), OrgId = org1, Name = "DSLR & Mirrorless", ParentCategoryId = cameras.CategoryId };
        var lenses = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000012"), OrgId = org1, Name = "Lenses", ParentCategoryId = cameras.CategoryId };

        // Level 1 - Lighting subcategories
        var ledPanels = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000020"), OrgId = org1, Name = "LED Panels", ParentCategoryId = lighting.CategoryId };
        var fresnels = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000021"), OrgId = org1, Name = "Fresnels", ParentCategoryId = lighting.CategoryId };

        // Level 1 - Audio subcategories
        var wirelessMics = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000030"), OrgId = org1, Name = "Wireless Microphones", ParentCategoryId = audio.CategoryId };
        var mixers = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000031"), OrgId = org1, Name = "Audio Mixers", ParentCategoryId = audio.CategoryId };

        // Level 2 - Cinema Camera sub-brands
        var arri = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000040"), OrgId = org1, Name = "ARRI", ParentCategoryId = cinemaCam.CategoryId };
        var red = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000041"), OrgId = org1, Name = "RED", ParentCategoryId = cinemaCam.CategoryId };
        var sony = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000042"), OrgId = org1, Name = "Sony", ParentCategoryId = cinemaCam.CategoryId };

        // Level 2 - Lens types
        var primeLenses = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000050"), OrgId = org1, Name = "Prime Lenses", ParentCategoryId = lenses.CategoryId };
        var zoomLenses = new AssetCategory { CategoryId = Guid.Parse("30000000-0000-0000-0000-000000000051"), OrgId = org1, Name = "Zoom Lenses", ParentCategoryId = lenses.CategoryId };

        cameras.Children = new() { cinemaCam, dslrMirrorless, lenses };
        cinemaCam.Children = new() { arri, red, sony };
        lenses.Children = new() { primeLenses, zoomLenses };
        lighting.Children = new() { ledPanels, fresnels };
        audio.Children = new() { wirelessMics, mixers };

        AssetCategories.AddRange(new[] { cameras, lighting, audio, grip, support,
            cinemaCam, dslrMirrorless, lenses, ledPanels, fresnels, wirelessMics, mixers,
            arri, red, sony, primeLenses, zoomLenses });
    }

    private void CreateProductCatalog()
    {
        var org1 = Organizations[0].OrgId;
        var cat = AssetCategories.ToDictionary(c => c.Name, c => c.CategoryId);

        ProductCatalog.Add(new ProductCatalog { Sku = "ARRI-ALEXA-MINI", OrgId = org1, CategoryId = cat["ARRI"], Manufacturer = "ARRI", ModelName = "Alexa Mini LF", BaseCost = 65000, Specifications = "{\"sensor\":\"LF 4.5K\",\"dynamicRange\":\"14.5 stops\",\"weight\":\"2.3kg\"}" });
        ProductCatalog.Add(new ProductCatalog { Sku = "ARRI-ALEXA-35", OrgId = org1, CategoryId = cat["ARRI"], Manufacturer = "ARRI", ModelName = "Alexa 35", BaseCost = 85000, Specifications = "{\"sensor\":\"S35 4.6K\",\"dynamicRange\":\"17 stops\",\"weight\":\"2.8kg\"}" });
        ProductCatalog.Add(new ProductCatalog { Sku = "RED-KOMODO", OrgId = org1, CategoryId = cat["RED"], Manufacturer = "RED", ModelName = "Komodo 6K", BaseCost = 30000, Specifications = "{\"sensor\":\"S35 6K\",\"dynamicRange\":\"16 stops\",\"weight\":\"1.1kg\"}" });
        ProductCatalog.Add(new ProductCatalog { Sku = "RED-V-RAPTOR", OrgId = org1, CategoryId = cat["RED"], Manufacturer = "RED", ModelName = "V-Raptor 8K", BaseCost = 55000, Specifications = "{\"sensor\":\"FF 8K\",\"dynamicRange\":\"17 stops\",\"weight\":\"1.8kg\"}" });
        ProductCatalog.Add(new ProductCatalog { Sku = "SONY-VENICE-2", OrgId = org1, CategoryId = cat["Sony"], Manufacturer = "Sony", ModelName = "VENICE 2 8K", BaseCost = 62000, Specifications = "{\"sensor\":\"FF 8.6K\",\"dynamicRange\":\"16 stops\",\"weight\":\"3.2kg\"}" });
        ProductCatalog.Add(new ProductCatalog { Sku = "SONY-FX6", OrgId = org1, CategoryId = cat["Sony"], Manufacturer = "Sony", ModelName = "FX6", BaseCost = 12000, Specifications = "{\"sensor\":\"FF 4K\",\"dynamicRange\":\"15 stops\",\"weight\":\"0.9kg\"}" });
        ProductCatalog.Add(new ProductCatalog { Sku = "CANON-C70", OrgId = org1, CategoryId = cat["DSLR & Mirrorless"], Manufacturer = "Canon", ModelName = "EOS C70", BaseCost = 8500, Specifications = "{\"sensor\":\"S35 4K\",\"dynamicRange\":\"14 stops\",\"weight\":\"1.3kg\"}" });
        ProductCatalog.Add(new ProductCatalog { Sku = "SONY-A7S3", OrgId = org1, CategoryId = cat["DSLR & Mirrorless"], Manufacturer = "Sony", ModelName = "A7S III", BaseCost = 4500, Specifications = "{\"sensor\":\"FF 4K\",\"dynamicRange\":\"15 stops\",\"weight\":\"0.7kg\"}" });

        // Lenses
        ProductCatalog.Add(new ProductCatalog { Sku = "ARRI-MP-50", OrgId = org1, CategoryId = cat["Prime Lenses"], Manufacturer = "ARRI", ModelName = "Master Prime 50mm T1.3", BaseCost = 8500 });
        ProductCatalog.Add(new ProductCatalog { Sku = "ARRI-MP-35", OrgId = org1, CategoryId = cat["Prime Lenses"], Manufacturer = "ARRI", ModelName = "Master Prime 35mm T1.3", BaseCost = 8200 });
        ProductCatalog.Add(new ProductCatalog { Sku = "ZEISS-CP3-24", OrgId = org1, CategoryId = cat["Prime Lenses"], Manufacturer = "Zeiss", ModelName = "CP.3 24mm T2.1", BaseCost = 4500 });
        ProductCatalog.Add(new ProductCatalog { Sku = "CANON-CN7-17", OrgId = org1, CategoryId = cat["Zoom Lenses"], Manufacturer = "Canon", ModelName = "CN7x17 KAS S", BaseCost = 12000 });

        // Lighting
        ProductCatalog.Add(new ProductCatalog { Sku = "ARRI-S360C", OrgId = org1, CategoryId = cat["LED Panels"], Manufacturer = "ARRI", ModelName = "SkyPanel S360-C", BaseCost = 8500 });
        ProductCatalog.Add(new ProductCatalog { Sku = "ARRI-S60C", OrgId = org1, CategoryId = cat["LED Panels"], Manufacturer = "ARRI", ModelName = "SkyPanel S60-C", BaseCost = 3500 });
        ProductCatalog.Add(new ProductCatalog { Sku = "APUTURE-600D", OrgId = org1, CategoryId = cat["LED Panels"], Manufacturer = "Aputure", ModelName = "600D Pro", BaseCost = 1800 });
        ProductCatalog.Add(new ProductCatalog { Sku = "APUTURE-300X", OrgId = org1, CategoryId = cat["LED Panels"], Manufacturer = "Aputure", ModelName = "300X", BaseCost = 900 });
        ProductCatalog.Add(new ProductCatalog { Sku = "MOLE-2K-FRES", OrgId = org1, CategoryId = cat["Fresnels"], Manufacturer = "Mole-Richardson", ModelName = "2K Fresnel", BaseCost = 1200 });

        // Audio
        ProductCatalog.Add(new ProductCatalog { Sku = "SENNHEISER-EW500", OrgId = org1, CategoryId = cat["Wireless Microphones"], Manufacturer = "Sennheiser", ModelName = "EW 500 G4", BaseCost = 2500 });
        ProductCatalog.Add(new ProductCatalog { Sku = "SONY-UWP-D27", OrgId = org1, CategoryId = cat["Wireless Microphones"], Manufacturer = "Sony", ModelName = "UWP-D27", BaseCost = 1800 });
        ProductCatalog.Add(new ProductCatalog { Sku = "SOUNDDEV-633", OrgId = org1, CategoryId = cat["Audio Mixers"], Manufacturer = "Sound Devices", ModelName = "MixPre-6 II", BaseCost = 1200 });
        ProductCatalog.Add(new ProductCatalog { Sku = "ZOOM-F8N", OrgId = org1, CategoryId = cat["Audio Mixers"], Manufacturer = "Zoom", ModelName = "F8n Pro", BaseCost = 1500 });
    }

    private void CreateSerializedAssets()
    {
        var org1 = Organizations[0].OrgId;
        var products = ProductCatalog.ToDictionary(p => p.Sku, p => p);

        // ARRI Alexa Mini LFs
        for (int i = 1; i <= 4; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0000-{i:D12}"),
                OrgId = org1,
                Sku = "ARRI-ALEXA-MINI",
                SerialNumber = $"ALM-{2024}-{1000 + i}",
                BarcodeRfid = $"RFID-ARRI-MINI-{i}",
                CurrentStatus = i <= 3 ? AssetStatus.Available : AssetStatus.In_Use,
                PurchaseDate = new DateTime(2024, 1, 15).AddDays(i * 30),
                CurrentMeterReading = _rng.Next(100, 800)
            });
        }

        // ARRI Alexa 35s
        for (int i = 1; i <= 2; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0001-{i:D12}"),
                OrgId = org1,
                Sku = "ARRI-ALEXA-35",
                SerialNumber = $"A35-{2024}-{2000 + i}",
                BarcodeRfid = $"RFID-ARRI-35-{i}",
                CurrentStatus = i == 1 ? AssetStatus.Available : AssetStatus.In_Repair,
                PurchaseDate = new DateTime(2024, 6, 1).AddDays(i * 15),
                CurrentMeterReading = _rng.Next(50, 300)
            });
        }

        // RED Komodos
        for (int i = 1; i <= 3; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0002-{i:D12}"),
                OrgId = org1,
                Sku = "RED-KOMODO",
                SerialNumber = $"KMD-{2024}-{3000 + i}",
                BarcodeRfid = $"RFID-RED-KM-{i}",
                CurrentStatus = i == 1 ? AssetStatus.Available : i == 2 ? AssetStatus.In_Use : AssetStatus.Damaged_Cosmetic,
                PurchaseDate = new DateTime(2024, 3, 10).AddDays(i * 20),
                CurrentMeterReading = _rng.Next(200, 600)
            });
        }

        // RED V-Raptors
        for (int i = 1; i <= 2; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0003-{i:D12}"),
                OrgId = org1,
                Sku = "RED-V-RAPTOR",
                SerialNumber = $"VRP-{2024}-{4000 + i}",
                BarcodeRfid = $"RFID-RED-VR-{i}",
                CurrentStatus = AssetStatus.Available,
                PurchaseDate = new DateTime(2024, 9, 1).AddDays(i * 10),
                CurrentMeterReading = _rng.Next(10, 150)
            });
        }

        // Sony VENICE 2
        for (int i = 1; i <= 2; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0004-{i:D12}"),
                OrgId = org1,
                Sku = "SONY-VENICE-2",
                SerialNumber = $"VNC-{2024}-{5000 + i}",
                BarcodeRfid = $"RFID-SONY-V2-{i}",
                CurrentStatus = i == 1 ? AssetStatus.Available : AssetStatus.In_Use,
                PurchaseDate = new DateTime(2024, 4, 20).AddDays(i * 25),
                CurrentMeterReading = _rng.Next(80, 400)
            });
        }

        // Sony FX6
        for (int i = 1; i <= 3; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0005-{i:D12}"),
                OrgId = org1,
                Sku = "SONY-FX6",
                SerialNumber = $"FX6-{2024}-{6000 + i}",
                BarcodeRfid = $"RFID-SONY-FX6-{i}",
                CurrentStatus = AssetStatus.Available,
                PurchaseDate = new DateTime(2024, 2, 5).AddDays(i * 15),
                CurrentMeterReading = _rng.Next(150, 500)
            });
        }

        // Canon C70
        for (int i = 1; i <= 2; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0006-{i:D12}"),
                OrgId = org1,
                Sku = "CANON-C70",
                SerialNumber = $"C70-{2024}-{7000 + i}",
                BarcodeRfid = $"RFID-CANON-C70-{i}",
                CurrentStatus = AssetStatus.Available,
                PurchaseDate = new DateTime(2024, 5, 15).AddDays(i * 10),
                CurrentMeterReading = _rng.Next(50, 200)
            });
        }

        // Lenses
        for (int i = 1; i <= 3; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0010-{i:D12}"),
                OrgId = org1,
                Sku = "ARRI-MP-50",
                SerialNumber = $"MP50-{2024}-{8000 + i}",
                BarcodeRfid = $"RFID-LENS-MP50-{i}",
                CurrentStatus = AssetStatus.Available,
                PurchaseDate = new DateTime(2024, 1, 20).AddDays(i * 15)
            });
        }
        for (int i = 1; i <= 2; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0011-{i:D12}"),
                OrgId = org1,
                Sku = "ARRI-MP-35",
                SerialNumber = $"MP35-{2024}-{9000 + i}",
                BarcodeRfid = $"RFID-LENS-MP35-{i}",
                CurrentStatus = AssetStatus.Available,
                PurchaseDate = new DateTime(2024, 2, 10).AddDays(i * 20)
            });
        }

        // Lighting
        for (int i = 1; i <= 3; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0020-{i:D12}"),
                OrgId = org1,
                Sku = "ARRI-S360C",
                SerialNumber = $"S360-{2024}-{10000 + i}",
                BarcodeRfid = $"RFID-LIGHT-S360-{i}",
                CurrentStatus = i == 1 ? AssetStatus.Available : AssetStatus.In_Use,
                PurchaseDate = new DateTime(2024, 3, 1).AddDays(i * 10)
            });
        }
        for (int i = 1; i <= 5; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0021-{i:D12}"),
                OrgId = org1,
                Sku = "ARRI-S60C",
                SerialNumber = $"S60-{2024}-{11000 + i}",
                BarcodeRfid = $"RFID-LIGHT-S60-{i}",
                CurrentStatus = AssetStatus.Available,
                PurchaseDate = new DateTime(2024, 4, 1).AddDays(i * 5)
            });
        }

        // Audio
        for (int i = 1; i <= 4; i++)
        {
            SerializedAssets.Add(new SerializedAsset
            {
                AssetId = Guid.Parse($"40000000-0000-0000-0030-{i:D12}"),
                OrgId = org1,
                Sku = "SENNHEISER-EW500",
                SerialNumber = $"EW500-{2024}-{12000 + i}",
                BarcodeRfid = $"RFID-AUDIO-EW500-{i}",
                CurrentStatus = AssetStatus.Available,
                PurchaseDate = new DateTime(2024, 1, 5).AddDays(i * 20)
            });
        }
    }

    private void CreateBulkPools()
    {
        var org1 = Organizations[0].OrgId;

        BulkQuantityPools.Add(new BulkQuantityPool { PoolId = Guid.Parse("50000000-0000-0000-0000-000000000001"), OrgId = org1, Sku = "APUTURE-600D", TotalQuantityOwned = 15, MinSafetyStock = 3 });
        BulkQuantityPools.Add(new BulkQuantityPool { PoolId = Guid.Parse("50000000-0000-0000-0000-000000000002"), OrgId = org1, Sku = "APUTURE-300X", TotalQuantityOwned = 25, MinSafetyStock = 5 });
        BulkQuantityPools.Add(new BulkQuantityPool { PoolId = Guid.Parse("50000000-0000-0000-0000-000000000003"), OrgId = org1, Sku = "MOLE-2K-FRES", TotalQuantityOwned = 10, MinSafetyStock = 2 });
        BulkQuantityPools.Add(new BulkQuantityPool { PoolId = Guid.Parse("50000000-0000-0000-0000-000000000004"), OrgId = org1, Sku = "SONY-UWP-D27", TotalQuantityOwned = 20, MinSafetyStock = 4 });
        BulkQuantityPools.Add(new BulkQuantityPool { PoolId = Guid.Parse("50000000-0000-0000-0000-000000000005"), OrgId = org1, Sku = "ZEISS-CP3-24", TotalQuantityOwned = 8, MinSafetyStock = 2 });
    }

    private void CreateCustomers()
    {
        var org1 = Organizations[0].OrgId;

        Customers.Add(new Customer { CustomerId = Guid.Parse("60000000-0000-0000-0000-000000000001"), OrgId = org1, AccountName = "Warner Bros. Pictures", CreditStatus = CreditStatus.Approved, ContactEmail = "productions@wb.com", Phone = "555-0101" });
        Customers.Add(new Customer { CustomerId = Guid.Parse("60000000-0000-0000-0000-000000000002"), OrgId = org1, AccountName = "Netflix Original Content", CreditStatus = CreditStatus.Approved, ContactEmail = "productions@netflix.com", Phone = "555-0102" });
        Customers.Add(new Customer { CustomerId = Guid.Parse("60000000-0000-0000-0000-000000000003"), OrgId = org1, AccountName = "HBO Max Productions", CreditStatus = CreditStatus.Approved, ContactEmail = "rentals@hbo.com", Phone = "555-0103" });
        Customers.Add(new Customer { CustomerId = Guid.Parse("60000000-0000-0000-0000-000000000004"), OrgId = org1, AccountName = "Indie Film Collective", CreditStatus = CreditStatus.Cash_Only, ContactEmail = "info@indiefilm.com", Phone = "555-0104" });
        Customers.Add(new Customer { CustomerId = Guid.Parse("60000000-0000-0000-0000-000000000005"), OrgId = org1, AccountName = "Event Horizon Studios", CreditStatus = CreditStatus.Hold, ContactEmail = "events@ehs.com", Phone = "555-0105" });
        Customers.Add(new Customer { CustomerId = Guid.Parse("60000000-0000-0000-0000-000000000006"), OrgId = org1, AccountName = "Pinewood Productions", CreditStatus = CreditStatus.Approved, ContactEmail = "rentals@pinewood.com", Phone = "555-0106" });
    }

    private void CreateProjects()
    {
        var customers = Customers.ToDictionary(c => c.AccountName, c => c.CustomerId);

        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000001"), CustomerId = customers["Warner Bros. Pictures"], ProjectName = "Summer Blockbuster 2025", StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2025, 9, 30) });
        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000002"), CustomerId = customers["Netflix Original Content"], ProjectName = "Netflix Drama Series S3", StartDate = new DateTime(2025, 7, 15), EndDate = new DateTime(2025, 12, 15) });
        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000003"), CustomerId = customers["HBO Max Productions"], ProjectName = "HBO Documentary Feature", StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2025, 10, 15) });
        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000004"), CustomerId = customers["Indie Film Collective"], ProjectName = "Indie Film - Sundance Entry", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 11, 30) });
        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000005"), CustomerId = customers["Pinewood Productions"], ProjectName = "Pinewood Commercial Shoot", StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 15) });
        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000006"), CustomerId = customers["Warner Bros. Pictures"], ProjectName = "Warner Bros. Holiday Campaign", StartDate = new DateTime(2025, 11, 1), EndDate = new DateTime(2025, 12, 31) });
        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000007"), CustomerId = customers["Netflix Original Content"], ProjectName = "Netflix Reality Show Pilot", StartDate = new DateTime(2025, 4, 1), EndDate = new DateTime(2025, 4, 30) });
        Projects.Add(new Project { ProjectId = Guid.Parse("70000000-0000-0000-0000-000000000008"), CustomerId = customers["HBO Max Productions"], ProjectName = "HBO Late Night Set Build", StartDate = new DateTime(2025, 10, 1), EndDate = new DateTime(2025, 11, 15) });
    }

    private void CreateScheduleAllocations()
    {
        var projects = Projects.ToDictionary(p => p.ProjectName, p => p.ProjectId);
        var categories = AssetCategories.ToDictionary(c => c.Name, c => c.CategoryId);
        var assets = SerializedAssets.ToDictionary(a => a.SerialNumber, a => a.AssetId);

        // Project 1: Summer Blockbuster - needs ARRI Alexa Minis + lenses
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000001"), ProjectId = projects["Summer Blockbuster 2025"], AllocationTier = AllocationTier.Hard_Locked, CategoryId = categories["ARRI"], StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2025, 9, 30) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000002"), ProjectId = projects["Summer Blockbuster 2025"], AllocationTier = AllocationTier.Hard_Locked, SerializedAssetId = assets["ALM-2024-1001"], StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2025, 9, 30) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000003"), ProjectId = projects["Summer Blockbuster 2025"], AllocationTier = AllocationTier.Hard_Locked, SerializedAssetId = assets["ALM-2024-1002"], StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2025, 9, 30) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000004"), ProjectId = projects["Summer Blockbuster 2025"], AllocationTier = AllocationTier.Hard_Locked, CategoryId = categories["Prime Lenses"], StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2025, 9, 30) });

        // Project 2: Netflix Drama - needs Sony VENICE + audio
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000005"), ProjectId = projects["Netflix Drama Series S3"], AllocationTier = AllocationTier.Hard_Locked, SerializedAssetId = assets["VNC-2024-5001"], StartDate = new DateTime(2025, 7, 15), EndDate = new DateTime(2025, 12, 15) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000006"), ProjectId = projects["Netflix Drama Series S3"], AllocationTier = AllocationTier.Soft_Hold, CategoryId = categories["Wireless Microphones"], StartDate = new DateTime(2025, 7, 15), EndDate = new DateTime(2025, 12, 15) });

        // Project 3: HBO Documentary - needs RED Komodo + lighting
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000007"), ProjectId = projects["HBO Documentary Feature"], AllocationTier = AllocationTier.Hard_Locked, SerializedAssetId = assets["KMD-2024-3001"], StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2025, 10, 15) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000008"), ProjectId = projects["HBO Documentary Feature"], AllocationTier = AllocationTier.Soft_Hold, CategoryId = categories["LED Panels"], StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2025, 10, 15) });

        // Project 4: Indie Film - needs Sony FX6 + audio
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000009"), ProjectId = projects["Indie Film - Sundance Entry"], AllocationTier = AllocationTier.Soft_Hold, CategoryId = categories["Sony"], StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 11, 30) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000010"), ProjectId = projects["Indie Film - Sundance Entry"], AllocationTier = AllocationTier.Soft_Hold, CategoryId = categories["Audio Mixers"], StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 11, 30) });

        // Project 5: Pinewood Commercial - short shoot
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000011"), ProjectId = projects["Pinewood Commercial Shoot"], AllocationTier = AllocationTier.Hard_Locked, SerializedAssetId = assets["FX6-2024-6001"], StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 15) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000012"), ProjectId = projects["Pinewood Commercial Shoot"], AllocationTier = AllocationTier.Hard_Locked, CategoryId = categories["LED Panels"], BulkQuantity = 4, StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 5, 15) });

        // Project 6: Warner Bros Holiday Campaign
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000013"), ProjectId = projects["Warner Bros. Holiday Campaign"], AllocationTier = AllocationTier.Soft_Hold, CategoryId = categories["Cinema Cameras"], StartDate = new DateTime(2025, 11, 1), EndDate = new DateTime(2025, 12, 31) });

        // Project 7: Netflix Reality Show Pilot (past project)
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000014"), ProjectId = projects["Netflix Reality Show Pilot"], AllocationTier = AllocationTier.Hard_Locked, SerializedAssetId = assets["C70-2024-7001"], StartDate = new DateTime(2025, 4, 1), EndDate = new DateTime(2025, 4, 30) });
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000015"), ProjectId = projects["Netflix Reality Show Pilot"], AllocationTier = AllocationTier.Hard_Locked, CategoryId = categories["Wireless Microphones"], BulkQuantity = 6, StartDate = new DateTime(2025, 4, 1), EndDate = new DateTime(2025, 4, 30) });

        // Project 8: HBO Late Night Set Build
        ScheduleAllocations.Add(new ScheduleAllocation { AllocationId = Guid.Parse("80000000-0000-0000-0000-000000000016"), ProjectId = projects["HBO Late Night Set Build"], AllocationTier = AllocationTier.Soft_Hold, CategoryId = categories["LED Panels"], BulkQuantity = 8, StartDate = new DateTime(2025, 10, 1), EndDate = new DateTime(2025, 11, 15) });
    }

    private void CreateLeaseTransactions()
    {
        var projects = Projects.ToDictionary(p => p.ProjectName, p => p.ProjectId);
        var assets = SerializedAssets.ToDictionary(a => a.SerialNumber, a => a.AssetId);
        var users = TenantUsers.ToDictionary(u => u.Username!, u => u.UserId);

        // Past completed lease - Netflix Reality Show
        LeaseTransactions.Add(new LeaseTransactionLedger
        {
            TransactionId = Guid.Parse("90000000-0000-0000-0000-000000000001"),
            ProjectId = projects["Netflix Reality Show Pilot"],
            AssetId = assets["C70-2024-7001"],
            OutboundTimestamp = new DateTime(2025, 4, 1, 8, 0, 0, DateTimeKind.Utc),
            DispatchedByUserId = users["logistics"],
            ActualReturnTimestamp = new DateTime(2025, 4, 30, 17, 0, 0, DateTimeKind.Utc),
            ReceivedByUserId = users["logistics"],
            ReturnCondition = "Pass/Clear"
        });

        // Active lease - Summer Blockbuster (partial deployment)
        LeaseTransactions.Add(new LeaseTransactionLedger
        {
            TransactionId = Guid.Parse("90000000-0000-0000-0000-000000000002"),
            ProjectId = projects["Summer Blockbuster 2025"],
            AssetId = assets["ALM-2024-1001"],
            OutboundTimestamp = new DateTime(2025, 6, 1, 7, 0, 0, DateTimeKind.Utc),
            DispatchedByUserId = users["logistics"]
        });
        LeaseTransactions.Add(new LeaseTransactionLedger
        {
            TransactionId = Guid.Parse("90000000-0000-0000-0000-000000000003"),
            ProjectId = projects["Summer Blockbuster 2025"],
            AssetId = assets["ALM-2024-1002"],
            OutboundTimestamp = new DateTime(2025, 6, 1, 7, 0, 0, DateTimeKind.Utc),
            DispatchedByUserId = users["logistics"]
        });

        // Active lease - Netflix Drama
        LeaseTransactions.Add(new LeaseTransactionLedger
        {
            TransactionId = Guid.Parse("90000000-0000-0000-0000-000000000004"),
            ProjectId = projects["Netflix Drama Series S3"],
            AssetId = assets["VNC-2024-5001"],
            OutboundTimestamp = new DateTime(2025, 7, 15, 9, 0, 0, DateTimeKind.Utc),
            DispatchedByUserId = users["logistics"]
        });

        // Completed lease - Pinewood Commercial
        LeaseTransactions.Add(new LeaseTransactionLedger
        {
            TransactionId = Guid.Parse("90000000-0000-0000-0000-000000000005"),
            ProjectId = projects["Pinewood Commercial Shoot"],
            AssetId = assets["FX6-2024-6001"],
            OutboundTimestamp = new DateTime(2025, 5, 1, 8, 30, 0, DateTimeKind.Utc),
            DispatchedByUserId = users["logistics"],
            ActualReturnTimestamp = new DateTime(2025, 5, 15, 16, 0, 0, DateTimeKind.Utc),
            ReceivedByUserId = users["logistics"],
            ReturnCondition = "Pass/Clear"
        });

        // Active lease - HBO Documentary
        LeaseTransactions.Add(new LeaseTransactionLedger
        {
            TransactionId = Guid.Parse("90000000-0000-0000-0000-000000000006"),
            ProjectId = projects["HBO Documentary Feature"],
            AssetId = assets["KMD-2024-3001"],
            OutboundTimestamp = new DateTime(2025, 8, 1, 6, 0, 0, DateTimeKind.Utc),
            DispatchedByUserId = users["logistics"]
        });
    }

    private void CreateSmrTickets()
    {
        var assets = SerializedAssets.ToDictionary(a => a.SerialNumber, a => a.AssetId);
        var users = TenantUsers.ToDictionary(u => u.Username!, u => u.UserId);

        // Open repair ticket for Alexa 35 that's In_Repair
        SmrTickets.Add(new SmrServiceTicket
        {
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000001"),
            AssetId = assets["A35-2024-2002"],
            AssignedTechnicianId = users["service"],
            TicketType = TicketType.Corrective_Repair,
            TicketStatus = TicketStatus.In_Progress,
            CurrentMeterReading = 275,
            CreatedAt = new DateTime(2025, 7, 10, 14, 0, 0, DateTimeKind.Utc),
            Description = "Sensor cleaning required - dust spots visible at T8. Also replacing fan assembly."
        });

        // PM ticket for Alexa Mini
        SmrTickets.Add(new SmrServiceTicket
        {
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000002"),
            AssetId = assets["ALM-2024-1003"],
            AssignedTechnicianId = users["service"],
            TicketType = TicketType.Preventive_Maintenance,
            TicketStatus = TicketStatus.Awaiting_Parts,
            CurrentMeterReading = 720,
            CreatedAt = new DateTime(2025, 7, 15, 9, 0, 0, DateTimeKind.Utc),
            Description = "500-hour PM service: firmware update, sensor cleaning, lens mount calibration."
        });

        // Completed repair ticket
        SmrTickets.Add(new SmrServiceTicket
        {
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000003"),
            AssetId = assets["KMD-2024-3003"],
            AssignedTechnicianId = users["tech2"],
            TicketType = TicketType.Corrective_Repair,
            TicketStatus = TicketStatus.Resolved,
            CurrentMeterReading = 580,
            CreatedAt = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc),
            ResolvedAt = new DateTime(2025, 6, 25, 16, 0, 0, DateTimeKind.Utc),
            Description = "HDMI port replacement - physical damage from cable strain."
        });

        // Compliance calibration ticket
        SmrTickets.Add(new SmrServiceTicket
        {
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000004"),
            AssetId = assets["VNC-2024-5002"],
            AssignedTechnicianId = users["tech2"],
            TicketType = TicketType.Compliance_Calibration,
            TicketStatus = TicketStatus.Testing_Verification,
            CurrentMeterReading = 380,
            CreatedAt = new DateTime(2025, 7, 1, 8, 0, 0, DateTimeKind.Utc),
            Description = "Annual color calibration and sensor alignment per manufacturer spec."
        });

        // PM ticket for S360 light
        SmrTickets.Add(new SmrServiceTicket
        {
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000005"),
            AssetId = assets["S360-2024-10002"],
            AssignedTechnicianId = users["service"],
            TicketType = TicketType.Preventive_Maintenance,
            TicketStatus = TicketStatus.In_Progress,
            CreatedAt = new DateTime(2025, 7, 20, 11, 0, 0, DateTimeKind.Utc),
            Description = "Firmware update and DMX port testing."
        });

        // Labor line items for ticket 1 (In Progress)
        SmrLaborLines.Add(new SmrLaborLineItem
        {
            LaborLineId = Guid.Parse("B0000000-0000-0000-0000-000000000001"),
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000001"),
            TechnicianId = users["service"],
            HoursSpent = 2.5m,
            CalculatedBurdenCost = 2.5m * 45m // $112.50
        });

        // Labor line items for ticket 3 (Resolved)
        SmrLaborLines.Add(new SmrLaborLineItem
        {
            LaborLineId = Guid.Parse("B0000000-0000-0000-0000-000000000002"),
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000003"),
            TechnicianId = users["tech2"],
            HoursSpent = 4.0m,
            CalculatedBurdenCost = 4.0m * 50m // $200.00
        });

        // Parts usage for ticket 1
        SmrPartsUsage.Add(new SmrPartsUsageLedger
        {
            PartsLineId = Guid.Parse("C0000000-0000-0000-0000-000000000001"),
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000001"),
            PartSkuOrId = "FAN-ARRI-A35",
            QuantityConsumed = 1,
            UnitCostAtConsumption = 85.00m
        });
        SmrPartsUsage.Add(new SmrPartsUsageLedger
        {
            PartsLineId = Guid.Parse("C0000000-0000-0000-0000-000000000002"),
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000001"),
            PartSkuOrId = "SENSOR-CLEAN-KIT",
            QuantityConsumed = 1,
            UnitCostAtConsumption = 45.00m
        });

        // Parts usage for ticket 3
        SmrPartsUsage.Add(new SmrPartsUsageLedger
        {
            PartsLineId = Guid.Parse("C0000000-0000-0000-0000-000000000003"),
            TicketId = Guid.Parse("A0000000-0000-0000-0000-000000000003"),
            PartSkuOrId = "HDMI-PORT-RED-KM",
            QuantityConsumed = 1,
            UnitCostAtConsumption = 120.00m
        });
    }

    private void CreateAuditLogs()
    {
        var users = TenantUsers.ToDictionary(u => u.Username!, u => u.UserId);
        var org1 = Organizations[0].OrgId;

        AuditLogs.Add(new AuditLogEntry { LogId = Guid.Parse("D0000000-0000-0000-0000-000000000001"), OrgId = org1, UserId = users["admin"], Action = "LOGIN", EntityType = "Session", EntityId = "N/A", Details = "User logged in", Timestamp = DateTime.UtcNow.AddHours(-2) });
        AuditLogs.Add(new AuditLogEntry { LogId = Guid.Parse("D0000000-0000-0000-0000-000000000002"), OrgId = org1, UserId = users["logistics"], Action = "CHECKOUT", EntityType = "SerializedAsset", EntityId = "40000000-0000-0000-0000-000000000001", Details = "Asset deployed to Summer Blockbuster 2025", Timestamp = DateTime.UtcNow.AddDays(-5) });
        AuditLogs.Add(new AuditLogEntry { LogId = Guid.Parse("D0000000-0000-0000-0000-000000000003"), OrgId = org1, UserId = users["service"], Action = "TICKET_CREATED", EntityType = "SmrServiceTicket", EntityId = "A0000000-0000-0000-0000-000000000001", Details = "Corrective repair ticket opened for A35-2024-2002", Timestamp = DateTime.UtcNow.AddDays(-3) });
        AuditLogs.Add(new AuditLogEntry { LogId = Guid.Parse("D0000000-0000-0000-0000-000000000004"), OrgId = org1, UserId = users["logistics"], Action = "RETURN", EntityType = "SerializedAsset", EntityId = "40000000-0000-0000-0005-000000000001", Details = "Asset returned from Pinewood Commercial Shoot - Pass/Clear", Timestamp = DateTime.UtcNow.AddDays(-10) });
        AuditLogs.Add(new AuditLogEntry { LogId = Guid.Parse("D0000000-0000-0000-0000-000000000005"), OrgId = org1, UserId = users["admin"], Action = "USER_CREATED", EntityType = "TenantUser", EntityId = "20000000-0000-0000-0000-000000000005", Details = "New service specialist added: Diana Park", Timestamp = DateTime.UtcNow.AddDays(-30) });
    }
}
