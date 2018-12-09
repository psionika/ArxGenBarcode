using System;
using ZXing;

namespace ArxGenBarcode.DataModels
{
    public class HistoryElement
    {
        public DateTime CreatedAt { get; set; }
        public BarcodeFormat Format { get; set; }
        public string Barcode { get; set; }

        public HistoryElement(DateTime createdAt, BarcodeFormat format, string barcode)
        {
            this.CreatedAt = createdAt;
            this.Format = format;
            this.Barcode = barcode;
        }
    }
}
