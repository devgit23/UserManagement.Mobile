using UserManagement.Mobile.Core.ViewModels;

namespace UserManagement.Mobile.Views;

public partial class FaceEnrollmentPage : ContentPage
{
    private readonly FaceEnrollmentViewModel _viewModel;

    public FaceEnrollmentPage(FaceEnrollmentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        viewModel.NavigateAsync = route => Shell.Current.GoToAsync(route);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateStepDots();
        await _viewModel.InitializeAsync();
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
                        "Camera permission is needed for face enrollment.", "OK");
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

            // Show captured preview briefly
            CapturedPreview.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
            CapturedPreview.IsVisible = true;
            FaceIcon.IsVisible = false;

            // Feed image to the ViewModel's capture step
            await _viewModel.CaptureStepCommand.ExecuteAsync(imageData);

            // Reset preview for next step
            CapturedPreview.IsVisible = false;
            FaceIcon.IsVisible = true;

            // Update step indicator dots
            UpdateStepDots();
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlertAsync("Not Supported",
                "Camera is not available on this device.", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlertAsync("Permission Denied",
                "Camera permission is required for face enrollment.", "OK");
        }
    }

    private void UpdateStepDots()
    {
        var activeColor = Application.Current?.Resources.TryGetValue("Primary", out var primary) == true
            ? (Color)primary : Colors.Blue;
        var inactiveColor = Colors.LightGray;

        Dot1.BackgroundColor = _viewModel.CurrentStep >= 1 ? activeColor : inactiveColor;
        Dot2.BackgroundColor = _viewModel.CurrentStep >= 2 ? activeColor : inactiveColor;
        Dot3.BackgroundColor = _viewModel.CurrentStep >= 3 ? activeColor : inactiveColor;
    }
}
