using Alcedo.Services.ImageTaggingService;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Alcedo.ViewModels;

internal partial class ImageTaggingViewModel : INotifyPropertyChanged
{
    private readonly IImageTaggingService _imageTaggingService;
    private bool _isImageLoaded;
    private string _base64Image = string.Empty;
    private ImageSource? _loadedImageSource;
    private bool _isLoading;
    private ObservableCollection<TagGroup> _tagGroups = [];
    private string? _customTag;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ImageSource? LoadedImageSource
    {
        get => _loadedImageSource;
        set
        {
            _loadedImageSource = value;
            IsImageLoaded = value != null;
            TagGroups.Clear();
            OnPropertyChanged();
        }
    }

    public bool IsImageLoaded
    {
        get => _isImageLoaded;
        set
        {
            _isImageLoaded = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public string Base64Image
    {
        get => _base64Image;
        private set
        {
            _base64Image = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TagGroup> TagGroups
    {
        get => _tagGroups;
        set
        {
            _tagGroups = value;
            OnPropertyChanged();
        }
    }

    public string? CustomTag
    {
        get => !string.IsNullOrWhiteSpace(_customTag) ? _customTag : null;
        set
        {
            _customTag = value;
            OnPropertyChanged();
        }
    }

    public Command LoadImageCommand { get; }
    public Command TakePhotoCommand { get; }
    public Command GenerateTagsCommand { get; }
    public Command CopyAllTagsCommand { get; }
    public Command ClearImageCommand { get; }

    public ImageTaggingViewModel(IImageTaggingService imageTaggingService)
    {
        _imageTaggingService = imageTaggingService;

        LoadImageCommand = new Command(async () => await OnLoadImageClickedAsync());
        TakePhotoCommand = new Command(async () => await OnTakePhotoClickedAsync());
        GenerateTagsCommand = new Command(async () => await OnGenerateTagsClickedAsync());
        CopyAllTagsCommand = new Command(async () => await OnCopyAllTagsClickedAsync());
        ClearImageCommand = new Command(OnClearImageClicked);
    }

    private async Task OnLoadImageClickedAsync()
    {
        try
        {
            IsLoading = true;
            var result = await FilePicker.PickAsync(new()
            {
                FileTypes = FilePickerFileType.Images
            });

            await LoadAndCompressImageAsync(result);
        }
        catch (Exception ex)
        {
            App.ShowExceptionAlert($"An error occurred while picking the file: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnTakePhotoClickedAsync()
    {
        try
        {
            IsLoading = true;
            var result = await MediaPicker.CapturePhotoAsync();

            await LoadAndCompressImageAsync(result);
        }
        catch (Exception ex)
        {
            App.ShowExceptionAlert($"An error occurred while taking the photo: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnGenerateTagsClickedAsync()
    {
        try
        {
            IsLoading = true;
            if (string.IsNullOrEmpty(Base64Image))
            {
                throw new ApplicationException("No image loaded.");
            }

            var groupedTags = await _imageTaggingService.GetTagsAsync(Base64Image, CustomTag);

            TagGroups.Clear();

            foreach (var tagsGroup in groupedTags)
            {
                TagGroups.Add(new TagGroup()
                {
                    Key = tagsGroup.Key,
                    Tags = [.. tagsGroup]
                });
            }
        }
        catch (Exception ex)
        {
            App.ShowExceptionAlert($"An error occurred while generating tags: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnCopyAllTagsClickedAsync()
    {
        try
        {
            IsLoading = true;
            var allTags = string.Join(" ", TagGroups.SelectMany(group => group.Tags.Select(tag => tag)));
            await Clipboard.SetTextAsync(allTags);
        }
        catch (Exception ex)
        {
            App.ShowExceptionAlert($"An error occurred while copying the tags: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnClearImageClicked()
    {
        try
        {
            IsLoading = true;
            Base64Image = string.Empty;
            SetImageSource();
        }
        catch (Exception ex)
        {
            App.ShowExceptionAlert($"An error occurred while clearing the image: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
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

            SetImageSource();
        }
    }

    private static byte[] CompressImage(byte[] imageBytes)
    {
        using var inputStream = new MemoryStream(imageBytes);
        using var original = SKBitmap.Decode(inputStream);
        using var resized = original.Resize(new SKImageInfo(original.Width / 2, original.Height / 2), SKSamplingOptions.Default);
        using var image = SKImage.FromBitmap(resized);
        using var outputStream = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Jpeg, 75).SaveTo(outputStream);
        return outputStream.ToArray();
    }

    private void SetImageSource()
    {
        LoadedImageSource = string.IsNullOrEmpty(Base64Image) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(Base64Image)));
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
