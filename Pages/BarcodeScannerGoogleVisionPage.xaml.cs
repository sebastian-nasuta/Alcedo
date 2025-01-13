using BarcodeScanner.Mobile;

namespace Alcedo.Pages;

public partial class BarcodeScannerGoogleVisionPage : ContentPage
{
    public BarcodeScannerGoogleVisionPage()
    {
        InitializeComponent();

#if ANDROID || IOS
        BarcodeScanner.Mobile.Methods.AskForRequiredPermission();
#endif
    }

    private void Camera_OnDetected(object sender, BarcodeScanner.Mobile.OnDetectedEventArg e)
    {
        List<BarcodeResult> obj = e.BarcodeResults;

        string result = string.Empty;
        for (int i = 0; i < obj.Count; i++)
        {
            result += $"Type : {obj[i].BarcodeType}, Value : {obj[i].DisplayValue}{Environment.NewLine}";
        }

        Dispatcher.Dispatch(async () =>
        {
            await DisplayAlert("Result", result, "OK");
            // If you want to start scanning again
            Camera.IsScanning = true;
        });
    }
}