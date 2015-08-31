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
	public class PixelatorBitmap
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

		public static async Task<WriteableBitmap> PixelateFromFileAsync(IStorageFile file, BitmapImage bitmapImage, int pixelateSize)
		{
			int height = bitmapImage.PixelHeight;
			int width = bitmapImage.PixelWidth;

			using (var stream = await file.OpenReadAsync())
			{
				WriteableBitmap bitmap = new WriteableBitmap(width, height);
				await bitmap.SetSourceAsync(stream);

				using (var buffer = bitmap.PixelBuffer.AsStream())
				{
					Byte[] pixels = new Byte[4 * width * height];
					buffer.Read(pixels, 0, pixels.Length);

					Parallel.For(0, width, x =>
					{
						Parallel.For(0, height, y =>
						{
							int index = ((y * width) + x) * 4;

							Byte b = pixels[index + 0];
							Byte g = pixels[index + 1];
							Byte r = pixels[index + 2];
							Byte a = pixels[index + 3];

							// Some simple color manipulation
							byte newB = (Byte)((r + g) / 2);
							byte newG = (Byte)((r + b) / 2);
							byte newR = (Byte)((b + g) / 2);

							pixels[index + 0] = newB;
							pixels[index + 1] = newG;
							pixels[index + 2] = newR;
							pixels[index + 3] = a;
						});
					});

					buffer.Position = 0;
					buffer.Write(pixels, 0, pixels.Length);

					return bitmap;
				}
			}
		}

		public static async Task<WriteableBitmap> Pixelate(IStorageFile file, BitmapImage image, int pixelateSize)
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
