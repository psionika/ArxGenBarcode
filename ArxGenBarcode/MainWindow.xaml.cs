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

            buttonGenerate_Click(this, null);            
        }

        private void FillComboBox()
        {
            comboBoxAllowFormat.ItemsSource = Current.Settings.PossibleFormats;
            comboBoxAllowFormat.SelectedIndex = 0;
        }

        private void SetImageBarcodeSource()
        {
            if (!string.IsNullOrEmpty(textBoxBarcode.Text))
            {
                var bitmapBarcode = Barcode.Generate(textBoxBarcode.Text,
                                                     (BarcodeFormat)comboBoxAllowFormat.SelectedItem,
                                                     GetNoiseValue(),
                                                     false);

                imageBarcode.Source = bitmapBarcode.ToWpfImage();

                listBoxBarcodeHistory.Items.Add(textBoxBarcode.Text);
            }
        }
        
        private void buttonGenerate_Click(object sender, RoutedEventArgs e)
        {
            noised = false;
            SetImageBarcodeSource();
        }

        private void textBoxBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(textBoxBarcode != null 
               && !string.IsNullOrEmpty(textBoxBarcode.Text) 
               && labelCount != null && labelCount.Content != null)
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
                Clipboard.SetText(textBoxBarcode.Text);
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

        private float GetNoiseValue()
        {
            if (!noised) return 0;

            string noiseString = textBoxNoise.Text;

            string uiSep = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            noiseString = noiseString.Replace(".", uiSep).Replace(",", uiSep);
            float noise = Convert.ToSingle(noiseString);

            return noise;
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
                textBoxBarcode.Text = listBoxBarcodeHistory.SelectedItem.ToString();

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
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    webCam.Source = bi;

                    
                }));
            }
            catch (Exception ex)
            {
            }
        }

        private void buttonStartWebCamCapture_Click(object sender, RoutedEventArgs e)
        {
            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(CamNewFrame);

            LocalWebCam.Start();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void buttonStopWebCam_Click(object sender, RoutedEventArgs e)
        {
            LocalWebCam.Stop();
        }

        private void buttonReadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "Image(*.PNG;*.JPG;*.GIF)|*.Png;*.JPG;*.GIF" + "|Все файлы (*.*)|*.* ";
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = false;

            if (myDialog.ShowDialog() == true)
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(myDialog.FileName);
                ProcessResult(Barcode.Decode(bitmap));
            }
        }

        private void parseImageFile(string filename)
        {
            try
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(filename);
                decode(bitmap);
            }
            catch (Exception)
            {
                throw new FileNotFoundException("Resource not found: " + filename);
            }
        }

        private void ProcessResult(Result result)
        {
            if (result != null)
            {

                comboBoxAllowFormat.SelectedItem = result.BarcodeFormat;
                textBoxBarcode.Text = result.Text;

                buttonGenerate_Click(this, null);
            }
            else
            {
                textBoxBarcode.Text = "error read barcode from image";
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private void buttonParseScreen_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            var allProcesses = Process.GetProcesses().ToList();

            const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
            const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

            const int VK_LWIN = 0x5B;
            const int VK_SHIFT = 0x10;
            const int S_KEY  = 0x53;

            keybd_event(VK_LWIN, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(S_KEY, 0, KEYEVENTF_EXTENDEDKEY, 0);
            Thread.Sleep(50);
            keybd_event(S_KEY, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);

            //TODO!!!
            bool scissorsRuning = true;

            do
            {
                Thread.Sleep(150);

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
