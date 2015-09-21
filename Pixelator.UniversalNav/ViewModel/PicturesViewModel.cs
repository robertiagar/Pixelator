using GalaSoft.MvvmLight;
using Pixelator.UniversalNav.Extensions;
using Pixelator.UniversalNav.Model;
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

   
}