using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Offline.Repositories;
using UserManagement.Mobile.Core.Offline.Sync;
using UserManagement.Mobile.Core.Services.Implementations;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels;
using UserManagement.Mobile.Handlers;
using UserManagement.Mobile.Views;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesignIcons");
            });

        // --- Configuration from appsettings.json ---
        var config = new ConfigurationBuilder()
            .AddJsonStream(FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult())
            .Build();

        builder.Configuration.AddConfiguration(config);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var baseAddress = config["ApiBaseAddress"]
            ?? throw new InvalidOperationException("ApiBaseAddress not configured in appsettings.json");

        // --- Platform services ---
        builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
        builder.Services.AddSingleton<IConnectivityService, MauiConnectivityService>();

        // --- Authenticated HTTP handler ---
        builder.Services.AddSingleton<AuthenticatedHttpClientHandler>();
        builder.Services.AddSingleton<HttpMessageHandler>(sp =>
        {
            var handler = sp.GetRequiredService<AuthenticatedHttpClientHandler>();
#if DEBUG
            // Trust the ASP.NET Core dev certificate in debug builds
            handler.InnerHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
#else
            handler.InnerHandler = new HttpClientHandler();
#endif
            return handler;
        });

        // --- Refit API clients ---
        RegisterRefitClient<IAuthApi>(builder.Services, baseAddress);
        RegisterRefitClient<IWorkforceApi>(builder.Services, baseAddress);
        RegisterRefitClient<IAttendanceApi>(builder.Services, baseAddress);
        RegisterRefitClient<ILeaveApi>(builder.Services, baseAddress);
        RegisterRefitClient<IEmployeesApi>(builder.Services, baseAddress);
        RegisterRefitClient<INotificationsApi>(builder.Services, baseAddress);
        RegisterRefitClient<IUsersApi>(builder.Services, baseAddress);
        RegisterRefitClient<ITimesheetApi>(builder.Services, baseAddress);

        // --- Local database ---
        builder.Services.AddSingleton(sp =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "hrconnect.db3");
            return new LocalDatabase(dbPath);
        });

        // --- Local repositories ---
        builder.Services.AddSingleton<ILocalLeaveRepository, LocalLeaveRepository>();
        builder.Services.AddSingleton<ILocalAttendanceRepository, LocalAttendanceRepository>();
        builder.Services.AddSingleton<ILocalEmployeeRepository, LocalEmployeeRepository>();
        builder.Services.AddSingleton<ILocalTimesheetRepository, LocalTimesheetRepository>();

        // --- Sync engine ---
        builder.Services.AddSingleton<ISyncEngine, SyncEngine>();
        builder.Services.AddSingleton<ISyncService, SyncService>();

        // --- App services ---
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IDialogService, UserManagement.Mobile.Services.DialogService>();

        // --- ViewModels ---
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<AttendanceViewModel>();
        builder.Services.AddTransient<LeaveListViewModel>();
        builder.Services.AddTransient<LeaveRequestViewModel>();
        builder.Services.AddTransient<EmployeeDirectoryViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<TimesheetViewModel>();

        // --- Pages ---
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<AttendancePage>();
        builder.Services.AddTransient<LeaveListPage>();
        builder.Services.AddTransient<LeaveRequestPage>();
        builder.Services.AddTransient<EmployeeDirectoryPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<TimesheetPage>();

        var app = builder.Build();

        // Initialize database on startup
        Task.Run(async () =>
        {
            var db = app.Services.GetRequiredService<LocalDatabase>();
            await db.InitializeAsync();
        });

        // Wire up auth service into the HTTP handler for token refresh
        var authHandler = app.Services.GetRequiredService<AuthenticatedHttpClientHandler>();
        var authService = app.Services.GetRequiredService<IAuthenticationService>();
        authHandler.SetAuthService(authService);

        // Initialize sync service (subscribes to connectivity changes for auto-sync)
        var syncService = app.Services.GetRequiredService<ISyncService>();
        syncService.Initialize();

        return app;
    }

    // Shared JSON options matching the server's expected format (camelCase, same as Blazor client)
    private static readonly JsonSerializerOptions SharedJsonOptions = new(JsonSerializerDefaults.Web);

    private static void RegisterRefitClient<T>(IServiceCollection services, string baseAddress) where T : class
    {
        services.AddSingleton(sp =>
        {
            var handler = sp.GetRequiredService<HttpMessageHandler>();
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
            return RestService.For<T>(httpClient, new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(SharedJsonOptions)
            });
        });
    }
}
