using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class FaceCapturePage : ContentPage, IQueryAttributable
{
    private readonly FaceCaptureViewModel _viewModel;

    public FaceCapturePage(FaceCaptureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        viewModel.NavigateAsync = route => Shell.Current.GoToAsync(route);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.ApplyQueryAttributes(query);
    }

    private async void OnCaptureClicked(object? sender, EventArgs e)
    {
        try
        {
            // Check camera permission
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    await DisplayAlertAsync("Permission Required",
                        "Camera permission is needed for face verification.", "OK");
                    return;
                }
            }

            // Capture photo using device camera
            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture Your Face"
            });

            if (photo is null)
                return; // User cancelled

            // Read photo bytes
            byte[] imageData;
            await using (var stream = await photo.OpenReadAsync())
            {
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            // Show captured preview
            CapturedPreview.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
            CapturedPreview.IsVisible = true;
            FaceIcon.IsVisible = false;

            // Submit frame for liveness analysis
            await _viewModel.SubmitLivenessFrameCommand.ExecuteAsync(imageData);

            // Process the captured image for face matching
            await _viewModel.ProcessCapturedImageCommand.ExecuteAsync(imageData);

            // Reset preview for next attempt if still on page
            CapturedPreview.IsVisible = false;
            FaceIcon.IsVisible = true;
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlertAsync("Not Supported",
                "Camera is not available on this device.", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlertAsync("Permission Denied",
                "Camera permission is required for face verification.", "OK");
        }
    }
}
