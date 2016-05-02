using System;
using System.Web.Mvc;

namespace Cascade.ImageResizer.Extensions
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Gets a url and querystring suitable for use with Image Resizer. Works with blobs too.
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="url">original Image Url</param>
        /// <param name="width">desired image width</param>
        /// <param name="height">desired image height</param>
        /// <param name="etc">any other Image Resizer parameters eg "mode=crop&border=2"</param>
        /// <returns></returns>
        public static string ResizeImage(this UrlHelper urlHelper, string url, int? width = null, int? height = null, string etc = null)
        {
            // Azure blob storage strings must be 'redirected' to a relative url for them to be processed by image resizer
            // assuming resizer is running on the same url as the website
            if (url.StartsWith("http"))
            {
                if (!url.Contains("blob.core.windows.net"))
                {
                    return url;
                }

                var index = url.IndexOf("//") + 2;
                index = url.IndexOf('/', index);
                url = url.Substring(index);
            }

            string qs = String.Empty;
            if (width.HasValue)
                qs += "width=" + width.ToString() + '&';
            if (height.HasValue)
                qs += "height=" + height.ToString() + '&';
            if (!String.IsNullOrWhiteSpace(etc))
                qs += etc;

            qs = qs.TrimEnd(new char[] { '&' });
            return url + '?' + qs;
        }
    }
}