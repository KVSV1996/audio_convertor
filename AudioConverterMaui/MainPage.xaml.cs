

namespace AudioConverterMaui
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            //DependencyService.Get<IMainPageDragAndDropService>();
        }

        private async void SelectFileButton_Click(object sender, EventArgs e)
        {
            // Логика выбора файла
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Please select a file"
            });

            if (result != null)
            {
                StatusLabel.Text = $"Selected file: {result.FileName}";
            }
        }

        private async void SelectFolderButton_Click(object sender, EventArgs e)
        {
            // Логика выбора папки
            var result = await FolderPicker.Default.PickFolderAsync();

            if (result != null)
            {
                StatusLabel.Text = $"Selected folder: {result.FullPath}";
            }
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            // Логика конвертации аудио
            StatusLabel.Text = "Conversion started...";
            // Ваша логика конвертации
            StatusLabel.Text = "Conversion completed.";
        }
    }

}
