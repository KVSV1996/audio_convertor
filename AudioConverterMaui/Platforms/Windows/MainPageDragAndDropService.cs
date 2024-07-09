using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;

namespace AudioConverterMaui.Platforms.Windows
{
    public class MainPageDragAndDropService : IMainPageDragAndDropService
    {
        public MainPageDragAndDropService()
        {
            var window = (App.Current as App).Window as Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsBasePage;
            window.DragOver += OnDragOver;
            window.Drop += OnDrop;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var file = items.FirstOrDefault() as Windows.Storage.StorageFile;

                if (file != null)
                {
                    // Ваша логика обработки файла
                }
            }
        }
    }
}
