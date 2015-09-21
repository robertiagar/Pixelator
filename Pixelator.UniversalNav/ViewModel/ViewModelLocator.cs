using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixelator.UniversalNav.ViewModel
{
    class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<PicturesViewModel>();
            SimpleIoc.Default.Register<PixelatedPicturesViewModel>();
            SimpleIoc.Default.Register<PixelatePictureViewModel>();
        }

        public PicturesViewModel Pictures
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PicturesViewModel>();
            }
        }

        public PixelatedPicturesViewModel PixelatedPictures
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PixelatedPicturesViewModel>();
            }
        }

        public PixelatePictureViewModel PixelatePicture
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PixelatePictureViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
