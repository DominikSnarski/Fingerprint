using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fingerprint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<int> deletionList;

        public MainWindow()
        {
            InitializeComponent();

            deletionList = new List<int>{3, 5, 7, 12, 13, 14, 15, 20,
21, 22, 23, 28, 29, 30, 31, 48,
52, 53, 54, 55, 56, 60, 61, 62,
63, 65, 67, 69, 71, 77, 79, 80,
81, 83, 84, 85, 86, 87, 88, 89,
91, 92, 93, 94, 95, 97, 99, 101,
103, 109, 111, 112, 113, 115, 116, 117,
118, 119, 120, 121, 123, 124, 125, 126,
127, 131, 133, 135, 141, 143, 149, 151,
157, 159, 181, 183, 189, 191, 192, 193,
195, 197, 199, 205, 207, 208, 209, 211,
212, 213, 214, 215, 216, 217, 219, 220,
221, 222, 223, 224, 225, 227, 229, 231,
237, 239, 240, 241, 243, 244, 245, 246,
247, 248, 249, 251, 252, 253, 254, 255 };
        }


        private void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Uri fileUri = new Uri(openFileDialog.FileName);
                    Img.Source = new BitmapImage(fileUri);
                    ImgPrev.Source = new BitmapImage(fileUri);
                }
                catch
                {
                    Img.Source = null;
                    ImgPrev.Source = null;
                }
            }
        }

        private byte[] To_Grey_Scale(byte[] pixels)
        {
            int NumberOfColorPixels = pixels.Length / 4;

            byte[] r = new byte[NumberOfColorPixels];
            byte[] g = new byte[NumberOfColorPixels];
            byte[] b = new byte[NumberOfColorPixels];
            byte[] f = new byte[NumberOfColorPixels];

            int i = 0;
            int ri = 0;
            int gi = 0;
            int bi = 0;
            int fi = 0;

            foreach (byte byt in pixels)
            {
                if (i % 4 == 0) b[bi++] = byt;
                else if (i % 4 == 1) g[gi++] = byt;
                else if (i % 4 == 2) r[ri++] = byt;
                else if (i % 4 == 3) f[fi++] = byt;
                i = (i + 1) % 4;
            }

            //Przejscie do skali szarosci dla kanalu R+g+b/3
            for (i = 0; i < r.Length; i++)
            {
                //  r[i] = (byte)((r[i] / 3) + (b[i] / 3) + (g[i] / 3));
                //Debug.WriteLine(r[i]);
            }

            return r;
        }

        private void Binarize_OTSU(object sender, RoutedEventArgs e)
        {
            if (Img.Source == null || ImgPrev.Source == null)
            {
                MessageBox.Show("You have to load image.");
                return;
            }

            WriteableBitmap wb = new WriteableBitmap(Img.Source as BitmapSource);
            int stride = (int)wb.PixelWidth * ((wb.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[(int)wb.PixelHeight * stride];
            int NumberOfPixels = ((wb.Format.BitsPerPixel + 7) / 8);
            if (NumberOfPixels != 4)
            {
                MessageBox.Show("Sorry this image pixel format is not supported.");
                return;
            }
            int NumberOfColorPixels = pixels.Length / NumberOfPixels;//pixels.Length / 4;

            wb.CopyPixels(pixels, stride, 0);
            byte[] kanal = new byte[NumberOfColorPixels];

            kanal = To_Grey_Scale(pixels);

            int i = 0;
            int ri = 0;

            byte[] r = kanal;

            //Sumy pixeli

            int[] rd = new int[256];


            foreach (byte byt in r)
            {
                rd[byt]++;
            };

            //Otsu

            int[] ots = new int[256];

            double[] wariancje = new double[256];
            int suma = 0;
            foreach (int x in rd) suma = suma + x;
            double[] srednie = new double[256];

            for (int w = 0; w < srednie.Length; w++)
            {
                double rdd = (double)rd[w];
                double sum = (double)suma;
                srednie[w] = rdd / sum;
            }
            for (int q = 0; q < ots.Length; q++)
            {
                //Sumy
                int suma0 = 0, suma1 = 0;
                for (int w = 0; w < rd.Length; w++)
                {
                    if (w < q) suma0 = suma0 + rd[w];
                    else suma1 = suma1 + rd[w];

                }

                //Srednie
                double sr0 = 0, sr1 = 0;

                sr0 = ((double)suma0) / ((double)suma);
                sr1 = ((double)suma1) / ((double)suma);

                //
                double u0 = 0, u1 = 0;
                for (int w = 0; w < srednie.Length; w++)
                {
                    if (w < q && sr0 != 0) u0 = u0 + (((double)w) * srednie[w] / sr0);
                    else if (sr1 != 0) u1 = u1 + (((double)w) * srednie[w] / sr1);
                }

                //Wariancje
                double w0 = 0, w1 = 0;
                for (int w = 0; w < srednie.Length; w++)
                {
                    if (w < q && sr0 != 0) w0 = w0 + ((Math.Pow((((double)w) - u0), 2) * srednie[w] / sr0));
                    else if (sr1 != 0) w1 = w1 + ((Math.Pow((((double)w) - u1), 2) * srednie[w] / sr1));
                }

                wariancje[q] = (sr0 * w0) + (sr1 * w1);
                // Debug.WriteLine("W[{0}] = {1}", q, wariancje[q]);
            }

            //Znalezienie progu
            int p = 0;
            double min = wariancje[0];
            for (i = 0; i < wariancje.Length; i++)
            {
                if (wariancje[i] < min)
                {
                    min = wariancje[i];
                    p = i;
                }
            }

            // Debug.WriteLine("Prog = {0}", p);
            //LUT

            int[] lut = new int[256];


            for (int x = 0; x <= 255; x++)
            {
                if (x > p) lut[x] = 255;
                else lut[x] = 0;
            }

            //Binaryzacja
            for (int x = 0; x < kanal.Length; x++)
            {
                kanal[x] = (byte)lut[kanal[x]];
            }

            SetImage(wb, stride, pixels, kanal);
        }

        private void Filter_Median_3(object sender, RoutedEventArgs e)
        {
            if (Img.Source == null || ImgPrev.Source == null)
            {
                MessageBox.Show("You have to load image.");
                return;
            }

            WriteableBitmap wb = new WriteableBitmap(Img.Source as BitmapSource);
            int stride = (int)wb.PixelWidth * ((wb.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[(int)wb.PixelHeight * stride];
            int NumberOfPixels = ((wb.Format.BitsPerPixel + 7) / 8);
            if (NumberOfPixels != 4)
            {
                MessageBox.Show("Sorry this image pixel format is not supported.");
                return;
            }
            int NumberOfColorPixels = pixels.Length / NumberOfPixels;//pixels.Length / 4;

            wb.CopyPixels(pixels, stride, 0);
            byte[] kanal = new byte[NumberOfColorPixels];

            kanal = To_Grey_Scale(pixels);

            int i = 0;
            int ri = 0;

            int h = wb.PixelHeight, w = wb.PixelWidth;
            //Przejscie do tablicy dwuwymiarowej
            byte[,] table = new byte[h, w];
            i = 0;
            for (int row = 0; row < h; row++)
            {
                for (int col = 0; col < w; col++)
                {
                    table[row, col] = kanal[i++];
                    //Debug.Write(table[row, col].ToString()+" ");
                }
                //Debug.WriteLine("");
            }

            i = 0;

            //Filtrowanie tablicy dwuwymiarowej
            for (int row = 0; row < h; row++)
            {
                for (int col = 0; col < w; col++)
                {
                    List<byte> chosen = new List<byte>();
                    for (int a = 0; a < 3; a++)
                        for (int aa = 0; aa < 3; aa++)
                        {
                            if ((row + (a - 1)) < 0 || (row + (a - 1)) >= h || (col + (aa - 1)) < 0 || (col + (aa - 1)) >= w) chosen.Add(0);
                            else chosen.Add(table[row + (a - 1), col + (aa - 1)]);
                        }
                    chosen.Sort();
                    kanal[i++] = chosen[chosen.Count / 2];

                }
            }

            //Stworzenie array obrazu

            SetImage(wb, stride, pixels, kanal);
        }

        private void Thinning(object sender, RoutedEventArgs e)
        {
            if (Img.Source == null || ImgPrev.Source == null)
            {
                MessageBox.Show("You have to load image.");
                return;
            }

            WriteableBitmap wb = new WriteableBitmap(Img.Source as BitmapSource);
            int stride = (int)wb.PixelWidth * ((wb.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[(int)wb.PixelHeight * stride];
            int NumberOfPixels = ((wb.Format.BitsPerPixel + 7) / 8);
            if (NumberOfPixels != 4)
            {
                MessageBox.Show("Sorry this image pixel format is not supported.");
                return;
            }
            int NumberOfColorPixels = pixels.Length / NumberOfPixels;//pixels.Length / 4;

            wb.CopyPixels(pixels, stride, 0);
            byte[] kanal = new byte[NumberOfColorPixels];

            kanal = To_Grey_Scale(pixels);

            int i = 0;

            int[] prev = new int[5] { 0, 0, 0, 0, 0 };
            int[] now = new int[5] { 0, 0, 0, 0, 0 };

            //Przejscie do tablicy dwuwymiarowej
            byte[,] table = new byte[wb.PixelHeight, wb.PixelWidth];
            i = 0;
            for (int row = 0; row < wb.PixelHeight; row++)
            {
                for (int col = 0; col < wb.PixelWidth; col++)
                {
                    table[row, col] = kanal[i++];
                    //Debug.Write(table[row, col].ToString()+" ");
                }
                //Debug.WriteLine("");
            }


            byte[,] table_KKM = new byte[wb.PixelHeight, wb.PixelWidth];


            table_KKM = BlackAsOne(table, wb.PixelHeight, wb.PixelWidth);

            while (true)
            {

                //Zaznaczenie 1 pixeli które stykają się krawędzią z 0 jako 2
                table_KKM = OneAsTwo(table_KKM, wb.PixelHeight, wb.PixelWidth);

                //Zaznaczenie 1 pixeli które stykają się rogiem z 0 jako 3
                table_KKM = OneAsThree(table_KKM, wb.PixelHeight, wb.PixelWidth);

                //Zaznaczenie pixeli na krawedzi z 2,3,4 sąsiadami jako 4

                table_KKM = FindFours(table_KKM, wb.PixelHeight, wb.PixelWidth);


                //Usuniecie pixeli o wartosci 4
                table_KKM = RemoveFours(table_KKM, wb.PixelHeight, wb.PixelWidth);

                for (int n = 2; n <= 3; n++)
                {
                    //Obliczenia wag 
                    table_KKM = CalculateWeights(table_KKM, wb.PixelHeight, wb.PixelWidth, n);
                }

                //Obliczenia ilości różnych cyfer
                for (int row = 0; row < wb.PixelHeight; row++)
                {
                    for (int col = 0; col < wb.PixelWidth; col++)
                    {
                        if (table_KKM[row, col] == 0) now[0]++;
                        if (table_KKM[row, col] == 1) now[1]++;
                        if (table_KKM[row, col] == 2) now[2]++;
                        if (table_KKM[row, col] == 3) now[3]++;
                        if (table_KKM[row, col] == 4) now[4]++;
                    }
                }
                Debug.WriteLine("{0} {1} {2} {3} {4}", now[0], now[1], now[2], now[3], now[4]);
                //Czy szkielet jest jedno pixelowy? (Czy ilości cyfer sie zmieniły?)
                if (now[0] != prev[0] || now[1] != prev[1] || now[2] != prev[2] || now[3] != prev[3] || now[4] != prev[4])
                {
                    prev[0] = now[0];
                    prev[1] = now[1];
                    prev[2] = now[2];
                    prev[3] = now[3];
                    prev[4] = now[4];

                    now[0] = 0;
                    now[1] = 0;
                    now[2] = 0;
                    now[3] = 0;
                    now[4] = 0;
                }
                else break;


            }

            //Zamiana tablicy na array
            i = 0;
            for (int row = 0; row < wb.PixelHeight; row++)
            {
                for (int col = 0; col < wb.PixelWidth; col++)
                {
                    //Debug.WriteLine(table_KKM[row, col]);
                    if (table_KKM[row, col] > 0)
                        kanal[i++] = 0;
                    else
                        kanal[i++] = 255;
                }
            }

            SetImage(wb, stride, pixels, kanal);

        }

        private void CrossingNumber(object sender, RoutedEventArgs e)
        {
            if (Img.Source == null || ImgPrev.Source == null)
            {
                MessageBox.Show("You have to load image.");
                return;
            }

            WriteableBitmap wb = new WriteableBitmap(Img.Source as BitmapSource);
            int stride = (int)wb.PixelWidth * ((wb.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[(int)wb.PixelHeight * stride];
            int NumberOfPixels = ((wb.Format.BitsPerPixel + 7) / 8);
            if (NumberOfPixels != 4)
            {
                MessageBox.Show("Sorry this image pixel format is not supported.");
                return;
            }
            int NumberOfColorPixels = pixels.Length / NumberOfPixels;//pixels.Length / 4;

            wb.CopyPixels(pixels, stride, 0);
            byte[] kanal = new byte[NumberOfColorPixels];

            kanal = To_Grey_Scale(pixels);

            int i = 0;

            //Przejscie do tablicy dwuwymiarowej
            byte[,] table = new byte[wb.PixelHeight, wb.PixelWidth];
            i = 0;
            for (int row = 0; row < wb.PixelHeight; row++)
            {
                for (int col = 0; col < wb.PixelWidth; col++)
                {
                    if (kanal[i] == 0)
                        table[row, col] = 1;
                    else if (kanal[i] > 0)
                        table[row, col] = 0;

                    i++;
                    //Debug.Write(table[row, col].ToString()+" ");
                }
                //Debug.WriteLine("");
            }

            int[] detected = new int[10];

            i = 0;
            for (int row = 0; row < wb.PixelHeight; row++)
            {
                for (int col = 0; col < wb.PixelWidth; col++)
                {
                    if (table[row, col] == 1)
                    {
                        if (row > 0 && col > 0 && row < wb.PixelHeight - 1 && col < wb.PixelWidth - 1)
                        {
                            int cn = (Math.Abs(table[row, col + 1] - table[row - 1, col + 1]) +
                                Math.Abs(table[row - 1, col + 1] - table[row - 1, col]) +
                                Math.Abs(table[row - 1, col] - table[row - 1, col - 1]) +
                                Math.Abs(table[row - 1, col - 1] - table[row, col - 1]) +
                                Math.Abs(table[row, col - 1] - table[row + 1, col - 1]) +
                                Math.Abs(table[row + 1, col - 1] - table[row + 1, col]) +
                                Math.Abs(table[row + 1, col] - table[row + 1, col + 1]) +
                                Math.Abs(table[row + 1, col + 1] - table[row, col + 1])
                                ) / 2;

                            //Debug.WriteLine(cn);
                            detected[cn]++;
                            if (cn == 0) kanal[i] = 50;
                            else if (cn == 1) kanal[i] = 100;
                            else if (cn == 3) kanal[i] = 150;
                            else if (cn == 4) kanal[i] = 200;
                        }
                    }
                    i++;


                }
            }

            //Zamiana array na obraz
            i = 0;
            int zmienna = 0;
            for (int x = 0; x < pixels.Length; x++)
            {

                    if (i % 4 == 0) pixels[x] = (byte)kanal[zmienna];
                    else if (i % 4 == 1) pixels[x] = (byte)kanal[zmienna];
                    else if (i % 4 == 2) pixels[x] = (byte)kanal[zmienna++];
                    //else if (i % 4 == 3) pixels[x] = (byte)pixels;

                    i = (i + 1) % 4;
                
            }

            for (int x = 0; x < pixels.Length; x+=4)
            {
                if (pixels[x] != 0 && pixels[x] != 255)
                {
                    if (pixels[x] == 50)
                    {
                        pixels[x] = 20;
                        pixels[x + 1] = 36;
                        pixels[x + 2] = 217;

                    }
                    else if (pixels[x] == 100)
                    {
                        pixels[x] = 44;
                        pixels[x + 1] = 186;
                        pixels[x + 2] = 39;

                    }
                    else if (pixels[x] == 150)
                    {
                        pixels[x] = 196;
                        pixels[x + 1] = 69;
                        pixels[x + 2] = 14;

                    }
                    else if (pixels[x] == 200)
                    {

                        pixels[x] = 224;
                        pixels[x + 1] = 20;
                        pixels[x + 2] = 227;

                    }
                }

                }

            Debug.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                detected[0], detected[1], detected[2], detected[3], detected[4], detected[5], detected[6], detected[7], detected[8], detected[9]);
                //Wgranie obrazu do image
                Int32Rect rect = new Int32Rect(
                     0,
                     0,
                     wb.PixelWidth,
                     wb.PixelHeight);



                wb.WritePixels(rect, pixels, stride, 0);
                Img.Source = wb;

            for (int x = 0; x < pixels.Length; x++)
            {
                if (pixels[x] == 0) pixels[x] = 255;
            }

            WriteableBitmap OnlyPoints = new WriteableBitmap(ImgPrev.Source as BitmapSource);
            OnlyPoints.WritePixels(rect, pixels, stride, 0);
            ImgPrev.Source = OnlyPoints;

        }

            private byte[,] BlackAsOne(byte[,] table, int h, int w)
            {

                byte[,] table_KKM = new byte[h, w];

                //Zaznaczenie czarnych pixeli jako 1
                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        if (table[row, col] == 0)
                        {
                            table_KKM[row, col] = 1;
                        }
                        else
                        {
                            table_KKM[row, col] = 0;
                        }
                        // Debug.Write(table[row, col].ToString()+" ");
                    }
                    //Debug.WriteLine("");
                }

                return table_KKM;

            }

            private byte[,] OneAsTwo(byte[,] table_KKM, int h, int w)
            {
                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        if (table_KKM[row, col] == 1)
                        {
                            if (row > 0)
                                if (table_KKM[row - 1, col] == 0)
                                {
                                    table_KKM[row, col] = 2;
                                    continue;
                                }
                            if (row < h - 1)
                                if (table_KKM[row + 1, col] == 0)
                                {
                                    table_KKM[row, col] = 2;
                                    continue;
                                }
                            if (col > 0)
                                if (table_KKM[row, col - 1] == 0)
                                {
                                    table_KKM[row, col] = 2;
                                    continue;
                                }
                            if (col < w - 1)
                                if (table_KKM[row, col + 1] == 0)
                                {
                                    table_KKM[row, col] = 2;
                                    continue;
                                }
                        }
                        //Debug.Write(table[row, col].ToString()+" ");
                    }
                    //Debug.WriteLine("");
                }

                return table_KKM;
            }

            private byte[,] OneAsThree(byte[,] table_KKM, int h, int w)
            {
                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        if (table_KKM[row, col] == 1)
                        {
                            if (row > 0 && col > 0)
                            {
                                if (table_KKM[row - 1, col - 1] == 0)
                                {
                                    table_KKM[row, col] = 3;
                                    continue;
                                }
                            }
                            if (row > 0 && col < w - 1)
                            {
                                if (table_KKM[row - 1, col + 1] == 0)
                                {
                                    table_KKM[row, col] = 3;
                                    continue;
                                }
                            }
                            if (row < h - 1 && col > 0)
                            {
                                if (table_KKM[row + 1, col - 1] == 0)
                                {
                                    table_KKM[row, col] = 3;
                                    continue;
                                }
                            }
                            if (row < h - 1 && col < w - 1)
                            {
                                if (table_KKM[row + 1, col + 1] == 0)
                                {
                                    table_KKM[row, col] = 3;
                                    continue;
                                }
                            }

                        }
                        //Debug.Write(table[row, col].ToString()+" ");
                    }
                    //Debug.WriteLine("");
                }

                return table_KKM;
            }

            private byte[,] FindFours(byte[,] table_KKM, int h, int w)
            {
                List<int> dlist = new List<int> { 3, 6, 12, 24, 48, 96, 192, 129, 7, 14, 28, 56, 112, 224, 193, 131, 15, 30, 60, 120, 240, 225, 195, 135 };

                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        if (table_KKM[row, col] > 1)
                        {
                            //Calculate the weight of pixel
                            int sum = 0;
                            //Sprawdzenie po krawedziach
                            if (row > 0)
                                if (table_KKM[row - 1, col] > 0)
                                {
                                    sum += 1;
                                }
                            if (row < h - 1)
                                if (table_KKM[row + 1, col] > 0)
                                {
                                    sum += 16;
                                }
                            if (col > 0)
                                if (table_KKM[row, col - 1] > 0)
                                {
                                    sum += 64;
                                }
                            if (col < w - 1)
                                if (table_KKM[row, col + 1] > 0)
                                {
                                    sum += 4;
                                }

                            //Sprawdzenie po skosach

                            if (row > 0 && col > 0)
                                if (table_KKM[row - 1, col - 1] > 0)
                                {
                                    sum += 128;
                                }
                            if (row > 0 && col < w - 1)
                                if (table_KKM[row - 1, col + 1] > 0)
                                {
                                    sum += 2;
                                }
                            if (row < h - 1 && col > 0)
                                if (table_KKM[row + 1, col - 1] > 0)
                                {
                                    sum += 32;
                                }
                            if (row < h - 1 && col < w - 1)
                                if (table_KKM[row + 1, col + 1] > 0)
                                {
                                    sum += 8;
                                }

                            //Debug.WriteLine(sum);
                            if (dlist.Contains(sum))
                            {
                                table_KKM[row, col] = 4;
                            }
                        }


                    }
                }

                return table_KKM;
            }

            private byte[,] RemoveFours(byte[,] table_KKM, int h, int w)
            {
                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        if (table_KKM[row, col] == 4)
                            table_KKM[row, col] = 0;
                        //Debug.Write(table_KKM[row, col].ToString()+" ");
                    }
                    //Debug.WriteLine("");
                }

                return table_KKM;
            }

            private byte[,] CalculateWeights(byte[,] table_KKM, int h, int w, int n)
            {

                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        if (table_KKM[row, col] == n)
                        {
                            //Calculate the weight of pixel
                            int sum = 0;
                            //Sprawdzenie po krawedziach
                            if (row > 0)
                                if (table_KKM[row - 1, col] > 0)
                                {
                                    sum += 1;
                                }
                            if (row < h - 1)
                                if (table_KKM[row + 1, col] > 0)
                                {
                                    sum += 16;
                                }
                            if (col > 0)
                                if (table_KKM[row, col - 1] > 0)
                                {
                                    sum += 64;
                                }
                            if (col < w - 1)
                                if (table_KKM[row, col + 1] > 0)
                                {
                                    sum += 4;
                                }

                            //Sprawdzenie po skosach

                            if (row > 0 && col > 0)
                                if (table_KKM[row - 1, col - 1] > 0)
                                {
                                    sum += 128;
                                }
                            if (row > 0 && col < w - 1)
                                if (table_KKM[row - 1, col + 1] > 0)
                                {
                                    sum += 2;
                                }
                            if (row < h - 1 && col > 0)
                                if (table_KKM[row + 1, col - 1] > 0)
                                {
                                    sum += 32;
                                }
                            if (row < h - 1 && col < w - 1)
                                if (table_KKM[row + 1, col + 1] > 0)
                                {
                                    sum += 8;
                                }
                            //Debug.WriteLine(sum);
                            if (deletionList.Contains(sum))
                            {
                                table_KKM[row, col] = 0;
                            }
                            else
                            {
                                table_KKM[row, col] = 1;
                            }
                        }



                    }
                }

                return table_KKM;
            }
            private void SetImage(WriteableBitmap wb, int stride, byte[] pixels, byte[] kanal)
            {
                //Zamiana array na obraz
                int i = 0;
                int zmienna = 0;
                for (int x = 0; x < pixels.Length; x++)
                {
                    if (i % 4 == 0) pixels[x] = (byte)kanal[zmienna];
                    else if (i % 4 == 1) pixels[x] = (byte)kanal[zmienna];
                    else if (i % 4 == 2) pixels[x] = (byte)kanal[zmienna++];
                    //else if (i % 4 == 3) pixels[x] = (byte)pixels;
                    i = (i + 1) % 4;
                }

                //Wgranie obrazu do image
                Int32Rect rect = new Int32Rect(
                         0,
                         0,
                         wb.PixelWidth,
                         wb.PixelHeight);



                wb.WritePixels(rect, pixels, stride, 0);
                Img.Source = wb;
            }
        }
    }
