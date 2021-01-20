using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
 
using System.IO;
 
using System.Threading;
 
using System.Windows.Media.Imaging;
 
using System.Windows.Threading;
using System.Diagnostics;


namespace image_pro
{
    public class UiModel : INotifyPropertyChanged
    {
        #region filds

        private double myVar00 = 0;
        private double myVar01 = -1;
        private double myVar02 = 0;
        private double myVar10 = -1;
        private double myVar11 = 5;
        private double myVar12 = -1;
        private double myVar20 = 0;
        private double myVar21 = -1;
        private double myVar22 = 0;

        #endregion

        #region prop


        public double MyProperty00
        {
            get { return myVar00; }
            set
            {

                myVar00 = value;
                PropertyChangedFunc(nameof(MyProperty00));
            }
        }
        public double MyProperty01
        {
            get { return myVar01; }
            set
            {

                myVar01 = value;
                PropertyChangedFunc(nameof(MyProperty01));
            }
        }

        public double MyProperty02
        {
            get { return myVar02; }
            set
            {

                myVar02 = value;
                PropertyChangedFunc(nameof(MyProperty02));
            }
        }
        public double MyProperty10
        {
            get { return myVar10; }
            set
            {

                myVar10 = value;
                PropertyChangedFunc(nameof(MyProperty10));
            }
        }
        public double MyProperty11
        {
            get { return myVar11; }
            set
            {

                myVar11 = value;
                PropertyChangedFunc(nameof(MyProperty11));
            }
        }

        public double MyProperty12
        {
            get { return myVar12; }
            set
            {

                myVar12 = value;
                PropertyChangedFunc(nameof(MyProperty12));
            }
        }

        public double MyProperty20
        {
            get { return myVar20; }
            set
            {

                myVar20 = value;
                PropertyChangedFunc(nameof(MyProperty20));
            }
        }

        public double MyProperty21
        {
            get { return myVar21; }
            set
            {

                myVar21 = value;
                PropertyChangedFunc(nameof(MyProperty21));
            }
        }

        public double MyProperty22
        {
            get { return myVar22; }
            set
            {

                myVar22 = value;
                PropertyChangedFunc(nameof(MyProperty22));
            }
        }


        #endregion
        private double strength;

        public double Strength
        {
            get { return strength; }
            set {

                strength = value;
                PropertyChangedFunc(nameof(Strength));
            }
        }

        public RelayCommand OpenImage { get; set; }
        public RelayCommand Run { get; set; }

        private BitmapImage bitmapImage;
        public BitmapImage BitmapImage
        {
            get { return bitmapImage; }
            set
            {
                bitmapImage = value;
                PropertyChangedFunc(nameof(BitmapImage));
            }
        }

        public UiModel()
        {
            OpenImage = new RelayCommand(OpenImageFun);
            Run = new RelayCommand(RunAction);
        }
        public BitmapImage ConvertBitmap(System.Drawing.Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }

        private void RunAction(object obj)
        {
            double[,] mask = new double[,] {
                {myVar00, myVar01, myVar02 },
                {myVar10, myVar11, myVar12 },
                { myVar20 ,myVar21, myVar22},
            };
            //BitmapImage = null;
            // var i =  NonfftSharpen(bitmap, mask, 1);
            BitmapImage = ConvertBitmap(NonfftSharpen(bitmap, mask, strength));
                

        }
        Bitmap bitmap;
        private void OpenImageFun(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                 bitmap = new Bitmap(openFileDialog.FileName);
                  BitmapImage = ConvertBitmap(bitmap);
            }
        }
        public static Bitmap NonfftSharpen(Bitmap image, double[,] mask, double strength)
        {
            Bitmap bitmap = (Bitmap)image.Clone();

            if (bitmap != null)
            {
                int width = bitmap.Width;
                int height = bitmap.Height;

                if (mask.GetLength(0) != mask.GetLength(1))
                {
                    throw new Exception("_numericalKernel dimensions must be same");
                }
                // Create sharpening filter.
                int filterSize = mask.GetLength(0);

                double[,] filter = (double[,])mask.Clone();

                int channels = sizeof(byte);
                double bias = 1.0 - strength;
                double factor = strength / 16.0;
                int halfOfFilerSize = filterSize / 2;

                byte[,] result = new byte[bitmap.Width, bitmap.Height];

                // Lock image bits for read/write.

                BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                                                            ImageLockMode.ReadWrite,
                                                            System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                // Declare an array to hold the bytes of the bitmap.
                int memorySize = bitmapData.Stride * height;
                byte[] memory = new byte[memorySize];

                // Copy the RGB values into the local array.
                Marshal.Copy(bitmapData.Scan0, memory, 0, memorySize);

                int rgb;
                // Fill the color array with the new sharpened color values.

                for (int y = halfOfFilerSize; y < height - halfOfFilerSize; y++)
                {
                    for (int x = halfOfFilerSize; x < width - halfOfFilerSize; x++)
                    {
                        for (int filterY = 0; filterY < filterSize; filterY++)
                        {
                            double grayShade = 0.0;

                            for (int filterX = 0; filterX < filterSize; filterX++)
                            {
                                int imageX = (x - halfOfFilerSize + filterX + width) % width;
                                int imageY = (y - halfOfFilerSize + filterY + height) % height;

                                rgb = imageY * bitmapData.Stride + channels * imageX;

                                grayShade += memory[rgb + 0] * filter[filterX, filterY];
                            }

                            rgb = y * bitmapData.Stride + channels * x;

                            int b = Math.Min(Math.Max((int)(factor * grayShade + (bias * memory[rgb + 0])), 0), 255);

                            result[x, y] = (byte)b;
                        }
                    }
                }

                // Update the image with the sharpened pixels.
                for (int x = halfOfFilerSize; x < width - halfOfFilerSize; x++)
                {
                    for (int y = halfOfFilerSize; y < height - halfOfFilerSize; y++)
                    {
                        rgb = y * bitmapData.Stride + channels * x;

                        memory[rgb + 0] = result[x, y];
                    }
                }

                // Copy the RGB values back to the bitmap.
                Marshal.Copy(memory, 0, bitmapData.Scan0, memorySize);

                // Release image bits.
                bitmap.UnlockBits(bitmapData);

                return bitmap;

            }
            else
            {
                throw new Exception("input image can't be null");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void PropertyChangedFunc(string propNmae)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propNmae));
        }
    }
}
