using Microsoft.Win32;
using NAudio.Wave;
using System.Windows;
using System.IO;
using Ookii.Dialogs.Wpf;
using Concentus.Structs;
using System;
using NVorbis;
using Concentus.Oggfile;
using NAudio.Vorbis;

namespace AudioConverter
{
    public partial class MainWindow : Window
    {
        private string inputFilePath;
        private string inputFolderPath;
        private string folderName;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.wma;*.aac;*.flac;*.ogg;*.m4a";
            if (openFileDialog.ShowDialog() == true)
            {
                inputFilePath = openFileDialog.FileName;
                StatusLabel.Content = "Selected: " + Path.GetFileName(inputFilePath);
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();

            if (dialog.ShowDialog() == true)
            {
                inputFolderPath = dialog.SelectedPath;
                folderName = Path.GetFileName(inputFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                StatusLabel.Content = "Selected Folder: " + folderName;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(inputFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "WAV files (*.wav)|*.wav";
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(inputFilePath) + ".wav";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string outputFilePath = saveFileDialog.FileName;
                    ConvertFile(inputFilePath, outputFilePath);
                }
            }
            else if (!string.IsNullOrEmpty(inputFolderPath))
            {
                var dialog = new VistaFolderBrowserDialog();
                dialog.SelectedPath = inputFolderPath;

                if (dialog.ShowDialog() == true)
                {
                    string outputFolderPath = dialog.SelectedPath;
                    ConvertAllFilesInDirectory(inputFolderPath, outputFolderPath);
                    StatusLabel.Content = "Conversion Completed: " + folderName;
                    MessageBox.Show("Conversion Completed!");
                }
            }
            else
            {
                MessageBox.Show("Please select a file or a folder first.");
            }
        }

        private void ConvertAllFilesInDirectory(string directoryPath, string outputFolderPath)
        {
            var supportedExtensions = new[] { ".mp3", ".m4a", ".ogg", ".wav", ".wma", ".aac", ".flac" };
            var files = Directory.GetFiles(directoryPath)
                                 .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                                 .ToArray();

            foreach (var file in files)
            {
                string outputFilePath = Path.Combine(outputFolderPath, Path.GetFileNameWithoutExtension(file) + ".wav");
                ConvertFile(file, outputFilePath);
            }
        }

        private void ConvertFile(string inputFilePath, string outputFilePath)
        {
            try
            {
                using (var reader = CreateAudioReader(inputFilePath))
                {
                    if (reader == null)
                    {
                        throw new InvalidOperationException("Unsupported audio format.");
                    }

                    var newFormat = new WaveFormat(8000, 16, 1); // 8000 Hz, 16 bit, mono
                    using (var resampler = new MediaFoundationResampler(reader, newFormat))
                    {
                        resampler.ResamplerQuality = 60; // 60 is the highest quality
                        WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                    }
                }

                if (string.IsNullOrEmpty(inputFolderPath))
                {
                    StatusLabel.Content = "Conversion Completed: " + Path.GetFileName(outputFilePath);
                    MessageBox.Show("Conversion Completed!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error converting file: " + inputFilePath + "\n" + ex.Message);
            }
        }

        private WaveStream CreateAudioReader(string inputFilePath)
        {
            string extension = Path.GetExtension(inputFilePath).ToLower();

            try
            {
                switch (extension)
                {
                    case ".mp3":
                    case ".wma":
                    case ".aac":
                    case ".flac":
                    case ".m4a":
                        return new MediaFoundationReader(inputFilePath);
                    case ".wav":
                        return new WaveFileReader(inputFilePath);
                    case ".ogg":
                        return CreateOpusReader(inputFilePath) ?? (WaveStream)new VorbisWaveReader(inputFilePath);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing audio reader: " + ex.Message);
                return null;
            }
        }

        private WaveStream CreateOpusReader(string inputFilePath)
        {
            try
            {
                var inputStream = File.OpenRead(inputFilePath);
                var opusDecoder = new OpusDecoder(48000, 2); // Заменено
                int sampleRate = 48000; // Стандартная частота для Opus
                int channels = 2; // Предполагаем стерео

                var oggStream = new OpusOggReadStream(opusDecoder, inputStream);

                // Создаем временный файл WAV
                var tempWavFile = Path.GetTempFileName();

                using (var waveFileWriter = new WaveFileWriter(tempWavFile, new WaveFormat(sampleRate, 16, channels)))
                {
                    while (oggStream.HasNextPacket)
                    {
                        short[] packetSamples = oggStream.DecodeNextPacket();
                        if (packetSamples != null)
                        {
                            byte[] buffer = new byte[packetSamples.Length * sizeof(short)];
                            Buffer.BlockCopy(packetSamples, 0, buffer, 0, buffer.Length);
                            waveFileWriter.Write(buffer, 0, buffer.Length);
                        }
                    }
                }

                // Читаем временный файл WAV
                return new WaveFileReader(tempWavFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error decoding Opus file: " + ex.Message);
                return null;
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
                string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var item in droppedItems)
                {
                    if (Directory.Exists(item))
                    {
                        inputFolderPath = item;
                        folderName = Path.GetFileName(item.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                        StatusLabel.Content = "Selected Folder: " + folderName;
                        inputFilePath = null;
                    }
                    else if (File.Exists(item))
                    {
                        inputFilePath = item;
                        StatusLabel.Content = "Selected: " + Path.GetFileName(item);
                        inputFolderPath = null;
                    }
                }
            }
            e.Handled = true;
        }
    }
}
