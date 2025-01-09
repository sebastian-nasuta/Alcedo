using Alcedo.Services.ImageTaggingService;
using Alcedo.ViewModels;

namespace Alcedo.Pages;

public partial class ImageTaggingPage : ContentPage
{
    public ImageTaggingPage(IImageTaggingService imageTaggingService)
    {
        InitializeComponent();
        BindingContext = new ImageTaggingViewModel(imageTaggingService);
    }
}
