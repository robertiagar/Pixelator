using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using Pixelator.Core;
using Pixelator.UniversalNav.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Pixelator.UniversalNav.ViewModel
{
    public class PixelatePictureViewModel : ViewModelBase
    {
        private int pixelateSize;
        private int imageCount;
        private bool pixelated = false;
        private bool added;

        public PixelatePictureViewModel()
        {
            ImageSource = null;
            PixelateCommand = new RelayCommand(async () => await PixelateAsync(), () => CanPixelate());
            SavePictureCommand = new RelayCommand(async () => await SaveAsync(), () => CanSave());
            AddImageCommand = new RelayCommand(async () => await AddImageAsync());

        }


        public ICommand PixelateCommand { get; private set; }
        public ICommand SavePictureCommand { get; private set; }
        public ICommand AddImageCommand { get; private set; }
        public BitmapSource ImageSource { get; private set; }

        public IStorageFile File { get; set; }

        public int PixelateSize
        {
            get { return pixelateSize; }
            set
            {
                Set<int>(() => PixelateSize, ref pixelateSize, value);
            }
        }

        private async Task AddImageAsync()
        {
            var fp = new FileOpenPicker();

            fp.FileTypeFilter.Add(".jpeg");
            fp.FileTypeFilter.Add(".png");
            fp.FileTypeFilter.Add(".bmp");
            fp.FileTypeFilter.Add(".jpg");
            // Using PickSingleFileAsync() will return a storage file which can be saved into an object of storage file class.          
            StorageFile sf = await fp.PickSingleFileAsync();

            await SetPictureAsync(sf);
        }

        private async Task SetPictureAsync(StorageFile file)
        {
            File = file;
            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                ImageSource = new BitmapImage();
                await ImageSource.SetSourceAsync(fileStream);
                RaisePropertyChanged(() => ImageSource);
                added = true;
                pixelated = false;
                (PixelateCommand as RelayCommand).RaiseCanExecuteChanged();
                (SavePictureCommand as RelayCommand).RaiseCanExecuteChanged();
            }
        }

        public async Task PixelateAsync()
        {
            var image = await PixelatorBitmap.PixelateAsync(File, pixelateSize);

            ImageSource = image;
            RaisePropertyChanged(() => ImageSource);
            pixelated = true;
            (SavePictureCommand as RelayCommand).RaiseCanExecuteChanged();
            (PixelateCommand as RelayCommand).RaiseCanExecuteChanged();
        }

        private bool CanPixelate()
        {
            return !pixelated && added;
        }

        public async Task SaveAsync()
        {
            try
            {
                //try get files and folders
                var appFolder = ApplicationData.Current.LocalFolder;
                var picturesFile = await appFolder.GetFileAsync("pictures.json");
                var picturesString = await FileIO.ReadTextAsync(picturesFile);
                var picturesList = JsonConvert.DeserializeObject<IList<Picture>>(picturesString);
                imageCount = picturesList.Count;

                var imageName = string.Format("image{0}.jpg", imageCount);
                var folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Pixelator", CreationCollisionOption.OpenIfExists);
                var imagePath = Path.Combine(folder.Path, imageName);
                await SaveImageFile(imageName, folder);

                var picture = new Picture(imageName, DateTime.Now, imagePath);
                picturesList.Add(picture);

                picturesString = JsonConvert.SerializeObject(picturesList);
                await FileIO.WriteTextAsync(picturesFile, picturesString);
            }
            catch
            {
                var imageName = string.Format("image{0}.jpg", imageCount);
                var folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Pixelator", CreationCollisionOption.ReplaceExisting);
                var imagePath = Path.Combine(folder.Path, imageName);
                await SaveImageFile(imageName, folder);

                var picture = new Picture(imageName, DateTime.Now, imagePath);
                var appFolder = ApplicationData.Current.LocalFolder;
                var picturesFile = await appFolder.CreateFileAsync("pictures.json", CreationCollisionOption.ReplaceExisting);
                var picturesList = new List<Picture>();
                picturesList.Add(picture);

                var picturesString = JsonConvert.SerializeObject(picturesList);
                await FileIO.WriteTextAsync(picturesFile, picturesString);
            }
        }

        private async Task SaveImageFile(string imageName, StorageFolder folder)
        {
            var file = await folder.CreateFileAsync(imageName);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var writeableBitmap = ImageSource as WriteableBitmap;
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream.AsRandomAccessStream());
                // Get pixels of the WriteableBitmap object 
                Stream pixelStream = writeableBitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                // Save the image file with jpg extension 
                encoder.SetPixelData(BitmapPixelFormat.Unknown, BitmapAlphaMode.Ignore, (uint)writeableBitmap.PixelWidth, (uint)writeableBitmap.PixelHeight, 120, 120, pixels);
                await encoder.FlushAsync();
            }
        }

        private bool CanSave()
        {
            return pixelated;
        }
    }
}
