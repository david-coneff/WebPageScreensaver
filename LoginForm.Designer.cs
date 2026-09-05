namespace WebPageScreensaver
{
    partial class LoginForm
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
            if (disposing && (_components != null))
            {
                _components.Dispose();
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
            this._tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this._tableLayoutPanelAddress = new System.Windows.Forms.TableLayoutPanel();
            this._textBoxAddress = new System.Windows.Forms.TextBox();
            this._buttonGo = new System.Windows.Forms.Button();
            this._webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this._tableLayoutPanelMain.SuspendLayout();
            this._tableLayoutPanelAddress.SuspendLayout();
            this.SuspendLayout();
            //
            // _tableLayoutPanelMain
            //
            this._tableLayoutPanelMain.ColumnCount = 1;
            this._tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelMain.Controls.Add(this._tableLayoutPanelAddress, 0, 0);
            this._tableLayoutPanelMain.Controls.Add(this._webView, 0, 1);
            this._tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this._tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanelMain.Name = "_tableLayoutPanelMain";
            this._tableLayoutPanelMain.RowCount = 2;
            this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelMain.Size = new System.Drawing.Size(1000, 700);
            this._tableLayoutPanelMain.TabIndex = 0;
            //
            // _tableLayoutPanelAddress
            //
            this._tableLayoutPanelAddress.ColumnCount = 2;
            this._tableLayoutPanelAddress.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelAddress.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._tableLayoutPanelAddress.Controls.Add(this._textBoxAddress, 0, 0);
            this._tableLayoutPanelAddress.Controls.Add(this._buttonGo, 1, 0);
            this._tableLayoutPanelAddress.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanelAddress.Location = new System.Drawing.Point(0, 0);
            this._tableLayoutPanelAddress.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanelAddress.Name = "_tableLayoutPanelAddress";
            this._tableLayoutPanelAddress.RowCount = 1;
            this._tableLayoutPanelAddress.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelAddress.Size = new System.Drawing.Size(1000, 36);
            this._tableLayoutPanelAddress.TabIndex = 0;
            //
            // _textBoxAddress
            //
            this._textBoxAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._textBoxAddress.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this._textBoxAddress.Location = new System.Drawing.Point(6, 8);
            this._textBoxAddress.Margin = new System.Windows.Forms.Padding(6);
            this._textBoxAddress.Name = "_textBoxAddress";
            this._textBoxAddress.Size = new System.Drawing.Size(908, 23);
            this._textBoxAddress.TabIndex = 0;
            this._textBoxAddress.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxAddress_KeyDown);
            //
            // _buttonGo
            //
            this._buttonGo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._buttonGo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._buttonGo.Location = new System.Drawing.Point(926, 6);
            this._buttonGo.Margin = new System.Windows.Forms.Padding(6);
            this._buttonGo.Name = "_buttonGo";
            this._buttonGo.Size = new System.Drawing.Size(68, 24);
            this._buttonGo.TabIndex = 1;
            this._buttonGo.Text = "Go";
            this._buttonGo.UseVisualStyleBackColor = true;
            this._buttonGo.Click += new System.EventHandler(this.ButtonGo_Click);
            //
            // _webView
            //
            this._webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._webView.Location = new System.Drawing.Point(0, 36);
            this._webView.Margin = new System.Windows.Forms.Padding(0);
            this._webView.Name = "_webView";
            this._webView.Size = new System.Drawing.Size(1000, 664);
            this._webView.Source = new System.Uri("about:blank", System.UriKind.Absolute);
            this._webView.TabIndex = 1;
            this._webView.ZoomFactor = 1D;
            //
            // LoginForm
            //
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this._tableLayoutPanelMain);
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Web Page Screensaver — Log In";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this._tableLayoutPanelMain.ResumeLayout(false);
            this._tableLayoutPanelAddress.ResumeLayout(false);
            this._tableLayoutPanelAddress.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanelAddress;
        private System.Windows.Forms.TextBox _textBoxAddress;
        private System.Windows.Forms.Button _buttonGo;
        private Microsoft.Web.WebView2.WinForms.WebView2 _webView;
    }
}
