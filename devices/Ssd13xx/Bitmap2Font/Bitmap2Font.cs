using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using System.Windows.Forms;


namespace Bitmap2Font
{
    public partial class Bitmap2Font : Form
    {
        public Bitmap2Font()
        {
            InitializeComponent();
        }

        Bitmap myBitmap = new Bitmap(1, 1);
        string ImageIdentification = "";
        string ImageColorDepth = "";
        string ImageDimensions = "";
        string ImageName = "banana";
        List<string> CSfile = new List<string>();

        public static ImageType GetFileImageTypeFromHeader(string file) // from StackOverFlow
        
        {
            byte[] headerBytes;
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                const int mostBytesNeeded = 11;//For JPEG
                if (fileStream.Length < mostBytesNeeded)
                    return ImageType.Unknown;
                headerBytes = new byte[mostBytesNeeded];
                fileStream.Read(headerBytes, 0, mostBytesNeeded);
            }

            //Sources:
            //http://stackoverflow.com/questions/9354747
            //http://en.wikipedia.org/wiki/Magic_number_%28programming%29#Magic_numbers_in_files
            //http://www.mikekunz.com/image_file_header.html

            //JPEG:
            if (headerBytes[0] == 0xFF &&//FF D8
                headerBytes[1] == 0xD8 &&
                (
                 (headerBytes[6] == 0x4A &&//'JFIF'
                  headerBytes[7] == 0x46 &&
                  headerBytes[8] == 0x49 &&
                  headerBytes[9] == 0x46)
                  ||
                 (headerBytes[6] == 0x45 &&//'EXIF'
                  headerBytes[7] == 0x78 &&
                  headerBytes[8] == 0x69 &&
                  headerBytes[9] == 0x66)
                ) &&
                headerBytes[10] == 00)
            {
                return ImageType.JPEG;
            }
            //PNG 
            if (headerBytes[0] == 0x89 && //89 50 4E 47 0D 0A 1A 0A
                headerBytes[1] == 0x50 &&
                headerBytes[2] == 0x4E &&
                headerBytes[3] == 0x47 &&
                headerBytes[4] == 0x0D &&
                headerBytes[5] == 0x0A &&
                headerBytes[6] == 0x1A &&
                headerBytes[7] == 0x0A)
            {
                return ImageType.PNG;
            }
            //GIF
            if (headerBytes[0] == 0x47 &&//'GIF'
                headerBytes[1] == 0x49 &&
                headerBytes[2] == 0x46)
            {
                return ImageType.GIF;
            }
            //BMP
            if (headerBytes[0] == 0x42 &&//42 4D
                headerBytes[1] == 0x4D)
            {
                return ImageType.BMP;
            }
            //TIFF
            if ((headerBytes[0] == 0x49 &&//49 49 2A 00
                 headerBytes[1] == 0x49 &&
                 headerBytes[2] == 0x2A &&
                 headerBytes[3] == 0x00)
                 ||
                (headerBytes[0] == 0x4D &&//4D 4D 00 2A
                 headerBytes[1] == 0x4D &&
                 headerBytes[2] == 0x00 &&
                 headerBytes[3] == 0x2A))
            {
                return ImageType.TIFF;
            }

            return ImageType.Unknown;
        }
        public enum ImageType
        {
            Unknown,
            JPEG,
            PNG,
            GIF,
            BMP,
            TIFF,
        }

        private void UpdateText()
        {
            textBox1.Clear();
            textBox1.Text += "Filename: " + ImageName + "\r\n";
            textBox1.Text += "File type: " + ImageIdentification + "\r\n";
            textBox1.Text += "Pixel format: " + myBitmap.PixelFormat + "\r\n";
            textBox1.Text += "Size: " + ImageDimensions + "\r\n";
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            ImageName = "";
            CSfile.Clear();
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Clear();
                textBox2.Text = openFileDialog1.FileName;
                ImageIdentification = GetFileImageTypeFromHeader(openFileDialog1.FileName).ToString();
                textBox1.Text += "Image type: " + ImageIdentification + "\r\n";
                if (ImageIdentification != "Unknown")
                {
                    ImageName = Path.GetFileNameWithoutExtension(textBox2.Text);
                    myBitmap = new Bitmap(Image.FromFile(openFileDialog1.FileName));
                    pictureBox1.Image = myBitmap;
                    ImageColorDepth = Image.FromFile(openFileDialog1.FileName).PixelFormat + "\r\n";
                    ImageDimensions = myBitmap.Width.ToString() + "x" + myBitmap.Height.ToString() + "\r\n";
                    UpdateText();
                }
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            if (ImageName != "")
            {
                try
                {
                    Bitmap PreviewBMP = new Bitmap((int)Font_Width_Sel.Value * (int)Bmp_Col_Sel.Value, ((int)Font_Height_Sel.Value * ((int)Bmp_End_Sel.Value - (int)Bmp_Start_Sel.Value)));
                    int RowCount = -1;
                    for (int j = (int)Bmp_Start_Sel.Value; j < (int)Bmp_End_Sel.Value; j++) //bitmap Rows
                    {
                        RowCount++;
                        for (int i = 0; i < (int)Bmp_Col_Sel.Value; i++)  //Bitmap columns
                        {
                            Rectangle cloneRect = new Rectangle(i * (int)Font_Width_Sel.Value, j * (int)Font_Height_Sel.Value, (int)Font_Width_Sel.Value, (int)Font_Height_Sel.Value);
                            Bitmap cloneBitmap = myBitmap.Clone(cloneRect, myBitmap.PixelFormat);
                            Graphics g = Graphics.FromImage(cloneBitmap);
                            // Draw the cloned portion of the Bitmap object.
                            g.DrawImage(cloneBitmap, 0, 0);
                            Graphics gg = Graphics.FromImage(PreviewBMP);
                            gg.DrawImage(cloneBitmap, i * (int)Font_Width_Sel.Value, RowCount * (int)Font_Height_Sel.Value);
                        }
                    }
                    pictureBox2.Image = PreviewBMP;
                }
                catch (Exception ex)
                {
                    string message = "Numerical error: try other values";
                    string title = "Error";
                    MessageBox.Show(message, title);
                }
            }
            else
            {
                string message = "Load File";
                string title = "Error";
                MessageBox.Show(message, title);
            }
        }

