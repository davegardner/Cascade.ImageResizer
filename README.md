# Cascade.ImageResizer
Supports the use of ImageResizer as an HttpService, with automatic image resizing of Orchard Images.

After installing and enabling the module in Orchard, you will find a number of hopefully self-explanatory 
settings under Site Administration.

Cascade.Imageresizer filters html output. It scans html for img tags and inserts imageresizer query strings 
into the src attribute to optimize image size for display. This gives you image size independance. You can 
upload large images to the Media Library, resize them in the Html Editor to whatever size you want and image resizer will 
create an image of exactly the right size on demand, and optionally cache it.

# Important
To use, you must install Imageresizer (http://imageresizing.net) and configure your web.config appropriately.

# Licensing
If you wish to use caching, which is highly recommended, then you need to license the Disk Cache module from Imageresizing.

