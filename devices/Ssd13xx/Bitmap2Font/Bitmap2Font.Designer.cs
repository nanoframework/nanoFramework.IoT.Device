namespace Bitmap2Font
{
    partial class Bitmap2Font
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PictureBoxOriginal = new System.Windows.Forms.PictureBox();
            this.PictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.OpenButton = new System.Windows.Forms.Button();
            this.TextBoxOriginalInfo = new System.Windows.Forms.TextBox();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.FontWidthSel = new System.Windows.Forms.NumericUpDown();
            this.FontHeightSel = new System.Windows.Forms.NumericUpDown();
            this.BmpEndSel = new System.Windows.Forms.NumericUpDown();
            this.BmpColSel = new System.Windows.Forms.NumericUpDown();
            this.BmpStartSel = new System.Windows.Forms.NumericUpDown();
            this.LabelFontWidth = new System.Windows.Forms.Label();
            this.LabelFontHeight = new System.Windows.Forms.Label();
            this.LabelBitmapColuimns = new System.Windows.Forms.Label();
            this.LabelStartingRow = new System.Windows.Forms.Label();
            this.LabelEndRow = new System.Windows.Forms.Label();
            this.TextBoxFilePath = new System.Windows.Forms.TextBox();
            this.PrevButton = new System.Windows.Forms.Button();
            this.GenButton = new System.Windows.Forms.Button();
            this.LabelOriginalBitmap = new System.Windows.Forms.Label();
            this.LabelPreviewBitmap = new System.Windows.Forms.Label();
            this.LabelOriginalBitmapInfo = new System.Windows.Forms.Label();
            this.LabalFirstAsciiCar = new System.Windows.Forms.Label();
            this.FirstAsciiCharacter = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontWidthSel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontHeightSel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BmpEndSel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BmpColSel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BmpStartSel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FirstAsciiCharacter)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBoxOriginal
            // 
            this.PictureBoxOriginal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBoxOriginal.Location = new System.Drawing.Point(28, 69);
            this.PictureBoxOriginal.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBoxOriginal.Name = "PictureBoxOriginal";
            this.PictureBoxOriginal.Size = new System.Drawing.Size(399, 368);
            this.PictureBoxOriginal.TabIndex = 0;
            this.PictureBoxOriginal.TabStop = false;
            // 
            // PictureBoxPreview
            // 
            this.PictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBoxPreview.Location = new System.Drawing.Point(576, 69);
            this.PictureBoxPreview.Margin = new System.Windows.Forms.Padding(4);
            this.PictureBoxPreview.Name = "PictureBoxPreview";
            this.PictureBoxPreview.Size = new System.Drawing.Size(399, 368);
            this.PictureBoxPreview.TabIndex = 1;
            this.PictureBoxPreview.TabStop = false;
            // 
            // OpenButton
            // 
            this.OpenButton.Location = new System.Drawing.Point(28, 16);
            this.OpenButton.Margin = new System.Windows.Forms.Padding(4);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(178, 28);
            this.OpenButton.TabIndex = 2;
            this.OpenButton.Text = "Open Bitmap";
            this.OpenButton.UseVisualStyleBackColor = true;
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // TextBoxOriginalInfo
            // 
            this.TextBoxOriginalInfo.Location = new System.Drawing.Point(28, 462);
            this.TextBoxOriginalInfo.Margin = new System.Windows.Forms.Padding(4);
            this.TextBoxOriginalInfo.Multiline = true;
            this.TextBoxOriginalInfo.Name = "TextBoxOriginalInfo";
            this.TextBoxOriginalInfo.Size = new System.Drawing.Size(399, 86);
            this.TextBoxOriginalInfo.TabIndex = 3;
            // 
            // FontWidthSel
            // 
            this.FontWidthSel.Location = new System.Drawing.Point(578, 462);
            this.FontWidthSel.Margin = new System.Windows.Forms.Padding(4);
            this.FontWidthSel.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.FontWidthSel.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.FontWidthSel.Name = "FontWidthSel";
            this.FontWidthSel.Size = new System.Drawing.Size(60, 22);
            this.FontWidthSel.TabIndex = 6;
            this.FontWidthSel.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // FontHeightSel
            // 
            this.FontHeightSel.Location = new System.Drawing.Point(578, 494);
            this.FontHeightSel.Margin = new System.Windows.Forms.Padding(4);
            this.FontHeightSel.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.FontHeightSel.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.FontHeightSel.Name = "FontHeightSel";
            this.FontHeightSel.Size = new System.Drawing.Size(60, 22);
            this.FontHeightSel.TabIndex = 7;
            this.FontHeightSel.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // BmpEndSel
            // 
            this.BmpEndSel.Location = new System.Drawing.Point(776, 526);
            this.BmpEndSel.Margin = new System.Windows.Forms.Padding(4);
            this.BmpEndSel.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.BmpEndSel.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.BmpEndSel.Name = "BmpEndSel";
            this.BmpEndSel.Size = new System.Drawing.Size(60, 22);
            this.BmpEndSel.TabIndex = 8;
            this.BmpEndSel.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // BmpColSel
            // 
            this.BmpColSel.Location = new System.Drawing.Point(777, 462);
            this.BmpColSel.Margin = new System.Windows.Forms.Padding(4);
            this.BmpColSel.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.BmpColSel.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.BmpColSel.Name = "BmpColSel";
            this.BmpColSel.Size = new System.Drawing.Size(60, 22);
            this.BmpColSel.TabIndex = 9;
            this.BmpColSel.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // BmpStartSel
            // 
            this.BmpStartSel.Location = new System.Drawing.Point(777, 494);
            this.BmpStartSel.Margin = new System.Windows.Forms.Padding(4);
            this.BmpStartSel.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.BmpStartSel.Name = "BmpStartSel";
            this.BmpStartSel.Size = new System.Drawing.Size(60, 22);
            this.BmpStartSel.TabIndex = 10;
            this.BmpStartSel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // LabelFontWidth
            // 
            this.LabelFontWidth.AutoSize = true;
            this.LabelFontWidth.Location = new System.Drawing.Point(642, 464);
            this.LabelFontWidth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelFontWidth.Name = "LabelFontWidth";
            this.LabelFontWidth.Size = new System.Drawing.Size(95, 16);
            this.LabelFontWidth.TabIndex = 11;
            this.LabelFontWidth.Text = "Font Width (px)";
            // 
            // LabelFontHeight
            // 
            this.LabelFontHeight.AutoSize = true;
            this.LabelFontHeight.Location = new System.Drawing.Point(642, 496);
            this.LabelFontHeight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelFontHeight.Name = "LabelFontHeight";
            this.LabelFontHeight.Size = new System.Drawing.Size(100, 16);
            this.LabelFontHeight.TabIndex = 12;
            this.LabelFontHeight.Text = "Font Height (px)";
            // 
            // LabelBitmapColuimns
            // 
            this.LabelBitmapColuimns.AutoSize = true;
            this.LabelBitmapColuimns.Location = new System.Drawing.Point(841, 464);
            this.LabelBitmapColuimns.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelBitmapColuimns.Name = "LabelBitmapColuimns";
            this.LabelBitmapColuimns.Size = new System.Drawing.Size(104, 16);
            this.LabelBitmapColuimns.TabIndex = 13;
            this.LabelBitmapColuimns.Text = "Bitmap Columns";
            // 
            // LabelStartingRow
            // 
            this.LabelStartingRow.AutoSize = true;
            this.LabelStartingRow.Location = new System.Drawing.Point(841, 496);
            this.LabelStartingRow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelStartingRow.Name = "LabelStartingRow";
            this.LabelStartingRow.Size = new System.Drawing.Size(171, 16);
            this.LabelStartingRow.TabIndex = 14;
            this.LabelStartingRow.Text = "Bitmap Starting Row (top=0)";
            // 
            // LabelEndRow
            // 
            this.LabelEndRow.AutoSize = true;
            this.LabelEndRow.Location = new System.Drawing.Point(841, 528);
            this.LabelEndRow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelEndRow.Name = "LabelEndRow";
            this.LabelEndRow.Size = new System.Drawing.Size(106, 16);
            this.LabelEndRow.TabIndex = 15;
            this.LabelEndRow.Text = "Bitmap End Row";
            // 
            // TextBoxFilePath
            // 
            this.TextBoxFilePath.Location = new System.Drawing.Point(213, 18);
            this.TextBoxFilePath.Margin = new System.Windows.Forms.Padding(4);
            this.TextBoxFilePath.Name = "TextBoxFilePath";
            this.TextBoxFilePath.Size = new System.Drawing.Size(761, 22);
            this.TextBoxFilePath.TabIndex = 16;
            // 
            // PrevButton
            // 
            this.PrevButton.Location = new System.Drawing.Point(455, 86);
            this.PrevButton.Margin = new System.Windows.Forms.Padding(4);
            this.PrevButton.Name = "PrevButton";
            this.PrevButton.Size = new System.Drawing.Size(100, 28);
            this.PrevButton.TabIndex = 18;
            this.PrevButton.Text = "Preview";
            this.PrevButton.UseVisualStyleBackColor = true;
            this.PrevButton.Click += new System.EventHandler(this.PrevButton_Click);
            // 
            // GenButton
            // 
            this.GenButton.Location = new System.Drawing.Point(455, 144);
            this.GenButton.Margin = new System.Windows.Forms.Padding(4);
            this.GenButton.Name = "GenButton";
            this.GenButton.Size = new System.Drawing.Size(100, 28);
            this.GenButton.TabIndex = 19;
            this.GenButton.Text = "Generate";
            this.GenButton.UseVisualStyleBackColor = true;
            this.GenButton.Click += new System.EventHandler(this.GenButton_Click);
            // 
            // LabelOriginalBitmap
            // 
            this.LabelOriginalBitmap.AutoSize = true;
            this.LabelOriginalBitmap.Location = new System.Drawing.Point(32, 50);
            this.LabelOriginalBitmap.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelOriginalBitmap.Name = "LabelOriginalBitmap";
            this.LabelOriginalBitmap.Size = new System.Drawing.Size(98, 16);
            this.LabelOriginalBitmap.TabIndex = 20;
            this.LabelOriginalBitmap.Text = "Original Bitmap";
            // 
            // LabelPreviewBitmap
            // 
            this.LabelPreviewBitmap.AutoSize = true;
            this.LabelPreviewBitmap.Location = new System.Drawing.Point(583, 50);
            this.LabelPreviewBitmap.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelPreviewBitmap.Name = "LabelPreviewBitmap";
            this.LabelPreviewBitmap.Size = new System.Drawing.Size(100, 16);
            this.LabelPreviewBitmap.TabIndex = 21;
            this.LabelPreviewBitmap.Text = "Preview Bitmap";
            // 
            // LabelOriginalBitmapInfo
            // 
            this.LabelOriginalBitmapInfo.AutoSize = true;
            this.LabelOriginalBitmapInfo.Location = new System.Drawing.Point(32, 443);
            this.LabelOriginalBitmapInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelOriginalBitmapInfo.Name = "LabelOriginalBitmapInfo";
            this.LabelOriginalBitmapInfo.Size = new System.Drawing.Size(122, 16);
            this.LabelOriginalBitmapInfo.TabIndex = 22;
            this.LabelOriginalBitmapInfo.Text = "Original Bitmap Info";
            // 
            // LabalFirstAsciiCar
            // 
            this.LabalFirstAsciiCar.AutoSize = true;
            this.LabalFirstAsciiCar.Location = new System.Drawing.Point(642, 528);
            this.LabalFirstAsciiCar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabalFirstAsciiCar.Name = "LabalFirstAsciiCar";
            this.LabalFirstAsciiCar.Size = new System.Drawing.Size(90, 16);
            this.LabalFirstAsciiCar.TabIndex = 24;
            this.LabalFirstAsciiCar.Text = "First ASCII car";
            // 
            // FirstAsciiCharacter
            // 
            this.FirstAsciiCharacter.Location = new System.Drawing.Point(578, 526);
            this.FirstAsciiCharacter.Margin = new System.Windows.Forms.Padding(4);
            this.FirstAsciiCharacter.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.FirstAsciiCharacter.Name = "FirstAsciiCharacter";
            this.FirstAsciiCharacter.Size = new System.Drawing.Size(60, 22);
            this.FirstAsciiCharacter.TabIndex = 23;
            this.FirstAsciiCharacter.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // Bitmap2Font
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 571);
            this.Controls.Add(this.LabalFirstAsciiCar);
            this.Controls.Add(this.FirstAsciiCharacter);
            this.Controls.Add(this.LabelOriginalBitmapInfo);
            this.Controls.Add(this.LabelPreviewBitmap);
            this.Controls.Add(this.LabelOriginalBitmap);
            this.Controls.Add(this.GenButton);
            this.Controls.Add(this.PrevButton);
            this.Controls.Add(this.TextBoxFilePath);
            this.Controls.Add(this.LabelEndRow);
            this.Controls.Add(this.LabelStartingRow);
            this.Controls.Add(this.LabelBitmapColuimns);
            this.Controls.Add(this.LabelFontHeight);
            this.Controls.Add(this.LabelFontWidth);
            this.Controls.Add(this.BmpStartSel);
            this.Controls.Add(this.BmpColSel);
            this.Controls.Add(this.BmpEndSel);
            this.Controls.Add(this.FontHeightSel);
            this.Controls.Add(this.FontWidthSel);
            this.Controls.Add(this.TextBoxOriginalInfo);
            this.Controls.Add(this.OpenButton);
            this.Controls.Add(this.PictureBoxPreview);
            this.Controls.Add(this.PictureBoxOriginal);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Bitmap2Font";
            this.Text = "Bitmap2Font";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontWidthSel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontHeightSel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BmpEndSel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BmpColSel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BmpStartSel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FirstAsciiCharacter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBoxOriginal;
        private System.Windows.Forms.PictureBox PictureBoxPreview;
        private System.Windows.Forms.Button OpenButton;
        private System.Windows.Forms.TextBox TextBoxOriginalInfo;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.NumericUpDown FontWidthSel;
        private System.Windows.Forms.NumericUpDown FontHeightSel;
        private System.Windows.Forms.NumericUpDown BmpEndSel;
        private System.Windows.Forms.NumericUpDown BmpColSel;
        private System.Windows.Forms.NumericUpDown BmpStartSel;
        private System.Windows.Forms.Label LabelFontWidth;
        private System.Windows.Forms.Label LabelFontHeight;
        private System.Windows.Forms.Label LabelBitmapColuimns;
        private System.Windows.Forms.Label LabelStartingRow;
        private System.Windows.Forms.Label LabelEndRow;
        private System.Windows.Forms.TextBox TextBoxFilePath;
        private System.Windows.Forms.Button PrevButton;
        private System.Windows.Forms.Button GenButton;
        private System.Windows.Forms.Label LabelOriginalBitmap;
        private System.Windows.Forms.Label LabelPreviewBitmap;
        private System.Windows.Forms.Label LabelOriginalBitmapInfo;
        private System.Windows.Forms.Label LabalFirstAsciiCar;
        private System.Windows.Forms.NumericUpDown FirstAsciiCharacter;
    }
}