        private void WriteHeader()
        {
            CSfile.Add("// Licensed to the .NET Foundation under one or more agreements.");
            CSfile.Add("// The .NET Foundation licenses this file to you under the MIT license.");
            CSfile.Add("// File Generated by Bitmap2Font");
            CSfile.Add("");
            CSfile.Add("using System;");
            CSfile.Add("");
            CSfile.Add("namespace Iot.Device.Ssd13xx.Samples");
            CSfile.Add("{");
            CSfile.Add("    public class " + ImageName + " : IFont");
            CSfile.Add("    {");
            CSfile.Add("        private static readonly byte[][] _fontTable =");
            CSfile.Add("        {");
        }

        private void WriteFooter()
        {
            CSfile.Add("        };");
            CSfile.Add("");
            CSfile.Add("        public override byte Width { get => "+ ((int)Font_Width_Sel.Value).ToString()+ "; }");
            CSfile.Add("        public override byte Height { get =>" + ((int)Font_Height_Sel.Value).ToString() + "; }");
            CSfile.Add("");
            CSfile.Add("        public override byte[] this[char character]");
            CSfile.Add("        {");
            CSfile.Add("            get");
            CSfile.Add("            {");
            CSfile.Add("                var index = (byte)character;");
            CSfile.Add("                if ((index < 32) || (index > 127))");
            CSfile.Add("                    return _fontTable[0x20];");
            CSfile.Add("                else");
            CSfile.Add("                    return _fontTable[index - 0x20];");
            CSfile.Add("            }");
            CSfile.Add("        }");
            CSfile.Add("    }");
            CSfile.Add("}");
        }

        private void GenButton_Click(object sender, EventArgs e)
        {
            if (ImageName != "")
            {
                try
                {
                    WriteHeader();
                    Bitmap PreviewBMP = new Bitmap((int)Font_Width_Sel.Value * (int)Bmp_Col_Sel.Value, ((int)Font_Height_Sel.Value * ((int)Bmp_End_Sel.Value - (int)Bmp_Start_Sel.Value)));
                    int RowCount = -1;
                    for (int j = (int)Bmp_Start_Sel.Value; j < (int)Bmp_End_Sel.Value; j++) //bitmap Rows
                    {
                        RowCount++;
                        for (int i = 0; i < (int)Bmp_Col_Sel.Value; i++)  //Bitmap columns
                        {
                            Rectangle cloneRect = new Rectangle(i * (int)Font_Width_Sel.Value, j * (int)Font_Height_Sel.Value, (int)Font_Width_Sel.Value, (int)Font_Height_Sel.Value);
                            Bitmap cloneBitmap = myBitmap.Clone(cloneRect, myBitmap.PixelFormat);
                            Graphics g = Graphics.FromImage(cloneBitmap);
                            // Draw the cloned portion of the Bitmap object.
                            g.DrawImage(cloneBitmap, 0, 0);
                            byte[] byteArray = new byte[(int)Font_Height_Sel.Value];

                            for (int y = 0; y < cloneBitmap.Height; y++)
                            {
                                byte rowByte = 0;
                                for (int x = 0; x < cloneBitmap.Width; x++)
                                {
                                    if (cloneBitmap.GetPixel(x, y).GetBrightness() < 0.02)
                                    {
                                        rowByte |= (byte)(1 << x);
                                    }
                                }
                                byteArray[y] = rowByte;
                            }
                            string str = string.Join(", ", byteArray.Select(item => "0x" + item.ToString("X2")));
                            CSfile.Add("        new byte[] { " + str + "},");
                            Graphics gg = Graphics.FromImage(PreviewBMP);
                            gg.DrawImage(cloneBitmap, i * (int)Font_Width_Sel.Value, RowCount * (int)Font_Height_Sel.Value);
                        }
                    }
                    WriteFooter();
                    pictureBox2.Image = PreviewBMP;

                    saveFileDialog1.Filter = "cs files (*.cs)|*.cs|All files (*.*)|*.*";
                    saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(textBox2.Text);
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllLines(saveFileDialog1.FileName, CSfile.ToArray());
                        PreviewBMP.Save(saveFileDialog1.FileName + ".bmp", ImageFormat.Bmp);
                    }

                }
                catch (Exception ex) 
                {
                    string message = "Numerical error: try other values";
                    string title = "Error";
                    MessageBox.Show(message, title);
                }


            }
            else
            {
                string message = "Load File and run Preview";
                string title = "Error";
                MessageBox.Show(message, title);
            }
        }
    }
}
