using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixelator.UniversalNav.Extensions
{
    static class MimeTypeExtensions
    {
        public static bool IsImageFormat(this string contentType)
        {
            if (contentType == "image/jpeg")
                return true;
            if (contentType == "image/png")
                return true;
            return false;
        }
    }
}
