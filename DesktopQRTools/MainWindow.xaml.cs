﻿using System;
using System.Windows;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for the Create QR Code button click.
        /// Opens a new window for QR code creation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CreateQRCodeButton_Click(object sender, RoutedEventArgs e)
        {
            QRCodeGeneratorWindow qrCodeGeneratorWindow = new QRCodeGeneratorWindow(configPath: null);
            qrCodeGeneratorWindow.Show();
        }

        /// <summary>
        /// Event handler for the Scan QR Code button click.
        /// Initiates the process to scan a QR code on the screen.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ScanQRCodeButton_Click(object sender, RoutedEventArgs e)
        {
            QRCodeScanFunctionality.TriggerQRScan();
        }

        /// <summary>
        /// Event handler for the Options button click.
        /// Opens the Options window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new OptionsWindow();
            optionsWindow.ShowDialog();
        }

        /// <summary>
        /// Triggers the QR code scan functionality when the global hotkey is pressed.
        /// </summary>
        public void TriggerHotkeyFunctionality()
        {
            QRCodeScanFunctionality.TriggerQRScan();
        }
    }
}
