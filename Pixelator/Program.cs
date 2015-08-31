using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixelator
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = ParseArguments(args);
			if (!options.Verbose)
			{
			}

			var writer = options.Verbose ? Console.Out : TextWriter.Null;

			Pixelate(options);
		}

		private static void Pixelate(Options options)
		{
			var stopwatch = new Stopwatch();
			Console.WriteLine("Starting normal...");
			stopwatch.Start();
			using (var image = Bitmap.FromFile(options.ImagePath))
			using (var bitmap = new Bitmap(image))
			{
				var result = Pixelate(bitmap, options.PixelSize);
				result.Save(options.ResultPath);
			}
			stopwatch.Stop();
			Console.WriteLine("Done in: \"{0}\".", stopwatch.Elapsed.ToString("c"));
			stopwatch.Reset();
			Console.WriteLine("Starting using lockbits...");
			stopwatch.Start();
			using (var image = Bitmap.FromFile(options.ImagePath))
			using (var bitmap = new Bitmap(image))
			{
				var result = PixelateLockBits(bitmap, options.PixelSize);
				result.Save(options.ResultPath);
			}
			stopwatch.Stop();
			Console.WriteLine("Done in: \"{0}\".", stopwatch.Elapsed.ToString("c"));
			stopwatch.Reset();
			Console.WriteLine("Starting using lockbits parallel...");
			stopwatch.Start();
			using (var image = Bitmap.FromFile(options.ImagePath))
			using (var bitmap = new Bitmap(image))
			{
				var result = PixelateLockBitsParallel(bitmap, options.PixelSize);
				result.Save(options.ResultPath);
			}
			stopwatch.Stop();
			Console.WriteLine("Done in: \"{0}\".", stopwatch.Elapsed.ToString("c"));
		}

		private static Bitmap Pixelate(Bitmap image, int blurSize)
		{
			return Pixelate(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
		}

		private static Bitmap PixelateLockBits(Bitmap image, int blurSize)
		{
			return PixelateLockBits(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
		}

		private static Bitmap PixelateLockBitsParallel(Bitmap image, int blurSize)
		{
			return PixelateLockBitsParallel(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
		}

		private static Bitmap Pixelate(Bitmap image, Rectangle rectangle, int pixelateSize)
		{
			Bitmap pixelated = new System.Drawing.Bitmap(image.Width, image.Height);

			// make an exact copy of the bitmap provided
			using (Graphics graphics = System.Drawing.Graphics.FromImage(pixelated))
				graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
					 new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

			// look at every pixel in the rectangle while making sure we're within the image bounds
			for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width && xx < image.Width; xx += pixelateSize)
			{
				for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height && yy < image.Height; yy += pixelateSize)
				{
					Int32 offsetX = pixelateSize / 2;
					Int32 offsetY = pixelateSize / 2;

					// make sure that the offset is within the boundry of the image
					while (xx + offsetX >= image.Width) offsetX--;
					while (yy + offsetY >= image.Height) offsetY--;

					// get the pixel color in the center of the soon to be pixelated area
					Color pixel = pixelated.GetPixel(xx + offsetX, yy + offsetY);

					// for each pixel in the pixelate size, set it to the center color
					for (Int32 x = xx; x < xx + pixelateSize && x < image.Width; x++)
						for (Int32 y = yy; y < yy + pixelateSize && y < image.Height; y++)
							pixelated.SetPixel(x, y, pixel);
				}
			}

			return pixelated;
		}

		private static Bitmap PixelateLockBits(Bitmap image, Rectangle rectangle, int pixelateSize)
		{
			using (LockBitmap lockBitmap = new LockBitmap(image))
			{
				var width = image.Width;
				var height = image.Height;

				for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width && xx < image.Width; xx += pixelateSize)
				{
					for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height && yy < image.Height; yy += pixelateSize)
					{
						Int32 offsetX = pixelateSize / 2;
						Int32 offsetY = pixelateSize / 2;

						// make sure that the offset is within the boundry of the image
						while (xx + offsetX >= image.Width) offsetX--;
						while (yy + offsetY >= image.Height) offsetY--;

						// get the pixel color in the center of the soon to be pixelated area
						Color pixel = lockBitmap.GetPixel(xx + offsetX, yy + offsetY);



						// for each pixel in the pixelate size, set it to the center color
						for (Int32 x = xx; x < xx + pixelateSize && x < image.Width; x++)
							for (Int32 y = yy; y < yy + pixelateSize && y < image.Height; y++)
								lockBitmap.SetPixel(x, y, pixel);
					}
				}
			}

			return image;
		}

		private static Bitmap PixelateLockBitsParallel(Bitmap image, Rectangle rectangle, int pixelateSize)
		{

			using (LockBitmap lockBitmap = new LockBitmap(image))
			{
				var width = image.Width;
				var height = image.Height;

				for (Int32 xx = rectangle.X; xx < rectangle.X + rectangle.Width && xx < image.Width; xx += pixelateSize)
				{
					for (Int32 yy = rectangle.Y; yy < rectangle.Y + rectangle.Height && yy < image.Height; yy += pixelateSize)
					{
						Int32 offsetX = pixelateSize / 2;
						Int32 offsetY = pixelateSize / 2;

						// make sure that the offset is within the boundry of the image
						while (xx + offsetX >= image.Width) offsetX--;
						while (yy + offsetY >= image.Height) offsetY--;

						// get the pixel color in the center of the soon to be pixelated area
						Color pixel = lockBitmap.GetPixel(xx + offsetX, yy + offsetY);

						// for each pixel in the pixelate size, set it to the center color
						Parallel.For(xx, xx + pixelateSize, x =>
						{
							if (x < width)
							{
								Parallel.For(yy, yy + pixelateSize, y =>
								{
									if (y < height)
									{
										lockBitmap.SetPixel(x, y, pixel);
									}
								});
							}
						});
					}
				}
			}

			return image;

		}

		private static Options ParseArguments(string[] args)
		{
			var options = new Options();
			Parser.Default.ParseArgumentsStrict(args, options);
			return options;
		}
	}
}
