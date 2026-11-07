namespace WebPageScreensaver
{
    internal partial class PrefsByScreenUserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer _components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _components != null)
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._textBoxURL = new System.Windows.Forms.TextBox();
            this._numericUpDownZoomPercent = new System.Windows.Forms.NumericUpDown();
            this._labelZoomPercent = new System.Windows.Forms.Label();
            this._toolTipURLButtons = new System.Windows.Forms.ToolTip(this.components);
            this._tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this._tableLayoutPanelMainBottom = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this._numericUpDownZoomPercent)).BeginInit();
            this._tableLayoutPanelMain.SuspendLayout();
            this._tableLayoutPanelMainBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // _textBoxURL
            //
            this._textBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxURL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._textBoxURL.Location = new System.Drawing.Point(6, 6);
            this._textBoxURL.Margin = new System.Windows.Forms.Padding(6);
            this._textBoxURL.Name = "_textBoxURL";
            this._textBoxURL.Size = new System.Drawing.Size(308, 23);
            this._textBoxURL.TabIndex = 10;
            this._toolTipURLButtons.SetToolTip(this._textBoxURL, "The web page this display shows.");
            //
            // _numericUpDownZoomPercent
            //
            this._numericUpDownZoomPercent.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._numericUpDownZoomPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._numericUpDownZoomPercent.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            this._numericUpDownZoomPercent.Location = new System.Drawing.Point(186, 6);
            this._numericUpDownZoomPercent.Margin = new System.Windows.Forms.Padding(6);
            this._numericUpDownZoomPercent.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            this._numericUpDownZoomPercent.Minimum = new decimal(new int[] { 25, 0, 0, 0 });
            this._numericUpDownZoomPercent.Name = "_numericUpDownZoomPercent";
            this._numericUpDownZoomPercent.Size = new System.Drawing.Size(50, 23);
            this._numericUpDownZoomPercent.TabIndex = 13;
            this._numericUpDownZoomPercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this._numericUpDownZoomPercent.Value = new decimal(new int[] { 100, 0, 0, 0 });
            //
            // _labelZoomPercent
            //
            this._labelZoomPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._labelZoomPercent.AutoSize = true;
            this._labelZoomPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._labelZoomPercent.Location = new System.Drawing.Point(248, 10);
            this._labelZoomPercent.Margin = new System.Windows.Forms.Padding(6);
            this._labelZoomPercent.Name = "_labelZoomPercent";
            this._labelZoomPercent.Size = new System.Drawing.Size(66, 15);
            this._labelZoomPercent.Text = "% zoom";
            this._toolTipURLButtons.SetToolTip(this._labelZoomPercent, "The default zoom level to open this display's pages at (100 = no zoom).");
            //
            // _tableLayoutPanelMain
            //
            this._tableLayoutPanelMain.AutoSize = true;
            this._tableLayoutPanelMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._tableLayoutPanelMain.ColumnCount = 1;
            this._tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelMain.Controls.Add(this._textBoxURL, 0, 0);
            this._tableLayoutPanelMain.Controls.Add(this._tableLayoutPanelMainBottom, 0, 1);
            this._tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this._tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanelMain.Name = "_tableLayoutPanelMain";
            this._tableLayoutPanelMain.RowCount = 2;
            this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this._tableLayoutPanelMain.Size = new System.Drawing.Size(320, 70);
            //
            // _tableLayoutPanelMainBottom
            //
            this._tableLayoutPanelMainBottom.AutoSize = true;
            this._tableLayoutPanelMainBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._tableLayoutPanelMainBottom.ColumnCount = 3;
            this._tableLayoutPanelMainBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelMainBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanelMainBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanelMainBottom.Controls.Add(this._numericUpDownZoomPercent, 1, 0);
            this._tableLayoutPanelMainBottom.Controls.Add(this._labelZoomPercent, 2, 0);
            this._tableLayoutPanelMainBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanelMainBottom.Location = new System.Drawing.Point(0, 35);
            this._tableLayoutPanelMainBottom.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanelMainBottom.Name = "_tableLayoutPanelMainBottom";
            this._tableLayoutPanelMainBottom.RowCount = 1;
            this._tableLayoutPanelMainBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelMainBottom.Size = new System.Drawing.Size(320, 35);
            //
            // PrefsByScreenUserControl
            //
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this._tableLayoutPanelMain);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(320, 70);
            this.Name = "PrefsByScreenUserControl";
            this.Size = new System.Drawing.Size(320, 70);
            ((System.ComponentModel.ISupportInitialize)(this._numericUpDownZoomPercent)).EndInit();
            this._tableLayoutPanelMain.ResumeLayout(false);
            this._tableLayoutPanelMain.PerformLayout();
            this._tableLayoutPanelMainBottom.ResumeLayout(false);
            this._tableLayoutPanelMainBottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanelMain;

        public System.Windows.Forms.TextBox _textBoxURL;

        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanelMainBottom;
        public System.Windows.Forms.NumericUpDown _numericUpDownZoomPercent;
        private System.Windows.Forms.Label _labelZoomPercent;

        private System.Windows.Forms.ToolTip _toolTipURLButtons;
        private System.ComponentModel.IContainer components;
    }
}
