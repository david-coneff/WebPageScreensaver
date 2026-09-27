using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace WebPageScreensaver
{
    internal partial class PreferencesForm : Form
    {
        private const string Webpage = "http://github.com/carlossanlop/web-page-screensaver/";

        public PreferencesForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method called when the form is loaded, so the UI gets updated with the registry data.
        /// </summary>
        private void PreferencesForm_Load(object sender, EventArgs e)
        {
            _checkBoxCloseOnMouseMovement.Checked = Preferences.CloseOnMouseMovement;

            MultiScreenMode multiScreenMode = Preferences.MultiScreen;

            // The Checked event will determine what to show in the tabs
            _radioButtonMirrorScreens.Checked = multiScreenMode == MultiScreenMode.Mirror;
            _radioButtonSeparateScreens.Checked = multiScreenMode == MultiScreenMode.Separate;
            _radioButtonSpanScreens.Checked = multiScreenMode == MultiScreenMode.Span;

            //_flowLayoutPanelMultiScreenMode.Enabled = Screen.AllScreens.Length > 1;
        }


        private void RadioButtonMultiScreenMode_Checked(object sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            MultiScreenMode multiScreenMode = radioButton.Name switch
            {
                nameof(_radioButtonMirrorScreens) => MultiScreenMode.Mirror,
                nameof(_radioButtonSeparateScreens) => MultiScreenMode.Separate,
                nameof(_radioButtonSpanScreens) => MultiScreenMode.Span,
                _ => throw new IndexOutOfRangeException("Unexpected radio button."),
            };

            // Save it to the registry
            Preferences.MultiScreen = multiScreenMode;

            int totalTabs = multiScreenMode switch
            {
                MultiScreenMode.Mirror or MultiScreenMode.Span => 1,
                MultiScreenMode.Separate => Screen.AllScreens.Length,
                _ => throw new IndexOutOfRangeException("Unrecognized MultiScreenMode value.")
            };

            string tabTextSuffix = multiScreenMode switch
            {
                MultiScreenMode.Mirror => " (Mirror)",
                MultiScreenMode.Span => " (Composite)",
                MultiScreenMode.Separate => "",
                _ => throw new IndexOutOfRangeException("Unrecognized MultiScreenMode value.")
            };

            _tabControlScreens.TabPages.Clear();

            for (int tabNumber = 0; tabNumber < totalTabs; tabNumber++)
            {
                TabPage tab = new TabPage
                {
                    Text = $"Display {tabNumber + 1}{tabTextSuffix}" // Matches registry key name
                };

                var currentUserControl = new PrefsByScreenUserControl
                {
                    AutoSize = true,
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Name = $"_prefsByScreenUserControl{tabNumber}",
                    TabIndex = 5
                };

                ScreenInformation currentScreen = Preferences.Screens[tabNumber];

                foreach (string url in currentScreen.URLs)
                {
                    currentUserControl._listViewURLs.Items.Add(url);
                }

                currentUserControl._numericUpDownRotationInterval.Value = currentScreen.RotationInterval;
                currentUserControl._checkBoxShuffle.Checked = currentScreen.Shuffle;
                currentUserControl._numericUpDownZoomPercent.Value = currentScreen.ZoomPercent;

                tab.Controls.Add(currentUserControl);
                _tabControlScreens.TabPages.Add(tab);
            }
        }

        /// <summary>
        /// Opens the project website in a new default browser tab.
        /// </summary>
        private void LinkLabelProjectURL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start {Webpage}",
                CreateNoWindow = true
            };
            Process.Start(startInfo);
        }

        /// <summary>
        /// Saves the selected settings and closes the window.
        /// </summary>
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Save();
            Close();
        }

        /// <summary>
        /// Closes the window without saving the settings.
        /// </summary>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Opens a real, interactive browser window (LoginForm) sharing the SAME persistent
        /// WebView2 profile the screensaver itself uses. Log in there once; the screensaver
        /// picks up the same session automatically from then on — see WebView2Session.
        /// </summary>
        private void ButtonLogIn_Click(object sender, EventArgs e)
        {
            string initialUrl = string.Empty;
            if (_tabControlScreens.SelectedTab?.Controls[0] is PrefsByScreenUserControl userControl
                && userControl._listViewURLs.Items.Count > 0)
            {
                initialUrl = userControl._listViewURLs.Items[0].Text;
            }

            using LoginForm loginForm = new LoginForm(initialUrl);
            loginForm.ShowDialog(this);
        }

        /// <summary>
        /// Exports every setting under HKCU\Software\WebPageScreensaver to a .reg file the user
        /// picks. Does not include the WebView2 login/session state -- see ConfigurationBackup.
        /// </summary>
        private void ToolStripMenuItemExportSettings_Click(object sender, EventArgs e)
        {
            using SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Export Settings",
                Filter = "Registry files (*.reg)|*.reg|All files (*.*)|*.*",
                DefaultExt = "reg",
                AddExtension = true,
                FileName = "WebPageScreensaver.reg",
            };

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                ConfigurationBackup.Export(saveFileDialog.FileName);
                MessageBox.Show(
                    this,
                    "Settings exported successfully." + Environment.NewLine + Environment.NewLine +
                    "Note: this does not include your logged-in session (cookies, etc.), which is " +
                    "stored separately in the WebView2 browser profile and is not exported.",
                    "Export Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Could not export settings." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Export Settings Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Imports settings from a .reg file the user picks back into the registry. Does not
        /// restore the WebView2 login/session state -- see ConfigurationBackup.
        /// </summary>
        private void ToolStripMenuItemImportSettings_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Import Settings",
                Filter = "Registry files (*.reg)|*.reg|All files (*.*)|*.*",
                DefaultExt = "reg",
                CheckFileExists = true,
            };

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                ConfigurationBackup.Import(openFileDialog.FileName);
                MessageBox.Show(
                    this,
                    "Settings imported successfully." + Environment.NewLine + Environment.NewLine +
                    "Close and reopen this Settings window to see the imported values: the controls " +
                    "already on screen were only populated once, when this window opened, and will " +
                    "not refresh on their own." + Environment.NewLine + Environment.NewLine +
                    "Note: this does not restore your logged-in session (cookies, etc.), which is " +
                    "stored separately in the WebView2 browser profile and is not part of this import.",
                    "Import Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Could not import settings." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Import Settings Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Read the data from the form and save it in the registry.
        /// </summary>
        private void Save()
        {
            Preferences.CloseOnMouseMovement = _checkBoxCloseOnMouseMovement.Checked;

            if (_radioButtonSpanScreens.Checked)
            {
                Preferences.MultiScreen = MultiScreenMode.Span;
            }
            else if (_radioButtonMirrorScreens.Checked)
            {
                Preferences.MultiScreen = MultiScreenMode.Mirror;
            }
            else // default
            {
                Preferences.MultiScreen = MultiScreenMode.Separate;
            }

            int screenNumber = 0;
            foreach (TabPage tab in _tabControlScreens.TabPages)
            {
                if (tab.Controls[0] is PrefsByScreenUserControl userControl)
                {
                    userControl.Save(screenNumber);
                    screenNumber++;
                }
                else
                {
                    throw new KeyNotFoundException("PrefsByScreenUserControl instance not found in tab.");
                }
            }
        }
    }
}
