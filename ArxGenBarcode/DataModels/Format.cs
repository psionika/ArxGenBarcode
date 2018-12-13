using System.Drawing;
using System.IO;
using ZXing;

namespace ArxGenBarcode.DataModels
{
    public class Format
    {
        public bool IsAllow { get; set; }

        public string Description { get; set; }

        public string UrlToWiki { get; set; }

        public Bitmap Image { get; set; }

        public BarcodeFormat BarcodeFormat { get; set; }

        public Format(bool isAllow, BarcodeFormat barcodeFormat)
        {
            this.IsAllow = isAllow;
            this.BarcodeFormat = barcodeFormat;
        }

        public Format (BarcodeFormat barcodeFormat, string description, string urlToWiki, string image)
        {
            this.BarcodeFormat = barcodeFormat;
            this.Description = description;
            this.UrlToWiki = urlToWiki;
            this.Image = new Bitmap(1,1);

            if (File.Exists(image))
            {
                this.Image = new Bitmap(image);
            }
            
            
        }
    }
}
