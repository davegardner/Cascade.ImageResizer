using System;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
using ImageResizer;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.MediaProcessing.Descriptors.Filter;
using Orchard.MediaProcessing.Services;
using Orchard.Utility.Extensions;

namespace Cascade.ImageResizer.Provider.Filter
{
    public class FormatAndResizeFilter : IImageFilterProvider {
        public FormatAndResizeFilter()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Transform", T("Transform"), T("Transform"))
                .Element("FormatAndResize", T("Format and Resize"), T("A single step filter that can change format as well as resize (preferred)."),
                         ApplyFilter,
                         DisplayFilter,
                         "FormatAndResizeFilter"
                );
        }

        public void ApplyFilter(FilterContext context) {
            int width = context.State.Width;
            int height = context.State.Height;
            string mode = context.State.Mode;
            string alignment = context.State.Alignment;
            string padcolor = context.State.PadColor;
            int quality = context.State.Quality;
            string format = context.State.Format;

            // Set the format and extension correctly
            // ImageResizer automagically changes the format from tiff to jpeg
            if (string.Compare(format, "(any)", true) == 0)
                if (string.Compare(Path.GetExtension(context.FilePath), ".tif", true) == 0)
                    format = "jpg";
                else
                    format = Path.GetExtension(context.FilePath);
            context.FilePath = Path.ChangeExtension(context.FilePath, format);
            
            var settings = new ResizeSettings {
                Mode = FitMode.Max,
                Height = height,
                Width = width,
                Format = format,
                Quality = quality
            };

            switch (mode) {
                case "max": settings.Mode = FitMode.Max; break;
                case "pad": settings.Mode = FitMode.Pad; break;
                case "crop": settings.Mode = FitMode.Crop; break;
                case "stretch": settings.Mode = FitMode.Stretch; break;
            }

            switch (alignment) {
                case "topleft": settings.Anchor = ContentAlignment.TopLeft; break;
                case "topcenter": settings.Anchor = ContentAlignment.TopCenter; break;
                case "topright": settings.Anchor = ContentAlignment.TopRight; break;
                case "middleleft": settings.Anchor = ContentAlignment.MiddleLeft; break;
                case "middlecenter": settings.Anchor = ContentAlignment.MiddleCenter; break;
                case "middleright": settings.Anchor = ContentAlignment.MiddleRight; break;
                case "bottomleft": settings.Anchor = ContentAlignment.BottomLeft; break;
                case "bottomcenter": settings.Anchor = ContentAlignment.BottomCenter; break;
                case "bottomright": settings.Anchor = ContentAlignment.BottomRight; break;
            }

            if (!String.IsNullOrWhiteSpace(padcolor)) {
                if (padcolor.StartsWith("#")) {
                    settings.BackgroundColor = ColorTranslator.FromHtml(padcolor);
                }
                else {
                    settings.BackgroundColor = Color.FromName(padcolor);
                }
            }

            var result = new MemoryStream();
            if (context.Media.CanSeek) {
                context.Media.Seek(0, SeekOrigin.Begin);
            }
            ImageBuilder.Current.Build(context.Media, result, settings);
            context.Media = result;
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            string mode = context.State.Mode;

            switch (mode) {
                case "pad": return T("Format to {0} and Pad to {1}x{2}", context.State.Format, context.State.Height, context.State.Width);
                case "crop": return T("Format to {0} and Crop to {1}x{2}", context.State.Format, context.State.Height, context.State.Width);
                case "stretch": return T("Format to {0} and Stretch to {1}x{2}", context.State.Format, context.State.Height, context.State.Width);
                default: return T("Format to {0} and resize to Max {1}x{2}", context.State.Format, context.State.Height, context.State.Width); 

            } 
        }
    }

    public class FormatAndResizeFilterForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public FormatAndResizeFilterForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = Shape.Form(
                        Id: "ImageFormatAndResizeFilter",
                        _Width: Shape.Textbox(
                            Id: "width", Name: "Width",
                            Title: T("Width"),
                            Value: 0,
                            Description: T("The width in pixels."),
                            Classes: new[] {"text-small"}),
                        _Height: Shape.Textbox(
                            Id: "height", Name: "Height",
                            Title: T("Height"),
                            Value: 0,
                            Description: T("The height in pixels."),
                            Classes: new[] {"text-small"}),
                        _Mode: Shape.SelectList(
                            Id: "mode", Name: "Mode",
                            Title: T("Mode"),
                            Description: T("How the image should be resized.<br/>Max: adjusts to the max given width or left, keeping image ratio.<br/>Pad: adds a padding so that the target image is exactly of width and height.<br/>Crop: removes part of the image to fit with given height and width.<br/>Stretch: stretches the image to fit within height and width."),
                            Size: 1,
                            Multiple: false),
                        _Alignment: Shape.SelectList(
                            Id: "alignment", Name: "Alignment",
                            Title: T("Alignment"),
                            Description: T("Select the alignment for Crop and Pad modes."),
                            Size: 1,
                            Multiple: false),
                        _PadColor: Shape.Textbox(
                            Id: "padcolor", Name: "PadColor",
                            Title: T("Pad Color"),
                            Value: "#ffffff",
                            Description: T("The background color to use to pad the image. Named color or hex value."),
                            Classes: new[] {"text-small"}),
                        _Format: Shape.SelectList(
                            Id: "format", Name: "Format",
                            Title: T("Format"),
                            Description: T("The format of the displayed image (usually JPG).<br/>Selecting (any) will keep the existing format, except for TIF files which are always changed to JPG."),
                            Size: 1,
                            Multiple: false),
                        _Quality: Shape.Textbox(
                            Id: "quality", Name: "Quality",
                            Title: T("Quality"),
                            Value: 90,
                            Description: T("JPEG image quality (0-100). The higher the quality the larger the image file.")),
                            Classes: new[] {"text-small"}
                        );

                    f._Format.Add(new SelectListItem { Value = "jpg", Text = T("JPG").Text });
                    f._Format.Add(new SelectListItem { Value = "png", Text = T("PNG").Text });
                    f._Format.Add(new SelectListItem { Value = "gif", Text = T("GIF").Text });
                    f._Format.Add(new SelectListItem { Value = "(any)", Text = T("(any)").Text });

                    f._Mode.Add(new SelectListItem { Value = "max", Text = T("Max").Text });
                    f._Mode.Add(new SelectListItem { Value = "pad", Text = T("Pad").Text });
                    f._Mode.Add(new SelectListItem { Value = "crop", Text = T("Crop").Text });
                    f._Mode.Add(new SelectListItem { Value = "stretch", Text = T("Stretch").Text });

                    f._Alignment.Add(new SelectListItem { Value = "topleft", Text = T("Top Left").Text });
                    f._Alignment.Add(new SelectListItem { Value = "topcenter", Text = T("Top Center").Text });
                    f._Alignment.Add(new SelectListItem { Value = "topright", Text = T("Top Right").Text });
                    f._Alignment.Add(new SelectListItem { Value = "middleleft", Text = T("Middle Left").Text });
                    f._Alignment.Add(new SelectListItem { Value = "middlecenter", Text = T("Middle Center").Text });
                    f._Alignment.Add(new SelectListItem { Value = "middleright", Text = T("Middle Right").Text });
                    f._Alignment.Add(new SelectListItem { Value = "bottomleft", Text = T("Bottom Left").Text });
                    f._Alignment.Add(new SelectListItem { Value = "bottomcenter", Text = T("Bottom Center").Text });
                    f._Alignment.Add(new SelectListItem { Value = "bottomright", Text = T("Bottom Right").Text });

                    return f;
                };

            context.Form("FormatAndResizeFilter", form);
        }
    }
}