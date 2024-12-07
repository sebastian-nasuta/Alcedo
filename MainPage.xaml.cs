
using SkiaSharp;

namespace Alcedo;

public partial class MainPage : ContentPage
{
    public string Base64Image { get; private set; } = string.Empty;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnLoadImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Please select an image file"
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();

                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                //while (imageBytes.Length > 1 * 1024 * 1024 / 10)

                do
                {
                    imageBytes = CompressImage(imageBytes);

                    Base64Image = Convert.ToBase64String(imageBytes);
                }
                while (Base64Image.Length > 70000);

                LoadedImage.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(Base64Image)));
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            await DisplayAlert("Error", $"An error occurred while picking the file: {ex.Message}", "OK");
        }
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

    private async void OnGenerateTagsClicked(object sender, EventArgs e)
    {
        try
        {
            if (LoadedImage.Source is null)
            {
                await DisplayAlert("Error", "Please load an image first", "OK");
                return;
            }

            var tags = await ComputerVisionService.GetTagsAsync(Base64Image);
            TagsLabel.Text = tags[0];
            await DisplayAlert("Tags Generated", "Tags have been generated successfully", "OK");
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            await DisplayAlert("Error", $"An error occurred while generating tags: {ex.Message}", "OK");
        }
    }
}
