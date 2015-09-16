using GalaSoft.MvvmLight;
using Pixelator.UniversalNav.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Pixelator.UniversalNav.ViewModel
{
    public class PicturesViewModel : ViewModelBase
    {
        private ObservableCollection<Picture> pictures;

        public PicturesViewModel()
        {
            this.pictures = new ObservableCollection<Picture>();
            this.MessengerInstance.Register<LoadImagesMessage>(this, async (msg) => await LoadImagesAsync(msg));
        }

        private async Task LoadImagesAsync(LoadImagesMessage message)
        {
            var picturesFolder = KnownFolders.PicturesLibrary;
            await ListFilesInFolder(picturesFolder);
        }

        public IList<Picture> Pictures
        {
            get { return pictures; }
        }

        public async Task ListFilesInFolder(StorageFolder folder)
        {
            var foldersInFolder = await folder.GetFoldersAsync();

            foreach (StorageFolder currentChildFolder in foldersInFolder)
            {
                await ListFilesInFolder(currentChildFolder);
            }

            var filesInFolder = await folder.GetFilesAsync();
            foreach (StorageFile currentFile in filesInFolder)
            {
                if (currentFile.ContentType.IsImageFormat())
                {
                    var picture = new Picture(currentFile.Name, currentFile.DateCreated.DateTime, currentFile.Path);
                    if (!pictures.Contains(picture))
                    {
                        pictures.Add(picture);
                        await picture.LoadImageAsync();
                    }
                }
            }
        }
    }

    internal class LoadImagesMessage
    {
    }

    public class Picture : ObservableObject, IEquatable<Picture>
    {
        public Picture(string fileName, DateTime dateCreated, string filePath)
        {
            FileName = fileName;
            DateCreated = dateCreated;
            FilePath = filePath;
            ImageSource = new BitmapImage();
        }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime DateCreated { get; set; }
        public BitmapSource ImageSource { get; set; }

        public async Task LoadImageAsync()
        {
            var file = await StorageFile.GetFileFromPathAsync(FilePath);
            using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap.
                var bitmapImage = new BitmapImage();

                await ImageSource.SetSourceAsync(fileStream);
                RaisePropertyChanged(() => ImageSource);
            }
        }

        public bool Equals(Picture other)
        {
            if (this.FilePath == other.FilePath)
            {
                return true;
            }
            return false;
        }
    }
}