using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Cascade.ImageResizer.ViewModels;
using Orchard.MediaLibrary.Models;
using Elements = Cascade.ImageResizer.Elements;
using ContentItem = Orchard.Layouts.Elements.ContentItem;

namespace Cascade.ImageResizer.Drivers
{
    public class ImageResizerElementDriver : ElementDriver<Elements.ImageResizer>
    {
        private readonly IContentManager _contentManager;

        public ImageResizerElementDriver(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(Elements.ImageResizer element, ElementEditorContext context)
        {

            var viewModel = new ImageResizerEditorViewModel();
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.ImageResizer", Model: viewModel);

            if (context.Updater != null)
            {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.MediaId = ParseImageId(viewModel.ImageId);
                element.Width = viewModel.Width;
                element.Height = viewModel.Height;
                element.Etc = viewModel.Etc;
                element.Responsive = viewModel.Responsive;
            }

            var imageId = element.MediaId;
            if (imageId == null)
            {
                viewModel.CurrentImage = default(ImagePart);
                viewModel.Responsive = true;
            }
            else
            {
                viewModel.CurrentImage = GetImage(imageId.Value);
            }
            viewModel.Width = element.Width;
            viewModel.Height = element.Height;
            viewModel.Etc = element.Etc;
            viewModel.Responsive = element.Responsive;

            return Editor(context, editor);
        }

        protected override void OnDisplaying(Elements.ImageResizer element, ElementDisplayingContext context)
        {
            var imageId = element.MediaId;
            var image = imageId != null ? GetImage(imageId.Value) : default(ImagePart);
            context.ElementShape.ImagePart = image;
            context.ElementShape.Width = element.Width;
            context.ElementShape.Height= element.Height;
            context.ElementShape.Etc = element.Etc ;
            context.ElementShape.Responsive = element.Responsive ;
        }

        protected override void OnExporting(Elements.ImageResizer element, ExportElementContext context)
        {
            var image = element.MediaId != null ? GetImage(element.MediaId.Value) : default(ImagePart);

            if (image == null)
                return;

            context.ExportableData["ImageResizer"] = _contentManager.GetItemMetadata(image).Identity.ToString();
        }

        protected override void OnImporting(Elements.ImageResizer element, ImportElementContext context)
        {
            var imageIdentity = context.ExportableData.Get("ImageResizer");
            var image = !String.IsNullOrWhiteSpace(imageIdentity) ? context.Session.GetItemFromSession(imageIdentity) : default(Orchard.ContentManagement.ContentItem);

            element.MediaId = image != null ? image.Id : default(int?);
        }

        protected ImagePart GetImage(int id)
        {
            return _contentManager.Get<ImagePart>(id, VersionOptions.Published);
        }

        private static int? ParseImageId(string imageId)
        {
            return ContentItem.Deserialize(imageId).FirstOrDefault();
        }
    }
}