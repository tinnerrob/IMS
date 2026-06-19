using CommunityToolkit.Maui;
using IMS.Services;
using IMS.ViewModels;
using IMS.Views;
using Microsoft.Extensions.Logging;

namespace IMS;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<IFakeDataService, FakeDataService>();
        builder.Services.AddSingleton<IDataStoreService, DataStoreService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<InventoryViewModel>();
        builder.Services.AddTransient<SchedulingViewModel>();
        builder.Services.AddTransient<SmrViewModel>();
        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<LeasesViewModel>();
        builder.Services.AddTransient<ExceptionsViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();
        builder.Services.AddTransient<AdminViewModel>();

        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<InventoryPage>();
        builder.Services.AddTransient<SchedulingPage>();
        builder.Services.AddTransient<SmrPage>();
        builder.Services.AddTransient<CustomersPage>();
        builder.Services.AddTransient<LeasesPage>();
        builder.Services.AddTransient<ExceptionsPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<AdminPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Eagerly initialize fake data service to ensure data is available
        var fakeData = app.Services.GetRequiredService<IFakeDataService>();
        fakeData.Initialize();

        return app;
    }
}
