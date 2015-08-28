using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixalator
{
	public class Options
	{
		[Option('v', "verbose", HelpText = "Prints all messages to standard output.")]
		public bool Verbose { get; set; }

		[Option('i', "imaage", HelpText = "Path to the original image.", Required = true)]
		public string ImagePath { get; set; }

		[Option('r', "result", HelpText = "Path to the resulting image.", Required = true)]
		public string ResultPath { get; set; }

		[Option('s', "size", HelpText = "Pixel size.", Required = true)]
		public int PixelSize { get; set; }
	}
}
