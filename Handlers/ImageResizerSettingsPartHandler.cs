using Cascade.ImageResizer.Models;
using Cascade.ImageResizer.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Localization;

namespace Cascade.ImageResizer.Handlers
{

    public class ImageResizerSettingsPartHandler : ContentHandler
    {
        public ImageResizerSettingsPartHandler() 
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ImageResizerSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<ImageResizerSettingsPart>("ImageResizerSettings", "Parts/ImageResizer.ImageResizerSettings", "ImageResizer"));

            // reset cache
            OnUpdated<ImageResizerSettingsPart>((context, part) => HtmlFilterService.Settings = null);
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("ImageResizer")));
        }
    }

}

