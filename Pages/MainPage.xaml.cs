using Alcedo.Services.ImageTaggingService;
using SkiaSharp;

namespace Alcedo.Pages;

public partial class MainPage : ContentPage
{
    private bool _isImageLoaded;
    private readonly IImageTaggingService _imageTaggingService;

    public bool IsImageLoaded
    {
        get => _isImageLoaded;
        set
        {
            _isImageLoaded = value;
            OnPropertyChanged();
        }
    }

    public string Base64Image { get; private set; } = string.Empty;

    public MainPage(IImageTaggingService imageTaggingService)
    {
        _imageTaggingService = imageTaggingService;
        InitializeComponent();
        BindingContext = this;
    }

    protected async void OnLoadImageClickedAsync(object sender, EventArgs e)
    {
        try
        {
            ToggleLoadingIndicator(true);
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            });

            await LoadAndCompressImageAsync(result);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            await DisplayAlert("Error", $"An error occurred while picking the file: {ex.Message}", "OK");
        }
        finally
        {
            ToggleLoadingIndicator(false);
        }
    }

    protected async void OnTakePhotoClickedAsync(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.CapturePhotoAsync();

            await LoadAndCompressImageAsync(result);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            await DisplayAlert("Error", $"An error occurred while taking the photo: {ex.Message}", "OK");
        }
        finally
        {
            ToggleLoadingIndicator(false);
        }
    }

    protected async void OnTestClickedAsync(object sender, EventArgs e)
    {
        try
        {
            if (loadedImage.Source is null)
            {
                await DisplayAlert("Error", "Please load an image first", "OK");
                return;
            }

            ToggleLoadingIndicator(true);

            var response = await _imageTaggingService.TestImageRecognitionAsync(Base64Image);
            await DisplayAlert("Test Result", response, "OK");
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            await DisplayAlert("Error", $"An error occurred while testing: {ex.Message}", "OK");
        }
        finally
        {
            ToggleLoadingIndicator(false);
        }
    }

    protected async void OnGenerateTagsClickedAsync(object sender, EventArgs e)
    {
        try
        {
            if (loadedImage.Source is null)
            {
                await DisplayAlert("Error", "Please load an image first", "OK");
                return;
            }

            ToggleLoadingIndicator(true);

            var groupedTags = await _imageTaggingService.GetTagsAsync(Base64Image, GetCustomTag());

            tagsStackLayout.Children.Clear();

            foreach (var tagsGroup in groupedTags)
            {
                RenderTagGroup(tagsGroup);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            await DisplayAlert("Error", $"An error occurred while generating tags: {ex.Message}", "OK");
        }
        finally
        {
            ToggleLoadingIndicator(false);
        }
    }

    protected async void OnCopyAllTagsClickedAsync(object sender, EventArgs e)
    {
        try
        {
            var allTags = string.Join(" ", tagsStackLayout.Children.OfType<Editor>().Select(editor => editor.Text));
            await Clipboard.SetTextAsync(allTags);
            await DisplayAlert("Copied", "All tags have been copied to the clipboard.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while copying tags: {ex.Message}", "OK");
        }
    }

    protected async void OnClearImageClickedAsync(object sender, EventArgs e)
    {
        try
        {
            ToggleLoadingIndicator(true);
            SetImageSource(null);
            await Task.CompletedTask;
        }
        finally
        {
            ToggleLoadingIndicator(false);
        }
    }

    private async Task LoadAndCompressImageAsync(FileResult? result)
    {
        if (result != null)
        {
            using var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            do
            {
                imageBytes = CompressImage(imageBytes);

                Base64Image = Convert.ToBase64String(imageBytes);
            }
            while (Base64Image.Length > 65000);

            SetImageSource(ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(Base64Image))));
        }
    }

    private void RenderTagGroup(IGrouping<string, string> tagsGroup)
    {
        var label = new Label
        {
            FontSize = 20,
            Margin = new Thickness(5),
            Text = tagsGroup.Key.ToUpper(),
            VerticalOptions = LayoutOptions.Center
        };

        var copyButton = new Button
        {
            FontFamily = "MSO",
            FontSize = 20,
            Margin = new Thickness(5),
            Text = "content_copy",
            VerticalOptions = LayoutOptions.Center
        };

        var editor = new Editor
        {
            Text = string.Join(" ", tagsGroup.Select(x => "#" + x)),
            HeightRequest = 80,
            Margin = new Thickness(5)
        };

        copyButton.Clicked += (s, args) => Clipboard.SetTextAsync(editor.Text);

        var stackLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Children = { label, copyButton }
        };

        tagsStackLayout.Children.Add(stackLayout);
        tagsStackLayout.Children.Add(editor);
    }

    private static byte[] CompressImage(byte[] imageBytes)
    {
        using var inputStream = new MemoryStream(imageBytes);
        using var original = SKBitmap.Decode(inputStream);
        using var resized = original.Resize(new SKImageInfo(original.Width / 2, original.Height / 2), SKFilterQuality.Medium);
        using var image = SKImage.FromBitmap(resized);
        using var outputStream = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Jpeg, 75).SaveTo(outputStream);
        return outputStream.ToArray();
    }

    private void SetImageSource(ImageSource? imageSource)
    {
        tagsStackLayout.Children.Clear();
        loadedImage.Source = imageSource;
        IsImageLoaded = imageSource != null;
    }

    private void ToggleLoadingIndicator(bool value)
    {
        loadingIndicator.IsRunning = loadingIndicator.IsVisible = value;
    }

    private string? GetCustomTag() => !string.IsNullOrWhiteSpace(customTagEntry.Text) ? customTagEntry.Text : null;
}
