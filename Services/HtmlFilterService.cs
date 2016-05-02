using Cascade.ImageResizer.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cascade.ImageResizer.Services
{

    // 2/12/2014: DAG: If img width is specified as a percentage then don't specify a width to ImageResier 
    // TODO: parameterize format, quality and types of file to resize and create appropriate IOrchard Settings.
    // TODO: if size matches then do nothing (ie do not append resizer qs) - but how can we know the original size on disk without an expensive look?


    public class HtmlFilterService : IHtmlFilter
    {
        private static Regex imgTagRe = new Regex(@"<img\s.*?\>", RegexOptions.IgnoreCase);
        private static Regex srcRe = new Regex(@"\s+((?:data-)?src)[ ]*=[ ]*['""]?([^'""]*)['""]?", RegexOptions.IgnoreCase);
        private static Regex widthRe = new Regex(@"width[ ]*=[ ]*['""]?(\d*)([^0-9.]*)['""]", RegexOptions.IgnoreCase);
        private static Regex heightRe = new Regex(@"height[ ]*=[ ]*['""]?(\d*)[\D]", RegexOptions.IgnoreCase);
        private static Regex qualityRe = new Regex(@"quality=(\d*)", RegexOptions.IgnoreCase);
        private static int sizeThreshold = 0;
        private static int lowQuality = 50;
        private static int highQuality = 90;
        private static bool convertPng = false;
        private static bool convertBmp = true;
        private static bool convertGif = true;
        private static bool convertTiff = true;
        private static bool convertJpeg = true;
        private static int defaultWidth = 2560;

        private readonly IOrchardServices _services;
        internal static ImageResizerSettingsPart Settings = null;

        public HtmlFilterService(IOrchardServices services)
        {
            _services = services;
        }

        public string ProcessContent(string text, string flavor)
        {
            if (string.Equals(flavor, "Html", StringComparison.OrdinalIgnoreCase))
            {
                if (Settings == null)
                {
                    try
                    {
                        Settings = _services.WorkContext.CurrentSite.As<ImageResizerSettingsPart>();
                        if (Settings != null)
                        {
                            sizeThreshold = Settings.SizeThreshold;
                            lowQuality = Settings.LowQuality;
                            highQuality = Settings.HighQuality;
                            convertJpeg = Settings.ConvertJpeg;
                            convertPng = Settings.ConvertPng;
                            convertTiff = Settings.ConvertTiff;
                            convertBmp = Settings.ConvertBmp;
                            convertGif = Settings.ConvertGif;
                            defaultWidth = Settings.DefaultWidth;
                        }
                    }
                    catch
                    {
                        // do nothing
                    }
                }
                return HtmlExtReplace(text);

            }
            else
                return text;
        }

        private static string HtmlExtReplace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var nowhitespace = NormalizeSpaces(text);

            text = imgTagRe.Replace(nowhitespace, ResizeImage);

            return text;
        }

        private static string ResizeImage(Match match)
        {
            var str = match.Value;
            string height = null;
            string width = null;
            string quality = null;
            string widthUnits = null;

            var heightMatch = heightRe.Match(str);
            if (heightMatch.Success)
                height = heightMatch.Groups[1].Value;

            var widthMatch = widthRe.Match(str);
            if (widthMatch.Success)
            {
                width = widthMatch.Groups[1].Value;
                widthUnits = widthMatch.Groups[2].Value;
                if (widthUnits.Contains("%"))
                    width = null;
            }

            // Get specifiied quality : overrides anything else
            var qualityMatch = qualityRe.Match(str);
            if (qualityMatch.Success)
                quality = qualityMatch.Groups[1].Value;


            // if there is no width specified then apply the default width
            if (String.IsNullOrWhiteSpace(width))
                width = defaultWidth.ToString();

            // This is for Retina, but only if a quality wasn't specified.
            // Double the resolution of small images but reduce the quality
            // otherwise, for larger images use high quality but original res.
            if (String.IsNullOrEmpty(quality))
            {
                int int_width = int.Parse(width);
                if (int_width > sizeThreshold)
                {
                    quality = highQuality.ToString();
                }
                else
                {
                    quality = lowQuality.ToString();
                    width = (int_width * 2).ToString();
                    if (height != null)
                    {
                        int int_height = int.Parse(height);
                        height = (int_height * 2).ToString();
                    }
                }
            }

            return srcRe.Replace(str, m => ReplaceSrc(m, height, width, quality));
        }

        /// <summary>
        /// Strip any existing ImageResizer height and width parameters from the querystring 
        /// and add new ones derived from the img height and width attributes
        /// </summary>
        private static string ReplaceSrc(Match match, string height, string width, string quality)
        {
            // either "src" or "data-src"
            var attr = match.Groups[1].Value;

            string path = match.Groups[2].Value;
            if (string.IsNullOrWhiteSpace(path))
                return match.ToString();


            var parts = path.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            string url = parts.Length > 0 ? parts[0] : null;

            // ignore svg etc
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
                return match.ToString();

            var ext = Path.GetExtension(path); //.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext))
                return match.ToString();

            // ignore files according to settings
            if ((!convertJpeg && (String.Compare(".jpg", ext) == 0 || String.Compare(".jpeg", ext) == 0))
                || (!convertTiff && (String.Compare(".tif", ext) == 0 || String.Compare(".tiff", ext) == 0))
                || (!convertPng && String.Compare(".png", ext) == 0)
                || (!convertBmp && String.Compare(".bmp", ext) == 0)
                || (!convertGif && String.Compare(".gif", ext) == 0)
                )
                return match.ToString();

            string qs = null;
            if (parts.Length == 2)
            {
                string qsEnc = parts[1];
                qs = qsEnc.Replace("&amp;", "&");
            }

            List<string> components = new List<string>();
            if (qs != null)
            {
                components = qs.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(i => !(
                        i.StartsWith("h=", StringComparison.InvariantCultureIgnoreCase) ||
                        i.StartsWith("w=", StringComparison.InvariantCultureIgnoreCase) ||
                        i.StartsWith("height=", StringComparison.InvariantCultureIgnoreCase) ||
                        i.StartsWith("width=", StringComparison.InvariantCultureIgnoreCase) ||
                        i.StartsWith("quality=", StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
            }
            if (height != null)
                components.Add("height=" + height);
            if (width != null)
                components.Add("width=" + width);
            if (!components.Any(c => c.StartsWith("quality=")) && quality != null)
                components.Add("quality=" + quality);
            if (!components.Any(c => c.StartsWith("format=")) && ext != ".jpg" && ext != ".jpeg")
                components.Add("format=jpg");
            qs = string.Join("&", components);

            // Azure blob storage strings must be 'redirected' to a relative url for them to be processed by image resizer
            // assuming resizer is running on the same url as the website
            if (url.StartsWith("http") && url.Contains("blob.core.windows.net"))
            {
                var index = url.IndexOf("//") + 2;
                index = url.IndexOf('/', index);
                url = url.Substring(index);
            }

            if (string.IsNullOrWhiteSpace(qs))
                return " " + attr + "='" + url + "'";
            else
                return " " + attr + "='" + url + "?" + qs + "'";
        }


        // I could not figure out a regex to do this so I've made a little state machine
        // to strip excess spaces everywhere except at the start of lines (indenting) and 
        // between <pre> and </pre>
        private enum Mode { StartOfLine, Normal, IgnoreExtraSpaces, Pre };

        private static string NormalizeSpaces(string text)
        {
            string result = "";
            Mode mode = Mode.StartOfLine;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (mode)
                {
                    case Mode.StartOfLine:
                        result += c;
                        if (IsStartPre(i, text))
                            mode = Mode.Pre;
                        else if (!(c == '\n' || c == '\r' || c == ' '))
                            mode = Mode.Normal;
                        break;

                    case Mode.Normal:
                        result += c;
                        if (IsStartPre(i, text))
                            mode = Mode.Pre;
                        else if (c == ' ')
                            mode = Mode.IgnoreExtraSpaces;
                        else if (c == '\n' || c == '\r')
                            mode = Mode.StartOfLine;
                        break;

                    case Mode.IgnoreExtraSpaces:
                        if (c == '\n' || c == '\r')
                        {
                            result += c;
                            mode = Mode.StartOfLine;
                        }
                        else if (IsStartPre(i, text))
                        {
                            result += c;
                            mode = Mode.Pre;
                        }
                        else if (c != ' ')
                        {
                            result += c;
                            mode = Mode.Normal;
                        }
                        // else do nothing so white space char is skipped
                        break;

                    case Mode.Pre:
                        result += c;
                        if (IsEndPre(i, text))
                            mode = Mode.Normal;
                        break;

                }
            }
            return result;
        }
        private static bool IsStartPre(int i, string text)
        {
            return Test(i, text, "<pre>");
        }
        private static bool IsEndPre(int i, string text)
        {
            return Test(i, text, "</pre>");
        }
        private static bool Test(int i, string text, string tag)
        {
            var len = tag.Length;
            if (i + len > text.Length)
                return false;
            var snippet = text.Substring(i, len).ToLower();
            return snippet == tag;
        }

    }
}
