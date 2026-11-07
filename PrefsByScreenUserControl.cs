using System.Windows.Forms;

namespace WebPageScreensaver
{
    internal partial class PrefsByScreenUserControl : UserControl
    {
        public PrefsByScreenUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves the selected preferences for a specific screen.
        /// This method must be called by the general Save method of the Preferences window, and must pass each available screen number.
        /// </summary>
        /// <param name="screenNumber">The specific screen number for which these preferences must be saved.</param>
        public void Save(int screenNumber)
        {
            ScreenInformation currentScreen = Preferences.Screens[screenNumber];
            currentScreen.URL = _textBoxURL.Text;
            currentScreen.ZoomPercent = (int)_numericUpDownZoomPercent.Value;
        }
    }
}
