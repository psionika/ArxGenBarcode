using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using ZXing;

namespace ArxGenBarcode
{
    public class Current
    {
        #region Constants

        private string Filename
        {
            get
            {
                string filename = Path.Combine(
                                                Path.GetDirectoryName
                                                (Assembly.GetEntryAssembly().Location),
                                                "settings.xml");

                return filename;
            }
        }

        #endregion

        #region Constructor

        public static Current Settings
        {
            get
            {
                return new Current();
            }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Current()
        {

        }

        private Current()
        {
            ReadSettings();
        }

        #endregion

        #region Action

        private void ReadSettings()
        {
            if (!File.Exists(Filename)) return;

            var doc = XDocument.Load(Filename);
            if (doc.Root == null) return;

            foreach (var format in doc.Root.Element(nameof(PossibleFormats))?.Elements())
            {
                if (!string.IsNullOrEmpty(format.Value))
                {
                    if (Enum.TryParse(format.Value, out BarcodeFormat myStatus))
                        possibleFormats.Add(myStatus);
                }
            }
        }

        public void WriteSettings()
        {
            var listFormat = (from p in PossibleFormats select new XElement("Format", p)).ToList();

            var doc =
                new XDocument(
                    new XElement("Current",
                        new XElement("PossibleFormats", listFormat)));

            doc.Save(Filename);
        }

        #endregion

        #region Properties

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
                WriteSettings();
            }
        }
        
        #endregion
    }
}
