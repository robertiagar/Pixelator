using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace Pixelator.Core
{
    public static class PixelatorBitmap
    {
        public static async Task<BitmapImage> GetImageFromFileAsync(IStorageFile file)
        {
            IRandomAccessStream fileStream =
                              await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            // Set the image source to the selected bitmap.
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileStream);

            return bitmapImage;
        }

        public static async Task<WriteableBitmap> PixelateAsync(IStorageFile file, int pixelateSize)
        {
            var image =await GetImageFromFileAsync(file);
            return await PixelateAsync(file, image, pixelateSize);
        }

        public static async Task<WriteableBitmap> PixelateAsync(IStorageFile file, BitmapImage image, int pixelateSize)
        {
            var simpleBitmap = new SimpleBitmap(file, image.PixelWidth, image.PixelHeight);
            await simpleBitmap.LoadImageAsync();

            var width = image.PixelWidth;
            var height = image.PixelHeight;

            for (Int32 xx = 0; xx < width; xx += pixelateSize)
            {
                for (Int32 yy = 0; yy < height; yy += pixelateSize)
                {
                    Int32 offsetX = pixelateSize / 2;
                    Int32 offsetY = pixelateSize / 2;

                    // make sure that the offset is within the boundry of the image
                    while (xx + offsetX >= width) offsetX--;
                    while (yy + offsetY >= height) offsetY--;

                    // get the pixel color in the center of the soon to be pixelated area
                    Color pixel = simpleBitmap.GetPixel(xx + offsetX, yy + offsetY);

                    // for each pixel in the pixelate size, set it to the center color
                    Parallel.For(xx, xx + pixelateSize, x =>
                    {
                        if (x < width)
                        {
                            Parallel.For(yy, yy + pixelateSize, y =>
                            {
                                if (y < height)
                                {
                                    simpleBitmap.SetPixel(x, y, pixel);
                                }
                            });
                        }
                    });
                }
            }

            return simpleBitmap.GetImage();
        }
    }
}
