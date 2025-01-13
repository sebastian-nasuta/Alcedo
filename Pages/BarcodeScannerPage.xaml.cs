using Alcedo.ViewModels;
using ZXing.Net.Maui;
/*
using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
*/

namespace Alcedo.Pages;

public partial class BarcodeScannerPage : ContentPage
{
    public BarcodeScannerPage()
    {
        InitializeComponent();
        BindingContext = new BarcodeScannerViewModel();
        
        barcodeReader.Options = new BarcodeReaderOptions()
        {
            AutoRotate = true,
            Formats = BarcodeFormats.All,
            Multiple = true,
            TryHarder = true,
            TryInverted = true
        };
    }

    private void CameraBarcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            barcodeResult.Text = $"{e.Results[0].Value} {e.Results[0].Format}";
        });
    }
}