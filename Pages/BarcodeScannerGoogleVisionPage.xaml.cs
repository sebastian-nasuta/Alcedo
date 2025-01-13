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

    private async void Camera_OnDetected(object sender, OnDetectedEventArg e)
    {
        List<BarcodeResult> obj = e.BarcodeResults;
        string result = string.Join(Environment.NewLine, obj.Select(x => x.DisplayValue));

        Dispatcher.Dispatch(() =>
        {
            BarcodesEditor.Text = result;
        });

        await Task.Delay(1000);
        Camera.IsScanning = true;
    }

    private void CopyBarcodeButton_Clicked(object sender, EventArgs e)
    {
        Clipboard.SetTextAsync(BarcodesEditor.Text);
    }
}
