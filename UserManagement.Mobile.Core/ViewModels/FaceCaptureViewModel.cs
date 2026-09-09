using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class FaceCaptureViewModel(
    IFaceRecognitionService faceRecognitionService,
    ILivenessDetectionService livenessService,
    IBiometricApi biometricApi,
    IConnectivityService connectivityService,
    LocalDatabase localDatabase) : ViewModelBase
{
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromSeconds(30);

    [ObservableProperty] private string _mode = "clockin";
    [ObservableProperty] private string _guidanceText = "Position your face in the oval";
    [ObservableProperty] private bool _faceDetected;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private int _retryCount;
    [ObservableProperty] private int _maxRetries = 3;
    [ObservableProperty] private bool _livenessReady;

    private float[]? _enrolledEmbedding;
    private string? _sessionNonce;

    public override async Task InitializeAsync()
    {
        Title = "Face Verification";
        ClearError();

        try
        {
            // Load ONNX model if not already loaded
            if (!faceRecognitionService.IsModelLoaded)
            {
                GuidanceText = "Loading face recognition model...";
                await faceRecognitionService.LoadModelAsync();
            }

            // Load enrolled embedding (from local cache or server)
            await LoadEnrolledEmbeddingAsync();

            if (_enrolledEmbedding is null)
            {
                SetError("No face enrollment found. Please enroll first.");
                return;
            }

            // Start liveness session
            livenessService.StartSession(SessionTimeout);
            _sessionNonce = livenessService.GenerateSessionNonce();
            LivenessReady = false;

            GuidanceText = "Position your face in the oval";
        }
        catch (Exception ex)
        {
            SetError($"Initialization failed: {ex.Message}");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var mode))
            Mode = mode?.ToString() ?? "clockin";
    }

    /// <summary>
    /// Called by the camera page for each preview frame to perform liveness analysis.
    /// Should be called at ~2-3 fps before the user taps "Capture".
    /// </summary>
    [RelayCommand]
    private async Task SubmitLivenessFrameAsync(byte[] imageData)
    {
        if (_enrolledEmbedding is null || IsProcessing) return;

        if (livenessService.IsSessionExpired)
        {
            GuidanceText = "Session expired. Tap Retry.";
            SetError("Verification session timed out. Please try again.");
            return;
        }

        var frameResult = await livenessService.AnalyzeFrameAsync(imageData);
        GuidanceText = frameResult.GuidanceText;

        // Check if we have enough frames for a verdict
        var verdict = livenessService.GetVerdict();
        LivenessReady = verdict.IsLive || verdict.Confidence > 0;
    }

    /// <summary>
    /// Called by the camera page when the user taps "Capture" or auto-captures.
    /// Performs liveness verdict + face embedding comparison.
    /// </summary>
    [RelayCommand]
    private async Task ProcessCapturedImageAsync(byte[] imageData)
    {
        if (IsProcessing || _enrolledEmbedding is null) return;

        IsProcessing = true;
        GuidanceText = "Verifying...";
        ClearError();

        try
        {
            // Check session timeout
            if (livenessService.IsSessionExpired)
            {
                SetError("Verification session expired. Please try again.");
                GuidanceText = "Session expired";
                return;
            }

            // Frame was already submitted via SubmitLivenessFrameCommand — get verdict
            var verdict = livenessService.GetVerdict();
            if (!verdict.IsLive)
            {
                RetryCount++;
                if (RetryCount >= MaxRetries)
                {
                    SetError(verdict.FailureReason ?? "Liveness check failed after maximum attempts.");
                    GuidanceText = "Verification failed";
                }
                else
                {
                    GuidanceText = verdict.FailureReason ?? "Liveness check failed. Try again.";
                    // Reset liveness for next attempt
                    livenessService.StartSession(SessionTimeout);
                    _sessionNonce = livenessService.GenerateSessionNonce();
                }
                return;
            }

            // Generate embedding from captured image
            var capturedEmbedding = await faceRecognitionService.GenerateEmbeddingAsync(imageData);

            // Compare with enrolled embedding
            var result = faceRecognitionService.Verify(capturedEmbedding, _enrolledEmbedding);

            if (result.IsMatch)
            {
                // Navigate to clock result with combined confidence
                var combinedConfidence = result.Confidence * 0.7f + verdict.Confidence * 0.3f;
                await NavigateAsync(
                    $"ClockResultPage?mode={Mode}&method=face&confidence={combinedConfidence:F3}");
            }
            else
            {
                RetryCount++;
                if (RetryCount >= MaxRetries)
                {
                    SetError("Face verification failed after maximum attempts. Please try again later.");
                    GuidanceText = "Verification failed";
                }
                else
                {
                    GuidanceText = $"Face not matched. Try again ({RetryCount}/{MaxRetries})";
                    // Reset liveness for next attempt
                    livenessService.StartSession(SessionTimeout);
                    _sessionNonce = livenessService.GenerateSessionNonce();
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"Verification error: {ex.Message}");
            GuidanceText = "Error occurred. Try again.";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void RetrySession()
    {
        ClearError();
        RetryCount = 0;
        LivenessReady = false;
        livenessService.StartSession(SessionTimeout);
        _sessionNonce = livenessService.GenerateSessionNonce();
        GuidanceText = "Position your face in the oval";
    }

    private async Task LoadEnrolledEmbeddingAsync()
    {
        // Try local cache first
        var conn = localDatabase.GetConnection();
        var local = await conn.Table<Offline.Database.Entities.LocalFaceEmbedding>()
            .FirstOrDefaultAsync();

        if (local is not null)
        {
            _enrolledEmbedding = faceRecognitionService.BytesToEmbedding(local.Embedding);
            return;
        }

        // Fetch from server
        if (!connectivityService.IsConnected) return;

        try
        {
            var response = await biometricApi.GetFaceEmbeddingAsync();
            var bytes = Convert.FromBase64String(response.EmbeddingBase64);
            _enrolledEmbedding = faceRecognitionService.BytesToEmbedding(bytes);

            // Cache locally
            await conn.InsertOrReplaceAsync(new Offline.Database.Entities.LocalFaceEmbedding
            {
                UserId = "current",
                Embedding = bytes,
                EnrolledAt = DateTimeOffset.UtcNow
            });
        }
        catch
        {
            // Silently fail — will show error if embedding is null
        }
    }
}
