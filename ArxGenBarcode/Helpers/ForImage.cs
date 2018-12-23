﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ArxGenBarcode.Helpers
{
    public static class ForImage
    {
        public static BitmapImage Convert(this Bitmap src)
        {
            var image = new BitmapImage();
            using (var ms = new MemoryStream())
            {
                src.Save(ms, ImageFormat.Bmp);

                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }

            return image;
        }

        public static BitmapImage ToWpfImage(this Image img)
        {
            using (var memoryStream = new MemoryStream())
            {
                img.Save(memoryStream, ImageFormat.Png);

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memoryStream;
                image.EndInit();
                return image;
            }
        }

        public static Bitmap AddSpeckleNoise(Bitmap bmp, float v = 0.04f)
        {
            var res = (Bitmap)bmp.Clone();
            var rnd = new Random();
            var stdDev = Math.Sqrt(v);//девиация - корень из дисперсии

            using (var wr = new ImageWrapper(res))
                foreach (var p in wr)
                {
                    var c = wr[p];
                    var noise = 2 * (rnd.NextDouble() - 0.5f) * 1.7 * stdDev;//равномерное распр со средним = 0, дисперсия = v
                    wr.SetPixel(p, c.R + noise * c.R, c.G + noise * c.G, c.B + noise * c.B);//Id=Is+n*Is
                }

            return res;
        }

        public static Bitmap Blur(Bitmap image, Int32 blurSize)
        {
            return Blur(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
        }

        private static Bitmap Blur(Bitmap image, Rectangle rectangle, Int32 blurSize)
        {
            Bitmap blurred = new Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = Graphics.FromImage(blurred))
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            // look at every pixel in the blur rectangle
            for (int xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (int yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    int avgR = 0, avgG = 0, avgB = 0;
                    int blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (int x = xx; (x < xx + blurSize && x < image.Width); x++)
                    {
                        for (int y = yy; (y < yy + blurSize && y < image.Height); y++)
                        {
                            Color pixel = blurred.GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (int x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                        for (int y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                            blurred.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

            return blurred;
        }


        public static Bitmap GetBitmap(this BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new Rectangle(Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              System.Windows.Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        /// <summary>
        /// Обертка над Bitmap для быстрого чтения и изменения пикселов.
        /// Также, класс контролирует выход за пределы изображения: при чтении за границей изображения - 
        /// возвращает DefaultColor, при записи за границей изображения - игнорирует присвоение.
        /// </summary>
        public class ImageWrapper : IDisposable, IEnumerable<Point>
        {
            /// <summary>
            /// Ширина изображения
            /// </summary>
            public int Width { get; private set; }

            /// <summary>
            /// Высота изображения
            /// </summary>
            public int Height { get; private set; }

            /// <summary>
            /// Цвет по-умолачнию (используется при выходе координат за пределы изображения)
            /// </summary>
            public Color DefaultColor { get; set; }

            private byte[] data;//буфер исходного изображения
            private byte[] outData;//выходной буфер
            private int stride;
            private BitmapData bmpData;
            private Bitmap bmp;

            /// <summary>
            /// Создание обертки поверх bitmap.
            /// </summary>
            /// <param name="copySourceToOutput">Копирует исходное изображение в выходной буфер</param>
            public ImageWrapper(Bitmap bmp, bool copySourceToOutput = false)
            {
                Width = bmp.Width;
                Height = bmp.Height;
                this.bmp = bmp;

                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                stride = bmpData.Stride;

                data = new byte[stride * Height];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, data, 0, data.Length);

                outData = copySourceToOutput ? (byte[])data.Clone() : new byte[stride * Height];
            }

            /// <summary>
            /// Возвращает пиксел из исходнго изображения.
            /// Либо заносит пиксел в выходной буфер.
            /// </summary>
            public Color this[int x, int y]
            {
                get
                {
                    var i = GetIndex(x, y);
                    return i < 0 ? DefaultColor : Color.FromArgb(data[i + 3], data[i + 2], data[i + 1], data[i]);
                }

                set
                {
                    var i = GetIndex(x, y);
                    if (i >= 0)
                    {
                        outData[i] = value.B;
                        outData[i + 1] = value.G;
                        outData[i + 2] = value.R;
                        outData[i + 3] = value.A;
                    };
                }
            }

            /// <summary>
            /// Возвращает пиксел из исходнго изображения.
            /// Либо заносит пиксел в выходной буфер.
            /// </summary>
            public Color this[Point p]
            {
                get { return this[p.X, p.Y]; }
                set { this[p.X, p.Y] = value; }
            }

            /// <summary>
            /// Заносит в выходной буфер значение цвета, заданные в double.
            /// Допускает выход double за пределы 0-255.
            /// </summary>
            public void SetPixel(Point p, double r, double g, double b)
            {
                if (r < 0) r = 0;
                if (r >= 256) r = 255;
                if (g < 0) g = 0;
                if (g >= 256) g = 255;
                if (b < 0) b = 0;
                if (b >= 256) b = 255;

                this[p.X, p.Y] = Color.FromArgb((int)r, (int)g, (int)b);
            }

            int GetIndex(int x, int y)
            {
                return (x < 0 || x >= Width || y < 0 || y >= Height) ? -1 : x * 4 + y * stride;
            }

            /// <summary>
            /// Заносит в bitmap выходной буфер и снимает лок.
            /// Этот метод обязателен к исполнению (либо явно, лмбо через using)
            /// </summary>
            public void Dispose()
            {
                System.Runtime.InteropServices.Marshal.Copy(outData, 0, bmpData.Scan0, outData.Length);
                bmp.UnlockBits(bmpData);
            }

            /// <summary>
            /// Перечисление всех точек изображения
            /// </summary>
            public IEnumerator<Point> GetEnumerator()
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        yield return new Point(x, y);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Меняет местами входной и выходной буферы
            /// </summary>
            public void SwapBuffers()
            {
                var temp = data;
                data = outData;
                outData = temp;
            }
        }
    }
}
