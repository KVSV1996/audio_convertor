using Microsoft.Win32;
using NAudio.Wave;
using System.Windows;
using System.IO;

namespace AudioConverter
{
    public partial class MainWindow : Window
    {
        private string inputFilePath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.wma;*.aac;*.flac";
            if (openFileDialog.ShowDialog() == true)
            {
                inputFilePath = openFileDialog.FileName;
                StatusLabel.Content = "Selected: " + Path.GetFileName(inputFilePath);
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inputFilePath))
            {
                MessageBox.Show("Please select a file first.");
                return;
            }

            string newFileName = NewFileNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(newFileName) || newFileName == "Enter new file name")
            {
                MessageBox.Show("Please enter a new file name.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "WAV files (*.wav)|*.wav";
            saveFileDialog.FileName = newFileName + ".wav";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string outputFilePath = saveFileDialog.FileName;

                    using (var reader = new MediaFoundationReader(inputFilePath))
                    {
                        var newFormat = new WaveFormat(8000, 16, 1); // 8000 Hz, 16 bit, mono
                        using (var resampler = new MediaFoundationResampler(reader, newFormat))
                        {
                            resampler.ResamplerQuality = 60; // 60 is the highest quality
                            WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                        }
                    }

                    StatusLabel.Content = "Conversion Completed: " + Path.GetFileName(outputFilePath);
                    MessageBox.Show("Conversion Completed!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    inputFilePath = files[0];
                    StatusLabel.Content = "Selected: " + Path.GetFileName(inputFilePath);
                }
            }
            e.Handled = true;
        }

        private void RemoveText(object sender, EventArgs e)
        {
            if (NewFileNameTextBox.Text == "Enter new file name")
            {
                NewFileNameTextBox.Text = "";
            }
        }

        private void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewFileNameTextBox.Text))
            {
                NewFileNameTextBox.Text = "Enter new file name";
            }
        }
    }
}