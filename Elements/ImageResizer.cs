using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Cascade.ImageResizer.Elements
{
    public class ImageResizer : Element
    {
        public override string Category
        {
            get { return "Media"; }
        }

        public override string ToolboxIcon
        {
            get { return "\uf065"; }
        }

        public int? MediaId
        {
            get { return this.Retrieve(x => x.MediaId); }
            set { this.Store(x => x.MediaId, value); }
        }

        public int? Width
        {
            get { return this.Retrieve(x => x.Width); }
            set { this.Store(x => x.Width, value); }
        }
        public int? Height
        {
            get { return this.Retrieve(x => x.Height); }
            set { this.Store(x => x.Height, value); }
        }
        public string Etc
        {
            get { return this.Retrieve(x => x.Etc); }
            set { this.Store(x => x.Etc, value); }
        }
        public bool Responsive
        {
            get { return this.Retrieve(x => x.Responsive); }
            set { this.Store(x => x.Responsive, value); }
        }
    }
}