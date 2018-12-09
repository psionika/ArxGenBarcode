using ZXing;

namespace ArxGenBarcode.DataModels
{
    public class Format
    {
        public bool IsAllow { get; set; }

        public BarcodeFormat BarcodeFormat { get; set; }

        public Format(bool isAllow, BarcodeFormat barcodeFormat)
        {
            this.IsAllow = isAllow;
            this.BarcodeFormat = barcodeFormat;
        }
    }
}
