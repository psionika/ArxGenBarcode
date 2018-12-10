using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using System.Collections.Generic;
using ArxGenBarcode.DataModels;

namespace ArxGenBarcode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool noised = false;

        public MainWindow()
        {
            InitializeComponent();
            FillComboBox();

            SetImageBarcodeSource();
        }

        private void FillComboBox()
        {
            comboBoxAllowFormat.ItemsSource = Current.Settings.PossibleFormats;
            comboBoxAllowFormat.SelectedIndex = 0;

            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            comboBoxListWebCam.ItemsSource = LoaclWebCamsCollection;
            comboBoxListWebCam.SelectedIndex = 0;
        }

        private void SetImageBarcodeSource()
        {
            if (!string.IsNullOrEmpty(textBoxBarcode.Text))
            {
                var format = (BarcodeFormat)comboBoxAllowFormat.SelectedItem;

                var bitmapBarcode = Barcode.Generate(textBoxBarcode.Text,
                                                     format,
                                                     GetNoiseValue(),
                                                     false);

                imageBarcode.Source = bitmapBarcode.ToWpfImage();

                AddToHistory(textBoxBarcode.Text, format);
            }
        }

        private void AddToHistory(string barcode, BarcodeFormat format)
        {
            if (listBoxBarcodeHistory.Items.Count > 0)
            {
                var selected = listBoxBarcodeHistory.Items.Cast<HistoryElement>().Last();

                if (selected.Barcode != barcode
                    || selected.Format != format)
                {
                    listBoxBarcodeHistory.Items.Add(new HistoryElement(DateTime.Now, format, barcode));
                }
            }
            else
            {
                listBoxBarcodeHistory.Items.Add(new HistoryElement(DateTime.Now, format, barcode));
            }            
        }

        private void ProcessResult(Result result)
        {
            if (result != null)
            {
                textBoxBarcode.Text = result.Text;
                comboBoxAllowFormat.SelectedItem = result.BarcodeFormat;

                buttonGenerate_Click(this, null);
            }
            else
            {
                textBoxBarcode.Text = "error read barcode from image";
            }
        }

        private float GetNoiseValue()
        {
            if (!noised) return 0;

            string noiseString = textBoxNoise.Text;

            string uiSep = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            noiseString = noiseString.Replace(".", uiSep).Replace(",", uiSep);
            float noise = Convert.ToSingle(noiseString);

            return noise;
        }

        private void buttonGenerate_Click(object sender, RoutedEventArgs e)
        {
            noised = false;
            SetImageBarcodeSource();
        }

        private void textBoxBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!string.IsNullOrEmpty(textBoxBarcode.Text) 
               && labelCount != null 
               && labelCount.Content != null)
            {
                labelCount.Content = textBoxBarcode?.Text?.Length.ToString();
            }                
        }

        private void buttonPrint_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bi = (BitmapImage)imageBarcode.Source; 

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = bi;
            image.Height = 200;            
            var containerImage = new InlineUIContainer(image);

            var pd = new PrintDialog();

            if (pd.ShowDialog() == true)
            {
                FlowDocument fd = new FlowDocument();
                fd.Blocks.Add(new Paragraph(new Run("")));

                var paragraphTitle = new Paragraph();
                paragraphTitle.Inlines.Add(new Bold(new Run("ArxGenBarcode")));
                paragraphTitle.Inlines.Add(new Run("\n" + DateTime.Now.ToLongTimeString() + ", " + DateTime.Now.ToLongDateString()));

                fd.Blocks.Add(paragraphTitle);

                fd.Blocks.Add(new Paragraph(containerImage));

                fd.Blocks.Add(new Paragraph(new Run("Data: "    + textBoxBarcode.Text)));

                fd.Blocks.Add(new Paragraph(new Run("Format: "  + comboBoxAllowFormat.SelectedItem)));

                fd.Blocks.Add(new Paragraph(new Run("Comment: " + textBoxComment.Text)));

                pd.PrintDocument((fd as IDocumentPaginatorSource).DocumentPaginator, "A print document");
            }

        }

        private void buttonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxBarcode.Text))
            {
                Clipboard.SetText(textBoxBarcode.Text);
            }                
        }

        private void buttonPasteFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textBoxBarcode.Text = Clipboard.GetText();
            }
            catch{ }            
        }

        private void buttonManageBarcodeFormatList_Click(object sender, RoutedEventArgs e)
        {
            var windowListFormat = new WindowListFormat();
            windowListFormat.ShowDialog();

            FillComboBox();
        }

        private void ButtonNoise_Click(object sender, RoutedEventArgs e)
        {
            noised = true;

            SetImageBarcodeSource();
        }

        private void buttonRotate_Click(object sender, RoutedEventArgs e)
        {
            var bitmapBarcode = Barcode.Generate(textBoxBarcode.Text, (BarcodeFormat)comboBoxAllowFormat.SelectedItem, GetNoiseValue(), true);
            imageBarcode.Source = bitmapBarcode.ToWpfImage();
        }

        private void buttonHistoryListClear_Click(object sender, RoutedEventArgs e)
        {
            listBoxBarcodeHistory.Items.Clear();
        }

        private void buttonHistoryListSelectItem_Click(object sender, RoutedEventArgs e)
        {
            if(listBoxBarcodeHistory.Items.Count > 0 
               && listBoxBarcodeHistory.SelectedItem != null)
            {
                var selected = listBoxBarcodeHistory.SelectedItems.Cast<HistoryElement>().First();

                comboBoxAllowFormat.SelectedItem = selected.Format;

                textBoxBarcode.Text = selected.Barcode.ToString();

                SetImageBarcodeSource();
            }
        }

        private void buttonBarcodeToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if(imageBarcode != null)
            {
                var image = imageBarcode.Source as BitmapImage;
                Clipboard.SetImage(image);
            }
        }

        VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LoaclWebCamsCollection;

        void CamNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();

                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();

                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    imageWebCam.Source = bi;
                    
                    var result = Barcode.Decode(bi.GetBitmap());
                    if(result != null && !string.IsNullOrEmpty(result.Text))
                    {
                        ProcessResult(result);

                        LocalWebCam.Stop();
                    }
                }));
            }
            catch (Exception ex)
            {
            }
        }

        private void buttonStartWebCamCapture_Click(object sender, RoutedEventArgs e)
        {
            var device = ((FilterInfo)comboBoxListWebCam.SelectedItem).MonikerString;
            
            LocalWebCam = new VideoCaptureDevice(device);
            LocalWebCam.DesiredFrameSize = new System.Drawing.Size(640, 480);
            LocalWebCam.DesiredFrameRate = 10;
            LocalWebCam.NewFrame += new NewFrameEventHandler(CamNewFrame);

            LocalWebCam.Start();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocalWebCam != null)
            {
                LocalWebCam.Stop();

                if (comboBoxListWebCam != null
               && comboBoxListWebCam.Items.Count > 0)
                {
                    LocalWebCam = (VideoCaptureDevice)comboBoxListWebCam.SelectedItem;
                }
            }
        }

        private void buttonStopWebCam_Click(object sender, RoutedEventArgs e)
        {
            if(LocalWebCam != null)
            {
                LocalWebCam.Stop();
                imageWebCam.Source = null;
            }
                
        }

        private void buttonReadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "Image(*.PNG;*.BMP;*.JPG;*.GIF)|*.Png;*.BMP;*.JPG;*.GIF" + "|All files (*.*)|*.* ";
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = false;

            if (myDialog.ShowDialog() == true)
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(myDialog.FileName);
                ProcessResult(Barcode.Decode(bitmap));
            }
        }

        private void buttonBarcodeImageToFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();

            dlg.FileName = "Image";
            dlg.DefaultExt = ".png";
            dlg.Filter = "Image file (.png)|*.png";

            var result = dlg.ShowDialog();

            if (result == true)
            {
                var bitmapBarcode = Barcode.Generate(textBoxBarcode.Text, (BarcodeFormat)comboBoxAllowFormat.SelectedItem, GetNoiseValue(), false);
                
                bitmapBarcode.Save(dlg.FileName, ImageFormat.Png);
            }
        }

        private void buttonParseClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var image = Clipboard.GetImage();
                var bitmap = image.GetBitmap();
                imageBarcode.Source = bitmap.ToWpfImage();
                ProcessResult(Barcode.Decode(bitmap));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonParseScreen_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            var allProcesses = Process.GetProcesses().ToList();

            Helpers.PressKeysToScissors();

            //TODO!!!
            bool scissorsRuning = true;

            do
            {
                var lisss = Process.GetProcesses().Except(allProcesses).ToList();

                if (!lisss.Any(x => x.ProcessName == "SnippingTool"))
                {
                    scissorsRuning = false;
                }                               
            }
            while (scissorsRuning);

            try
            {
                var image = Clipboard.GetImage();
                var bitmap = image.GetBitmap();
                imageBarcode.Source = bitmap.ToWpfImage();

                ProcessResult(Barcode.Decode(bitmap));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.WindowState = WindowState.Normal;
            }            
        }

        private void buttonHistoryRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = listBoxBarcodeHistory.SelectedItems.Cast<Object>().ToArray();

            foreach (var item in selected)
            {
                listBoxBarcodeHistory.Items.Remove(item);
            }                
        }

        private void comboBoxAllowFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetImageBarcodeSource();
        }
    }
}
