using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.Storage.Streams;

namespace Pixelator.Core
{
	public class SimpleBitmap
	{
		private IStorageFile file;
		private byte[] pixels;
		private int width;
		private int height;
        private WriteableBitmap bitmap;

        public SimpleBitmap(IStorageFile file, int width, int height)
		{
			this.file = file;
			this.width = width;
			this.height = height;
		}

		public async Task LoadImageAsync()
		{
			using (var stream = await file.OpenReadAsync())
			{
				WriteableBitmap bitmap = new WriteableBitmap(width, height);
				await bitmap.SetSourceAsync(stream);

				using (var buffer = bitmap.PixelBuffer.AsStream())
				{
					pixels = new Byte[4 * width * height];
					buffer.Read(pixels, 0, pixels.Length);
				}

                this.bitmap = bitmap;
			}
		}

		public Color GetPixel(int x, int y)
		{
			Color clr;

			int index = ((y * width) + x) * 4;

			Byte b = pixels[index + 0];
			Byte g = pixels[index + 1];
			Byte r = pixels[index + 2];
			Byte a = pixels[index + 3];

			clr = Color.FromArgb(a, r, g, b);

			return clr;
		}

		public void SetPixel(int x, int y, Color color)
		{
			int index = ((y * width) + x) * 4;

			pixels[index + 0] = color.B;
			pixels[index + 1] = color.G;
			pixels[index + 2] = color.R;
			pixels[index + 3] = color.A;
		}

		public WriteableBitmap GetImage()
		{
			using (var buffer = bitmap.PixelBuffer.AsStream())
			{
				buffer.Position = 0;
				buffer.Write(pixels, 0, pixels.Length);
			}

			return bitmap;
		}
	}
}
