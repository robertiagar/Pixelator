using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using GalaSoft.MvvmLight.Messaging;
using Pixelator.UniversalNav.Model;

namespace Pixelator.UniversalNav.ViewModel
{
    public class PixelatedPicturesViewModel : ViewModelBase
    {
        private ObservableCollection<Picture> pictures;

        public PixelatedPicturesViewModel()
        {
            pictures = new ObservableCollection<Picture>();
            Messenger.Default.Register<LoadPixelatedImagesMessage>(this, async (msg) => await LoadPicturesAsync());
        }

        public IList<Picture> Pictures { get { return pictures; } }

        public async Task LoadPicturesAsync()
        {
            try
            {
                var appFolder = ApplicationData.Current.LocalFolder;
                var picturesFile = await appFolder.GetFileAsync("pictures.json");
                var picturesString = await FileIO.ReadTextAsync(picturesFile);
                var picturesList = JsonConvert.DeserializeObject<IEnumerable<Picture>>(picturesString);

                pictures.Clear();
                foreach (var picture in picturesList)
                {
                    pictures.Add(picture);
                    await picture.LoadImageAsync();
                }
            }
            catch
            {
                //file has not been created yet
            }
        }


    }

    internal class LoadPixelatedImagesMessage
    {
    }
}
