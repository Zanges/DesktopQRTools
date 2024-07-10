using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using System.IO;
using System.IO.Abstractions;
using Microsoft.Win32;
using System.Windows.Media;
using System.Configuration;
using System.IO.Abstractions.TestingHelpers;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for QRCodeGeneratorWindow.xaml
    /// </summary>
    public partial class QRCodeGeneratorWindow : Window
    {
        private WriteableBitmap? _generatedQRCode;
        private string? _qrCodeContent;
        private string _autoSaveQRCodeName = "QRCode";
        private string _autoSaveDirectory = "";
        private bool _skipSaveDialog = false;
        private bool _appendDate = false;
        private bool _appendTime = false;

        public virtual IFileSystem FileSystem { get; protected set; } = new FileSystem();

        public QRCodeGeneratorWindow(string? configPath = null)
        {
            InitializeComponent();
            LoadConfiguration(configPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
        }

        // Constructor for testing with mock file system
        public QRCodeGeneratorWindow(string configPath, IFileSystem fileSystem) : this(configPath)
        {
            FileSystem = fileSystem;
        }

        private void LoadConfiguration(string configFilePath = null)
        {
            if (configFilePath == null)
            {
                configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            }

            if (File.Exists(configFilePath))
            {
                string[] lines = File.ReadAllLines(configFilePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        switch (parts[0])
                        {
                            case "AutoSaveQRCodeName":
                                _autoSaveQRCodeName = parts[1];
                                break;
                            case "SkipSaveDialog":
                                bool.TryParse(parts[1], out _skipSaveDialog);
                                break;
                            case "AutoSaveDirectory":
                                _autoSaveDirectory = parts[1];
                                break;
                            case "AppendDate":
                                bool.TryParse(parts[1], out _appendDate);
                                break;
                            case "AppendTime":
                                bool.TryParse(parts[1], out _appendTime);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for the Generate QR Code button click.
        /// Generates a QR code based on the content in the text box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string content = ContentTextBox.Text;
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Please enter some content for the QR code.", "Empty Content", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _generatedQRCode = GenerateQRCode(content);
                QRCodeImage.Source = _generatedQRCode;
                SaveButton.IsEnabled = true;
                _qrCodeContent = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the QR code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event handler for the Save QR Code button click.
        /// Saves the generated QR code as an image file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_generatedQRCode == null)
            {
                MessageBox.Show("Please generate a QR code first.", "No QR Code", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fileName;
            string filePath;

            if (_skipSaveDialog && !string.IsNullOrEmpty(_autoSaveDirectory))
            {
                fileName = GetAutoSaveFileName();
                filePath = Path.Combine(_autoSaveDirectory, fileName);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = _autoSaveQRCodeName,
                    Filter = (ImageFormatComboBox.SelectedIndex == 0) ? "PNG Image|*.png" : "SVG Image|*.svg",
                    Title = "Save QR Code Image"
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                filePath = saveFileDialog.FileName;
            }

            try
            {
                if (ImageFormatComboBox.SelectedIndex == 0)
                {
                    SaveQRCodeImage(_generatedQRCode, filePath);
                }
                else
                {
                    if (_qrCodeContent != null)
                    {
                        SaveQRCodeAsSvg(_qrCodeContent, filePath);
                    }
                    else
                    {
                        MessageBox.Show("No QR code content available to save.", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                MessageBox.Show("QR code image saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the QR code image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Generates a QR code from the given text.
        /// </summary>
        /// <param name="text">The text to encode in the QR code.</param>
        /// <returns>A bitmap image of the generated QR code.</returns>
        public WriteableBitmap GenerateQRCode(string text)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 1,
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H
                }
            };

            var pixelData = writer.Write(text);
            var bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height, 96, 96, PixelFormats.Bgr32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, pixelData.Width, pixelData.Height), pixelData.Pixels, pixelData.Width * 4, 0);

            return bitmap;
        }

        /// <summary>
        /// Saves the generated QR code as a PNG image.
        /// </summary>
        /// <param name="bitmap">The WriteableBitmap containing the QR code.</param>
        /// <param name="filePath">The file path to save the image.</param>
        private void SaveQRCodeImage(WriteableBitmap bitmap, string filePath)
        {
            using (var stream = FileSystem.File.Create(filePath))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }
        }

        // Public method for testing
        public void SaveQRCode()
        {
            if (_generatedQRCode != null)
            {
                string fileName = GetAutoSaveFileName();
                string filePath = FileSystem.Path.Combine(_autoSaveDirectory, fileName);
                SaveQRCodeImage(_generatedQRCode, filePath);
            }
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a BitmapSource for WPF display.
        /// </summary>
        /// <param name="bitmap">The System.Drawing.Bitmap to convert.</param>
        /// <returns>A BitmapSource for WPF display.</returns>
        private BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        /// <summary>
        /// Saves the QR code as an SVG file.
        /// </summary>
        /// <param name="content">The content of the QR code.</param>
        /// <param name="filePath">The file path to save the SVG.</param>
        private void SaveQRCodeAsSvg(string content, string filePath)
        {
            var qrCodeWriter = new ZXing.BarcodeWriterSvg
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 1
                }
            };

            var svgImage = qrCodeWriter.Write(content);
            File.WriteAllText(filePath, svgImage.Content);
        }

        private string GetAutoSaveFileName()
        {
            string fileName = _autoSaveQRCodeName;
            if (_appendDate)
                fileName += $"-{DateTime.Now:yyyyMMdd}";
            if (_appendTime)
                fileName += $"-{DateTime.Now:HHmmss}";
            
            string extension = ImageFormatComboBox.SelectedIndex == 0 ? "png" : "svg";
            fileName += $".{extension}";

            // If file already exists, append a number
            int counter = 1;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string filePath = Path.Combine(_autoSaveDirectory, fileName);
            while (File.Exists(filePath))
            {
                fileName = $"{fileNameWithoutExtension}_{counter}.{extension}";
                filePath = Path.Combine(_autoSaveDirectory, fileName);
                counter++;
            }

            return fileName;
        }

        // Methods to support testing
        public string GetAutoSaveQRCodeName() => _autoSaveQRCodeName;
        public bool GetSkipSaveDialog() => _skipSaveDialog;
        public string GetAutoSaveDirectory() => _autoSaveDirectory;
        public bool GetAppendDate() => _appendDate;
        public bool GetAppendTime() => _appendTime;
    }
}
