using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace ArxGenBarcode
{
    public class Settings
    {
        private List<BarcodeFormat> possibleFormats = new List<BarcodeFormat>();
        public List<BarcodeFormat> PossibleFormats
        {
            get
            {
                if(possibleFormats.Count == 0)
                {
                    possibleFormats.Add(BarcodeFormat.DATA_MATRIX);
                    possibleFormats.Add(BarcodeFormat.PDF_417);
                    possibleFormats.Add(BarcodeFormat.AZTEC);

                    possibleFormats.Add(BarcodeFormat.CODE_128);
                }

                return possibleFormats;
            }
            set
            {
                possibleFormats = value;
            }
        }
    }
}
