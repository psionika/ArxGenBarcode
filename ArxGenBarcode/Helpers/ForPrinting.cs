using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using ZXing;

namespace ArxGenBarcode.Helpers
{
    public class ForPrinting
    {
        BitmapImage BarcodeImage { get; set; }
        string BarcodeText { get; set; }
        BarcodeFormat Format { get; set; }
        string Comment { get; set; }

        public ForPrinting(BitmapImage barcodeImage, string barcodeText, BarcodeFormat format, string comment)
        {
            this.BarcodeImage = barcodeImage;
            this.BarcodeText = barcodeText;
            this.Format = format;
            this.Comment = comment;                
        }

        public void Print()
        {
            var pd = new PrintDialog();

            if (pd.ShowDialog() == true)
            {
                FlowDocument fd = GenerateFlowDocument();
                pd.PrintDocument((fd as IDocumentPaginatorSource).DocumentPaginator, "A print document");
            }
        }

        public void ExportToXPS()
        {
            var dlg = new SaveFileDialog();

            dlg.FileName = "Export barcode";
            dlg.DefaultExt = ".xps";
            dlg.Filter = "XPS document (.xps)|*.xps";

            var result = dlg.ShowDialog();

            if (result == true)
            {
                FlowDocument fd = GenerateFlowDocument();
                SaveAsXps(dlg.FileName, fd);
            }
        }

        public void ExportToPDF()
        {
            var dlg = new SaveFileDialog();

            dlg.FileName = "Export barcode";
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "pdf document (.pdf)|*.pdf";

            var result = dlg.ShowDialog();

            if (result == true)
            {
                string tempFileName = Path.Combine(Path.GetTempPath(), "tempExportBarcode" + DateTime.Now.Ticks + ".xps");

                FlowDocument fd = GenerateFlowDocument();

                SaveAsXps(tempFileName, fd);

                PdfSharp.Xps.XpsConverter.Convert(tempFileName, dlg.FileName, 0);

                File.Delete(tempFileName);
            }           
        }

        public static void SaveAsXps(string path, FlowDocument document)
        {
            using (Package package = Package.Open(path, FileMode.Create))
            {
                using (var xpsDoc = new XpsDocument(package, CompressionOption.Normal))
                {
                    var xpsSm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    DocumentPaginator dp = ((IDocumentPaginatorSource)document).DocumentPaginator;

                    xpsSm.SaveAsXaml(dp);
                }
            }
        }

        public FlowDocument GenerateFlowDocument()
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = this.BarcodeImage;
            image.Height = 200;
            var containerImage = new InlineUIContainer(image);

            FlowDocument fd = new FlowDocument();
            fd.Blocks.Add(new Paragraph(new Run("")));

            var paragraphTitle = new Paragraph();

            paragraphTitle.Inlines.Add(new Bold(new Run("ArxGenBarcode")));
            paragraphTitle.Inlines.Add(new Run("\n" + DateTime.Now.ToLongTimeString() + ", " + DateTime.Now.ToLongDateString()));

            fd.Blocks.Add(paragraphTitle);

            fd.Blocks.Add(new Paragraph(containerImage));

            fd.Blocks.Add(new Paragraph(new Run("Data: " + this.BarcodeText)));

            fd.Blocks.Add(new Paragraph(new Run("Format: " + this.Format)));

            fd.Blocks.Add(new Paragraph(new Run("Comment: " + this.Comment)));

            return fd;
        }

    }
}
