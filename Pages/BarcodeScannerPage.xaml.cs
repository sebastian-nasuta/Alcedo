using Alcedo.ViewModels;
using Camera.MAUI;
using Camera.MAUI.ZXing;
using Camera.MAUI.ZXingHelper;

namespace Alcedo.Pages;

public partial class BarcodeScannerPage : ContentPage
{
    public BarcodeScannerPage()
    {
        InitializeComponent();
        BindingContext = new BarcodeScannerViewModel();
        cameraView.BarCodeOptions = new BarcodeDecodeOptions
        {
            AutoRotate = true,
            TryInverted = true,
            TryHarder = true,
            ReadMultipleCodes = true,
            PossibleFormats = { BarcodeFormat.All_1D }
        };
        cameraView.BarCodeDecoder = new ZXingBarcodeDecoder();
    }

    private void CamerasLoaded(object? sender, EventArgs e)
    {
        if (cameraView.NumCamerasDetected > 0)
        {
            if (cameraView.NumMicrophonesDetected > 0)
                cameraView.Microphone = cameraView.Microphones.First();
            cameraView.Camera = cameraView.Cameras.First();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (await cameraView.StartCameraAsync() == CameraResult.Success)
                {
                    //controlButton.Text = "Stop";
                    //playing = true;
                }
            });
        }
    }

    private async void OnBarcodeDetected(object? sender, BarcodeEventArgs args)
    {
        System.Diagnostics.Debugger.Break();
        foreach (var result in args.Result)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                //await Toast.Make(result.Text, ToastDuration.Long).Show();
                await Task.CompletedTask;
            });
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        cameraView.BarcodeDetected += OnBarcodeDetected;
        cameraView.CamerasLoaded += CamerasLoaded;
    }
}