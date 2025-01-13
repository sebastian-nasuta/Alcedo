using Alcedo.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace Alcedo.Pages;

public partial class BarcodeScannerZXingPage : ContentPage
{
    public BarcodeScannerZXingPage()
    {
        InitializeComponent();
        BindingContext = new BarcodeScannerViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetupCamera();
    }

    private void SetupCamera()
    {
        Dispatcher.Dispatch(() =>
        {
            var newCameraView = new CameraBarcodeReaderView
            {
                CameraLocation = CameraLocation.Rear,
                HeightRequest = 300,
                IsDetecting = true,
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
            var index = gridLayout.Children.IndexOf(barcodeReader);
            gridLayout.Children.Remove(barcodeReader);
            gridLayout.Children.Insert(index, newCameraView);
            barcodeReader = newCameraView;
        });
    }

    private void CameraBarcodeReaderView_BarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            barcodeResult.Text = $"{e.Results[0].Value} {e.Results[0].Format}";
        });
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        barcodeReader.IsTorchOn = !barcodeReader.IsTorchOn;
    }
}
