using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ZXing;
using ArxGenBarcode.DataModels;

namespace ArxGenBarcode
{
    /// <summary>
    /// Interaction logic for WindowListFormat.xaml
    /// </summary>
    public partial class WindowListFormat : Window
    {
        public WindowListFormat()
        {
            InitializeComponent();

            var allFormat = new List<Format>();

            foreach (BarcodeFormat format in (BarcodeFormat[])Enum.GetValues(typeof(BarcodeFormat)))
            {
                bool allow = Current.Settings.PossibleFormats.Contains(format);

                allFormat.Add(new Format(allow, format));
            }

            listBoxAllFormats.ItemsSource = allFormat;        
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Current.Settings.PossibleFormats = listBoxAllFormats.Items.Cast<Format>()
                                                       .Where(x => x.IsAllow)
                                                       .Select(x => x.BarcodeFormat)
                                                       .ToList();

            Close();
        }
    }
}
