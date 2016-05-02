using Orchard.MediaLibrary.Models;

namespace Cascade.ImageResizer.ViewModels {
    public class ImageResizerEditorViewModel {
        public string ImageId { get; set; }
        public ImagePart CurrentImage { get; set; }
        public int? Width { get; set; }
        public int? Height{ get; set; }
        public string Etc{ get; set; }
        public bool Responsive { get; set; }
    }
}