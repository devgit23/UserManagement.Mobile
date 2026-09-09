using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using UserManagement.Common.Biometric;
using UserManagement.Mobile.Core.Offline.Database;
using UserManagement.Mobile.Core.Services.Interfaces;
using UserManagement.Mobile.Core.ViewModels.Base;
using UserManagement.Shared.ApiContracts;

namespace UserManagement.Mobile.Core.ViewModels;

public partial class FaceEnrollmentViewModel(
    IFaceRecognitionService faceRecognitionService,
    IBiometricApi biometricApi,
    IConnectivityService connectivityService,
    LocalDatabase localDatabase) : ViewModelBase
{
    private const int TotalSteps = 3;

    [ObservableProperty] private int _currentStep = 1;
    [ObservableProperty] private string _stepInstruction = "Look straight at the camera";
    [ObservableProperty] private bool _isCapturing;
    [ObservableProperty] private bool _isEnrolling;
    [ObservableProperty] private bool _enrollmentComplete;
    [ObservableProperty] private double _progress;

    private readonly List<float[]> _capturedEmbeddings = [];
    private readonly List<byte[]> _capturedImages = [];

    private static readonly string[] StepInstructions =
    [
        "Look straight at the camera",
        "Turn your head slightly to the left",
        "Turn your head slightly to the right"
    ];

    public override async Task InitializeAsync()
    {
        Title = "Set Up Face Recognition";
        ClearError();

        try
        {
            if (!faceRecognitionService.IsModelLoaded)
            {
                StepInstruction = "Loading face recognition model...";
                await faceRecognitionService.LoadModelAsync();
            }

            UpdateStepUI();
        }
        catch (Exception ex)
        {
            SetError($"Failed to initialize: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when a face image is captured at the current step.
    /// </summary>
    [RelayCommand]
    private async Task CaptureStepAsync(byte[] imageData)
    {
        if (IsCapturing) return;

        IsCapturing = true;
        ClearError();

        try
        {
            var embedding = await faceRecognitionService.GenerateEmbeddingAsync(imageData);
            _capturedEmbeddings.Add(embedding);
            _capturedImages.Add(imageData);

            if (CurrentStep < TotalSteps)
            {
                CurrentStep++;
                UpdateStepUI();
            }
            else
            {
                // All steps done — finalize enrollment
                await FinalizeEnrollmentAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Capture failed: {ex.Message}. Please try again.");
        }
        finally
        {
            IsCapturing = false;
        }
    }

    [RelayCommand]
    private void RetakeStep()
    {
        if (_capturedEmbeddings.Count > 0)
        {
            _capturedEmbeddings.RemoveAt(_capturedEmbeddings.Count - 1);
            _capturedImages.RemoveAt(_capturedImages.Count - 1);
        }

        if (CurrentStep > 1)
        {
            CurrentStep--;
            UpdateStepUI();
        }

        ClearError();
    }

    private async Task FinalizeEnrollmentAsync()
    {
        IsEnrolling = true;
        StepInstruction = "Processing enrollment...";

        try
        {
            // Average the embeddings for a robust template
            var averagedEmbedding = faceRecognitionService.AverageEmbeddings(_capturedEmbeddings);
            var embeddingBytes = faceRecognitionService.EmbeddingToBytes(averagedEmbedding);
            var embeddingBase64 = Convert.ToBase64String(embeddingBytes);

            // Resize first captured image to a small thumbnail for server upload
            // (server enforces [StringLength(100000)] on ThumbnailBase64 ≈ ~75KB max)
            string? thumbnailBase64 = null;
            if (_capturedImages.Count > 0)
            {
                thumbnailBase64 = CreateThumbnailBase64(_capturedImages[0]);
            }

            // Upload to server
            if (connectivityService.IsConnected)
            {
                await biometricApi.EnrollFaceAsync(new FaceEnrollmentRequest
                {
                    FaceEmbeddingBase64 = embeddingBase64,
                    ThumbnailBase64 = thumbnailBase64,
                    QualityScore = 95.0m // Placeholder — real quality scoring can be added
                });
            }

            // Cache locally for offline verification
            var conn = localDatabase.GetConnection();
            await conn.InsertOrReplaceAsync(new Offline.Database.Entities.LocalFaceEmbedding
            {
                UserId = "current",
                Embedding = embeddingBytes,
                Thumbnail = _capturedImages.Count > 0 ? _capturedImages[0] : null,
                EnrolledAt = DateTimeOffset.UtcNow
            });

            EnrollmentComplete = true;
            StepInstruction = "Face enrolled successfully!";
        }
        catch (Exception ex)
        {
            SetError($"Enrollment failed: {ex.Message}");
            StepInstruction = "Enrollment failed. Please try again.";
        }
        finally
        {
            IsEnrolling = false;
        }
    }

    [RelayCommand]
    private async Task FinishAsync()
    {
        // Navigate back to biometric choice or attendance
        await NavigateAsync("..");
    }

    private void UpdateStepUI()
    {
        Progress = (double)(CurrentStep - 1) / TotalSteps;
        StepInstruction = CurrentStep <= TotalSteps
            ? StepInstructions[CurrentStep - 1]
            : "Complete";
    }

    /// <summary>
    /// Resizes a full-resolution camera image to a 200×200 JPEG thumbnail
    /// suitable for server upload (stays well under the 100K Base64 char limit).
    /// </summary>
    private static string CreateThumbnailBase64(byte[] imageData)
    {
        const int thumbnailSize = 200;
        const int jpegQuality = 70;

        using var original = SKBitmap.Decode(imageData);
        if (original is null)
            return Convert.ToBase64String(imageData); // fallback if decode fails

        // Maintain aspect ratio, fit within thumbnailSize × thumbnailSize
        float scale = Math.Min((float)thumbnailSize / original.Width, (float)thumbnailSize / original.Height);
        int newWidth = (int)(original.Width * scale);
        int newHeight = (int)(original.Height * scale);

        using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);
        if (resized is null)
            return Convert.ToBase64String(imageData); // fallback

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, jpegQuality);

        return Convert.ToBase64String(data.ToArray());
    }
}
