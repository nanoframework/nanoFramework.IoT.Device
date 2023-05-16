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

        private Bitmap _myBitmap = new Bitmap(1, 1);
        private string _imageIdentification = string.Empty;
        private string _imageColorDepth = string.Empty;
        private string _imageDimensions = string.Empty;
        private string _imageName= string.Empty;
        private List<string> _cSfiles = new List<string>();

        /// <summary>
        /// Gets the file type from the header.
        /// </summary>
        /// <param name="file">The file header.</param>
        /// <returns>The image type.</returns>
        public static ImageType GetFileImageTypeFromHeader(string file) // from StackOverFlow      
        {
            byte[] headerBytes;
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                const int mostBytesNeeded = 11;//For JPEG
                if (fileStream.Length < mostBytesNeeded)
                {
                    return ImageType.Unknown;
                }

                headerBytes = new byte[mostBytesNeeded];
                fileStream.Read(headerBytes, 0, mostBytesNeeded);
            }

            // Sources:
            // http://stackoverflow.com/questions/9354747
            // http://en.wikipedia.org/wiki/Magic_number_%28programming%29#Magic_numbers_in_files
            // http://www.mikekunz.com/image_file_header.html

            // JPEG:
            if (headerBytes[0] == 0xFF && // FF D8
                headerBytes[1] == 0xD8 &&
                (
                 (headerBytes[6] == 0x4A && // 'JFIF'
                  headerBytes[7] == 0x46 &&
                  headerBytes[8] == 0x49 &&
                  headerBytes[9] == 0x46)
                  ||
                 (headerBytes[6] == 0x45 && // 'EXIF'
                  headerBytes[7] == 0x78 &&
                  headerBytes[8] == 0x69 &&
                  headerBytes[9] == 0x66)
                ) &&
                headerBytes[10] == 00)
            {
                return ImageType.JPEG;
            }

            // PNG 
            if (headerBytes[0] == 0x89 && // 89 50 4E 47 0D 0A 1A 0A
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

            // GIF
            if (headerBytes[0] == 0x47 && // 'GIF'
                headerBytes[1] == 0x49 &&
                headerBytes[2] == 0x46)
            {
                return ImageType.GIF;
            }

            // BMP
            if (headerBytes[0] == 0x42 && // 42 4D
                headerBytes[1] == 0x4D)
            {
                return ImageType.BMP;
            }

            // TIFF
            if ((headerBytes[0] == 0x49 && // 49 49 2A 00
                 headerBytes[1] == 0x49 &&
                 headerBytes[2] == 0x2A &&
                 headerBytes[3] == 0x00)
                 ||
                (headerBytes[0] == 0x4D && // 4D 4D 00 2A
                 headerBytes[1] == 0x4D &&
                 headerBytes[2] == 0x00 &&
                 headerBytes[3] == 0x2A))
            {
                return ImageType.TIFF;
            }

            return ImageType.Unknown;
        }

        private void UpdateText()
        {
            TextBoxOriginalInfo.Clear();
            TextBoxOriginalInfo.Text += "Filename: " + _imageName + "\r\n";
            TextBoxOriginalInfo.Text += "File type: " + _imageIdentification + "\r\n";
            TextBoxOriginalInfo.Text += "Pixel format: " + _myBitmap.PixelFormat + "\r\n";
            TextBoxOriginalInfo.Text += "Size: " + _imageDimensions + "\r\n";
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            PictureBoxOriginal.Image = null;
            PictureBoxPreview.Image = null;
            _imageName = string.Empty;
            _cSfiles.Clear();
            DialogResult result = OpenFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                TextBoxOriginalInfo.Clear();
                TextBoxFilePath.Text = OpenFileDialog.FileName;
                _imageIdentification = GetFileImageTypeFromHeader(OpenFileDialog.FileName).ToString();
                TextBoxOriginalInfo.Text += "Image type: " + _imageIdentification + "\r\n";

                if (_imageIdentification != "Unknown")
                {
                    _imageName = Path.GetFileNameWithoutExtension(TextBoxFilePath.Text).Replace(".", "").Replace("-", "");
                    _myBitmap = new Bitmap(Image.FromFile(OpenFileDialog.FileName));
                    PictureBoxOriginal.Image = _myBitmap;
                    _imageColorDepth = Image.FromFile(OpenFileDialog.FileName).PixelFormat + "\r\n";
                    _imageDimensions = _myBitmap.Width.ToString() + "x" + _myBitmap.Height.ToString() + "\r\n";
                    UpdateText();
                }
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_imageName))
            {
                try
                {
                    Bitmap previewBMP = new Bitmap((int)FontWidthSel.Value * (int)BmpColSel.Value, ((int)FontHeightSel.Value * ((int)BmpEndSel.Value - (int)BmpStartSel.Value)));
                    int rowCount = -1;

                    // bitmap Rows
                    for (int j = (int)BmpStartSel.Value; j < (int)BmpEndSel.Value; j++)
                    {
                        rowCount++;

                        // Bitmap columns
                        for (int i = 0; i < (int)BmpColSel.Value; i++)
                        {
                            Rectangle cloneRect = new Rectangle(i * (int)FontWidthSel.Value, j * (int)FontHeightSel.Value, (int)FontWidthSel.Value, (int)FontHeightSel.Value);
                            Bitmap cloneBitmap = _myBitmap.Clone(cloneRect, _myBitmap.PixelFormat);
                            Graphics g = Graphics.FromImage(cloneBitmap);

                            // Draw the cloned portion of the Bitmap object.
                            g.DrawImage(cloneBitmap, 0, 0);
                            Graphics gg = Graphics.FromImage(previewBMP);
                            gg.DrawImage(cloneBitmap, i * (int)FontWidthSel.Value, rowCount * (int)FontHeightSel.Value);
                        }
                    }

                    PictureBoxPreview.Image = previewBMP;
                }
                catch (Exception ex)
                {
                    string message = $"Numerical error: try other values. {ex}";
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
            _cSfiles.Add("// Licensed to the .NET Foundation under one or more agreements.");
            _cSfiles.Add("// The .NET Foundation licenses this file to you under the MIT license.");
            _cSfiles.Add("// File Automatically Generated by Bitmap2Font");
            _cSfiles.Add("");
            _cSfiles.Add("using System;");
            _cSfiles.Add("");
            _cSfiles.Add("namespace Iot.Device.Ssd13xx");
            _cSfiles.Add("{");
            _cSfiles.Add("    /// <summary>");
            _cSfiles.Add("    /// " + _imageName + " font.");
            _cSfiles.Add("    /// </summary>");
            _cSfiles.Add("    public class " + _imageName + " : IFont");
            _cSfiles.Add("    {");
            _cSfiles.Add("        private static readonly byte[][] _fontTable =");
            _cSfiles.Add("        {");
        }

        private void WriteFooter()
        {
            _cSfiles.Add("        };");
            _cSfiles.Add("");
            _cSfiles.Add("        /// <inheritdoc/>");
            _cSfiles.Add("        public override byte Width { get => " + ((int)FontWidthSel.Value).ToString() + "; }");
            _cSfiles.Add("");
            _cSfiles.Add("        /// <inheritdoc/>");
            _cSfiles.Add("        public override byte Height { get => " + ((int)FontHeightSel.Value).ToString() + "; }");
            _cSfiles.Add("");
            _cSfiles.Add("        /// <inheritdoc/>");
            _cSfiles.Add("        public override byte[] this[char character]");
            _cSfiles.Add("        {");
            _cSfiles.Add("            get");
            _cSfiles.Add("            {");
            _cSfiles.Add("                var index = (byte)character;");
            _cSfiles.Add("                if ((index < " + FirstAsciiCharacter.Value + ") || (index > " + (FirstAsciiCharacter.Value + BmpColSel.Value * (BmpEndSel.Value - BmpStartSel.Value) -1) + "))");
            _cSfiles.Add("                {");
            _cSfiles.Add("                    return _fontTable[" + FirstAsciiCharacter.Value + "];");
            _cSfiles.Add("                }");
            _cSfiles.Add("                else");
            _cSfiles.Add("                {");
            _cSfiles.Add("                    return _fontTable[index - " + FirstAsciiCharacter.Value + "];");
            _cSfiles.Add("                }");
            _cSfiles.Add("            }");
            _cSfiles.Add("        }");
            _cSfiles.Add("    }");
            _cSfiles.Add("}");
        }

        private void GenButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_imageName))
            {
                try
                {
                    WriteHeader();
                    Bitmap previewBMP = new Bitmap((int)FontWidthSel.Value * (int)BmpColSel.Value, ((int)FontHeightSel.Value * ((int)BmpEndSel.Value - (int)BmpStartSel.Value)));
                    int rowCount = -1;

                    //bitmap Rows
                    for (int j = (int)BmpStartSel.Value; j < (int)BmpEndSel.Value; j++)
                    {
                        rowCount++;

                        //Bitmap columns
                        for (int i = 0; i < (int)BmpColSel.Value; i++) 
                        {
                            Rectangle cloneRect = new Rectangle(i * (int)FontWidthSel.Value, j * (int)FontHeightSel.Value, (int)FontWidthSel.Value, (int)FontHeightSel.Value);
                            Bitmap cloneBitmap = _myBitmap.Clone(cloneRect, _myBitmap.PixelFormat);
                            Graphics g = Graphics.FromImage(cloneBitmap);

                            // Draw the cloned portion of the Bitmap object.
                            g.DrawImage(cloneBitmap, 0, 0);
                            byte[] byteArray = new byte[(int)FontHeightSel.Value];

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
                            _cSfiles.Add("            new byte[] { " + str + " },");
                            Graphics gg = Graphics.FromImage(previewBMP);
                            gg.DrawImage(cloneBitmap, i * (int)FontWidthSel.Value, rowCount * (int)FontHeightSel.Value);
                        }
                    }

                    WriteFooter();
                    PictureBoxPreview.Image = previewBMP;

                    SaveFileDialog.Filter = "cs files (*.cs)|*.cs|All files (*.*)|*.*";
                    SaveFileDialog.FileName = Path.GetFileNameWithoutExtension(TextBoxFilePath.Text);
                    if (SaveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllLines(SaveFileDialog.FileName, _cSfiles.ToArray());
                        previewBMP.Save(SaveFileDialog.FileName + ".bmp", ImageFormat.Bmp);
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Numerical error: try other values. {ex}";
                    string title = "Error";
                    MessageBox.Show(message, title);
                }
            }
            else
            {
                string message = "Load File and run Preview.";
                string title = "Error";
                MessageBox.Show(message, title);
            }
        }
    }
}
