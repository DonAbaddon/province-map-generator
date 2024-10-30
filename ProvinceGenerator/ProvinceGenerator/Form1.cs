using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProvinceGenerator
{
    public partial class Form1 : Form
    {
        private static readonly int OFFSET = 30;
        private static readonly double PROBABILITY = 0.1;
        private static readonly int LENGTH = 10;
        private static readonly Random rnd = new Random();

        private Bitmap template;
        private Bitmap map;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG Files(*.png)|*.png";
            if(openFileDialog.ShowDialog() == DialogResult.OK )
            {
                try
                {
                    template = new Bitmap(openFileDialog.FileName);
                    ShowPicture(template);
                }
                catch
                {
                    MessageBox.Show("Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowPicture(Bitmap bpm)
        {
            Bitmap toShow = Resize(bpm, 800, 800);
            pictureBox1.Image = toShow;
        }

        private Bitmap Resize(Bitmap bmp, int width, int height) 
        {
            try
            {
                Bitmap b = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, width, height);
                }
                return b;
            }
            catch
            {
                Console.WriteLine("Bitmap could not be resized");
                return bmp;
            }
        }

        private int[][] GenerateColors()
        {
            int size = 256 * 256 * 256 - 2;
            int[][] colors = new int[size][];
            for(int red = 0; red < 256; red++)
            {
                for(int green = 0; green < 256; green++)
                {
                    for(int blue = 0; blue < 256; blue++)
                    {
                        if(red + green + blue != 0 && red + green + blue != 255 * 3)
                        {
                            colors[red * 256 * 256 + green * 256 + blue - 1] = new[] { red, green, blue };
                        }
                    }
                }
            }
            for (int i = size - 1; i > 0; i--)
            {
                int j = rnd.Next(0, i + 1);
                // Меняем местами array[i] с element в random index
                int[] temp = colors[i];
                colors[i] = colors[j];
                colors[j] = temp;
            }
            return colors;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (template == null)
            {
                return;
            }

            Bitmap newMap = new Bitmap(template);

            int width = newMap.Width;
            int height = newMap.Height;

            int[][] colors = GenerateColors();

            int colorId = 0;
            StringBuilder content = new StringBuilder("0;0;0;0;x;x;\n");
            for(int x = 0; x < width; x += OFFSET)
            {
                for(int y = 0; y < height; y += OFFSET)
                {
                    Color pixel = newMap.GetPixel(x, y);
                    if(IsWhite(pixel))
                    {
                        int[] color = colors[colorId];
                        Color newColor = Color.FromArgb(color[0], color[1], color[2]);
                        double direction = rnd.NextDouble();
                        if (direction < 0.25)
                        {
                            for (int newx = x - LENGTH; newx <= x + LENGTH; newx++)
                            {
                                PaintPixel(newMap, newx, y, newColor);
                            }
                        }
                        else if(direction < 0.5)
                        {
                            for (int newy = y - LENGTH; newy <= y + LENGTH; newy++)
                            {
                                PaintPixel(newMap, x, newy, newColor);
                            }
                        }
                        else if(direction < 0.75)
                        {
                            for(int i = -LENGTH; i <= LENGTH; i++)
                            {
                                PaintPixel(newMap, x - i, y - i, newColor);
                            }
                        }
                        else
                        {
                            for (int i = -LENGTH; i <= LENGTH; i++)
                            {
                                PaintPixel(newMap, x + i, y - i, newColor);
                            }
                        }
                        colorId++;
                        string str = colorId.ToString() + ";" + newColor.R.ToString() + ";" + newColor.G.ToString() + ";" + newColor.B.ToString() + ";PROVINCE" + colorId.ToString() + ";x;\n";
                        content.Append(str);
                    }
                }
            }

            File.WriteAllText("definition.csv", content.ToString());

            map = newMap;
            ShowPicture(map);
        }

        private void Iteration()
        {
            int width = map.Width;
            int height = map.Height;

            Bitmap tempMap = new Bitmap(map);

            Rectangle rectangle = new Rectangle(0, 0, width, height);
            BitmapData mapData = map.LockBits(rectangle, ImageLockMode.ReadOnly, map.PixelFormat);
            BitmapData tempMapData = tempMap.LockBits(rectangle, ImageLockMode.WriteOnly, tempMap.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(map.PixelFormat) / 8;
            int stride = mapData.Stride;

            Parallel.For(0, width, (x) =>
            {
                Random innerRnd = new Random();
                for(int y = 0; y < height; y++)
                {
                    int pixel = PixelIndex(x, y, stride, bytesPerPixel);
                    if(IsWhite(ReadPixel(mapData, pixel)))
                    {
                        if(x > 0)
                        {
                            Color neighbour = ReadPixel(mapData, PixelIndex(x - 1, y, stride, bytesPerPixel));
                            if(!IsWhite(neighbour) && !IsBlack(neighbour) && innerRnd.NextDouble() < PROBABILITY)
                            {
                                WritePixel(tempMapData, pixel, neighbour);
                            }
                        }
                        if(x < width - 1)
                        {
                            Color neighbour = ReadPixel(mapData, PixelIndex(x + 1, y, stride, bytesPerPixel));
                            if (!IsWhite(neighbour) && !IsBlack(neighbour) && innerRnd.NextDouble() < PROBABILITY)
                            {
                                WritePixel(tempMapData, pixel, neighbour);
                            }
                        }
                        if(y > 0)
                        {
                            Color neighbour = ReadPixel(mapData, PixelIndex(x, y - 1, stride, bytesPerPixel));
                            if (!IsWhite(neighbour) && !IsBlack(neighbour) && innerRnd.NextDouble() < PROBABILITY)
                            {
                                WritePixel(tempMapData, pixel, neighbour);
                            }
                        }
                        if(y < height - 1)
                        {
                            Color neighbour = ReadPixel(mapData, PixelIndex(x, y + 1, stride, bytesPerPixel));
                            if (!IsWhite(neighbour) && !IsBlack(neighbour) && innerRnd.NextDouble() < PROBABILITY)
                            {
                                WritePixel(tempMapData, pixel, neighbour);
                            }
                        }
                    }
                }
            });

            map.UnlockBits(mapData);
            tempMap.UnlockBits(tempMapData);

            map.Dispose();
            map = tempMap;
        }

        private int PixelIndex(int x, int y, int stride, int bytesPerPixel)
        {
            return y * stride + x * bytesPerPixel;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Iteration();
            ShowPicture(map);
        }

        private Color ReadPixel(BitmapData data, int idx)
        {
            return Color.FromArgb(
                Marshal.ReadByte(data.Scan0, idx),
                Marshal.ReadByte(data.Scan0, idx + 1),
                Marshal.ReadByte(data.Scan0, idx + 2));
        }

        private void WritePixel(BitmapData data, int idx, Color color)
        {
            Marshal.WriteByte(data.Scan0, idx, color.R);
            Marshal.WriteByte(data.Scan0, idx + 1, color.G);
            Marshal.WriteByte(data.Scan0, idx + 2, color.B);
            Marshal.WriteByte(data.Scan0, idx + 3, 255);
        }

        private void PaintPixel(Bitmap bmp, int x, int y, Color color)
        {
            if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
            {
                Color pixel = bmp.GetPixel(x, y);
                if (IsWhite(pixel))
                {
                    bmp.SetPixel(x, y, color);
                }
            }
        }

        private void PaintPixel(Bitmap bmp, int x, int y, Color color, double probability)
        {
            if(x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
            {
                Color pixel = bmp.GetPixel(x, y);
                if(IsWhite(pixel))
                {
                    double ver = rnd.NextDouble();
                    if (ver < probability)
                    {
                        bmp.SetPixel(x, y, color);
                    }
                }
            }
        }

        private bool IsWhite(Color pixel)
        {
            return pixel.R == 255 && pixel.G == 255 && pixel.B == 255;
        }

        private bool IsBlack(Color pixel)
        {
            return pixel.R == 0 && pixel.G == 0 && pixel.B == 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            map.Save("provinces.png", ImageFormat.Png);
        }
    }
}
