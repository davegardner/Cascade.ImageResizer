using Orchard.ContentManagement;

namespace Cascade.ImageResizer.Models
{
    public class ImageResizerSettingsPart : ContentPart
    {
        public int DefaultWidth
        {
            get { return this.Retrieve(x => x.DefaultWidth, 1170); }
            set { this.Store(x => x.DefaultWidth, value); }
        }
        public int SizeThreshold
        {
            get { return this.Retrieve(x => x.SizeThreshold, 800); }
            set { this.Store(x => x.SizeThreshold, value); }
        }
        public int HighQuality
        {
            get { return this.Retrieve(x => x.HighQuality, 80); }
            set { this.Store(x => x.HighQuality, value); }
        }
        public int LowQuality
        {
            get { return this.Retrieve(x => x.LowQuality, 80); }
            set { this.Store(x => x.LowQuality, value); }
        }
        public bool ConvertPng
        {
            get { return this.Retrieve(x => x.ConvertPng, false); }
            set { this.Store(x => x.ConvertPng, value); }
        }
        public bool ConvertTiff
        {
            get { return this.Retrieve(x => x.ConvertTiff, true); }
            set { this.Store(x => x.ConvertTiff, value); }
        }
        public bool ConvertJpeg
        {
            get { return this.Retrieve(x => x.ConvertJpeg, true); }
            set { this.Store(x => x.ConvertJpeg, value); }
        }
        public bool ConvertGif
        {
            get { return this.Retrieve(x => x.ConvertGif, false); }
            set { this.Store(x => x.ConvertGif, value); }
        }
        public bool ConvertBmp
        {
            get { return this.Retrieve(x => x.ConvertBmp, true); }
            set { this.Store(x => x.ConvertBmp, value); }
        }

    }
}
