using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Pixelator.Core.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private BitmapSource originalImage;
        private BitmapSource pixelatedImage;
        private IStorageFile file;
        private int pixelateSize = 50;

        public MainViewModel()
        {
            this.LoadImagesCommand = new RelayCommand(async () => await LoadImageAsync());
            this.PixelateImageCommand = new RelayCommand(async () => await PixelateAsync());
        }

        public BitmapSource OriginalImage
        {
            get { return originalImage; }
            set
            {
                Set<BitmapSource>(() => OriginalImage, ref originalImage, value);
            }
        }

        public BitmapSource PixelatedImage
        {
            get { return pixelatedImage; }
            set
            {
                Set<BitmapSource>(() => PixelatedImage, ref pixelatedImage, value);
            }
        }

        public int PixelateSize
        {
            get { return pixelateSize; }
            set
            {
                Set<int>(() => PixelateSize, ref pixelateSize, value);
            }
        }

        public ICommand LoadImagesCommand { get; private set; }
        public ICommand PixelateImageCommand { get; private set; }

        private async Task LoadImageAsync()
        {
            // Load an image

            Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");

            // Open the file picker.
            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();
            this.file = file;

            OriginalImage = await PixelatorBitmap.GetImageFromFileAsync(file);
        }

        private async Task PixelateAsync()
        {
            PixelatedImage = await PixelatorBitmap.Pixelate(file, (BitmapImage)OriginalImage, PixelateSize);
        }
    }
}