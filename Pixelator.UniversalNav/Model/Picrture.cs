using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Xaml.Media.Imaging;

namespace Pixelator.UniversalNav.Model
{
    public class Picture : ObservableObject, IEquatable<Picture>
    {
        public Picture(string fileName, DateTime dateCreated, string filePath)
        {
            FileName = fileName;
            DateCreated = dateCreated;
            FilePath = filePath;
            ImageSource = new BitmapImage();

            this.SetAsBackgroundCommand = new RelayCommand(async () =>
            {
                if (UserProfilePersonalizationSettings.IsSupported())
                {
                    var file = await SaveImage("backgroundimage.jpg");
                    UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                    Debug.Write(await profileSettings.TrySetWallpaperImageAsync(file));
                }
            });

            this.SetAsLockscreenCommand = new RelayCommand(async () =>
            {
                if (UserProfilePersonalizationSettings.IsSupported())
                {
                    var file = await SaveImage("lockscreenimage.jpg");
                    UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                    Debug.Write(await profileSettings.TrySetLockScreenImageAsync(file));
                }
            });
        }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime DateCreated { get; set; }

        [JsonIgnore]
        public BitmapSource ImageSource { get; set; }

        [JsonIgnore]
        public ICommand SetAsBackgroundCommand { get; private set; }
        [JsonIgnore]
        public ICommand SetAsLockscreenCommand { get; private set; }

        public async Task LoadImageAsync()
        {
            var file = await StorageFile.GetFileFromPathAsync(FilePath);
            using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap.
                //var bitmapImage = new BitmapImage();

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

        private async Task<StorageFile> SaveImage(string name)
        {
            var localAppDataFileName = name;

            var localFolder = ApplicationData.Current.LocalFolder;
            var folder = await localFolder.CreateFolderAsync("Local", CreationCollisionOption.ReplaceExisting);
            var file = await folder.CreateFileAsync(localAppDataFileName, CreationCollisionOption.ReplaceExisting);

            var image = await StorageFile.GetFileFromPathAsync(FilePath);
            await image.CopyAndReplaceAsync(file);

            return file;
        }
    }
}
