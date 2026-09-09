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
        RegisterRefitClient<IBiometricApi>(builder.Services, baseAddress);

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
        builder.Services.AddSingleton<IGeofenceMonitoringService, UserManagement.Mobile.Handlers.MauiGeofenceMonitoringService>();
        builder.Services.AddSingleton<IFaceRecognitionService>(sp =>
        {
            var service = new FaceRecognitionService();
            service.ModelPathResolver = async () =>
            {
                const long MinValidModelSize = 10_000_000; // Real model is ~63 MB; anything < 10 MB is corrupt
                var targetPath = Path.Combine(FileSystem.AppDataDirectory, "arcface_model.onnx");

                // If file exists but is too small, it's a corrupt/HTML download — delete it
                if (File.Exists(targetPath))
                {
                    var fileInfo = new FileInfo(targetPath);
                    if (fileInfo.Length >= MinValidModelSize)
                        return targetPath;

                    fileInfo.Delete(); // Remove corrupt file
                }

                // Try to copy from bundled app package first (for offline/production builds)
                try
                {
                    await using var stream = await FileSystem.OpenAppPackageFileAsync("arcface_model.onnx");
                    await using var fs = File.Create(targetPath);
                    await stream.CopyToAsync(fs);
                    if (new FileInfo(targetPath).Length >= MinValidModelSize)
                        return targetPath;
                    File.Delete(targetPath);
                }
                catch
                {
                    // Not bundled — download on demand
                }

                // Download ArcFace INT8 model from Hugging Face ONNX Model Zoo (one-time, ~63 MB)
                // Use a handler that follows redirects (Hugging Face LFS uses CDN redirects)
                const string modelUrl = "https://huggingface.co/onnxmodelzoo/arcfaceresnet100-11-int8/resolve/main/arcfaceresnet100-11-int8.onnx";
                using var handler = new HttpClientHandler { AllowAutoRedirect = true, MaxAutomaticRedirections = 10 };
                using var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) };
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("UserManagement.Mobile/1.0");

                var tempPath = targetPath + ".tmp";
                await using (var response = await httpClient.GetStreamAsync(modelUrl))
                await using (var fs = File.Create(tempPath))
                {
                    await response.CopyToAsync(fs);
                }

                // Validate downloaded file
                if (new FileInfo(tempPath).Length < MinValidModelSize)
                {
                    File.Delete(tempPath);
                    throw new InvalidOperationException(
                        $"Downloaded model is too small (likely an error page). " +
                        $"Please download the ArcFace ONNX model manually from {modelUrl} " +
                        $"and place it at: {targetPath}");
                }

                File.Move(tempPath, targetPath, overwrite: true);
                return targetPath;
            };
            return service;
        });

        builder.Services.AddSingleton<ILivenessDetectionService, LivenessDetectionService>();

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
        builder.Services.AddTransient<BiometricChoiceViewModel>();
        builder.Services.AddTransient<FaceCaptureViewModel>();
        builder.Services.AddTransient<FaceEnrollmentViewModel>();
        builder.Services.AddTransient<ClockResultViewModel>();
        builder.Services.AddTransient<BiometricSettingsViewModel>();
        builder.Services.AddTransient<AdminBiometricViewModel>();

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
        builder.Services.AddTransient<BiometricChoicePage>();
        builder.Services.AddTransient<FaceCapturePage>();
        builder.Services.AddTransient<FaceEnrollmentPage>();
        builder.Services.AddTransient<ClockResultPage>();
        builder.Services.AddTransient<BiometricSettingsPage>();
        builder.Services.AddTransient<AdminBiometricPage>();

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
