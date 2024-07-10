using System.Windows;

namespace DesktopQRTools
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement saving options
            DesktopQRToolsTests.MessageBoxManager.Show("Options saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}
