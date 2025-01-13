using Alcedo.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace Alcedo.Pages;

public partial class BarcodeScannerPage : ContentPage
{
    public BarcodeScannerPage()
    {
        InitializeComponent();
        BindingContext = new BarcodeScannerViewModel();
        SetupCamera();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetupCamera();
    }

    private void SetupCamera()
    {
        var newCameraView = new CameraBarcodeReaderView
        {
            CameraLocation = CameraLocation.Rear,
            HeightRequest = 300,
            IsDetecting = true,
            IsTorchOn = false,
            Options = new()
            {
                AutoRotate = true,
                Formats = BarcodeFormats.All,
                Multiple = true,
                TryHarder = true,
                TryInverted = true
            },
            WidthRequest = 300
        };
        newCameraView.BarcodesDetected += CameraBarcodeReaderView_BarcodesDetected;
        var index = verticalStackLayout.Children.IndexOf(barcodeReader);
        verticalStackLayout.Children.Remove(barcodeReader);
        verticalStackLayout.Children.Insert(index, newCameraView);
        barcodeReader = newCameraView;
    }

    private void CameraBarcodeReaderView_BarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            barcodeResult.Text = $"{e.Results[0].Value} {e.Results[0].Format}";
        });
    }
}
