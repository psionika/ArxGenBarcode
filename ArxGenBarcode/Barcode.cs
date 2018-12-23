using ArxGenBarcode.Helpers;
using System;
using System.Drawing;
using System.Windows;
using ZXing;

namespace ArxGenBarcode
{
    public static class Barcode
    {
        private static double currentRotate = 0;

        public static Bitmap Generate(string barcode, BarcodeFormat barcodeFormat)
        {
            return Generate(barcode, barcodeFormat, 0, false);
        }

        public static Bitmap Generate(string barcode, BarcodeFormat barcodeFormat, float noise, bool rotate = false, bool blur = false)
        {
            var bitmap = new Bitmap(1, 1);

            try
            {
                var writer = new BarcodeWriter
                {
                    Format = barcodeFormat
                };

                bitmap = writer.Write(barcode);

                if (rotate)
                {
                    switch (currentRotate)
                    {
                        case 0:
                            currentRotate = 90;
                            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 90:
                            currentRotate = 180;
                            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 180:
                            currentRotate = 270;
                            bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case 270:
                            currentRotate = 0;
                            break;
                    }
                }
                else currentRotate = 0;

                if (noise != 0)
                {
                    bitmap = Helpers.ForImage.AddSpeckleNoise(bitmap, noise);
                }

                if(blur)
                {
                   bitmap = ForImage.Blur(bitmap, 2);
                }
            }
            catch (Exception ex)
            {
                //TODO
                MessageBox.Show(ex.Message);
            }

            return bitmap;
        }

        public static Result Decode(Bitmap image)
        {
            using (image)
            {
                BarcodeReader reader = new BarcodeReader();

                reader.Options.PureBarcode = true;
                reader.Options.PossibleFormats = Current.Settings.PossibleFormats;
                reader.Options.TryHarder = true;

                reader.AutoRotate = true;
                reader.TryInverted = true;

                return reader.Decode(image);
            }
        }
    }
}
