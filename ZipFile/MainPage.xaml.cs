using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ZipFile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string DestinationFolderPath = string.Empty;
        string SourceFolderPath = string.Empty;

        StorageFolder SourceFolder;
        StorageFolder DestinationFolder;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker FolderPickFol = new FolderPicker();
            FolderPickFol.SuggestedStartLocation = PickerLocationId.Desktop;
            FolderPickFol.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder SelectFolderToZipa = await FolderPickFol.PickSingleFolderAsync();
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolder", SelectFolderToZipa);
            SourceFolder = SelectFolderToZipa;
            SourceFolderPath = SelectFolderToZipa.Path;
            TxbFolderToZip.Text = SourceFolderPath;
        }

        private async void BtnChooseDestination_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker FolderPickFol = new FolderPicker();
            FolderPickFol.SuggestedStartLocation = PickerLocationId.Desktop;
            FolderPickFol.FileTypeFilter.Add("*");
            StorageFolder SelectFolderToZipa = await FolderPickFol.PickSingleFolderAsync();
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedDestination", SelectFolderToZipa);
            DestinationFolder = SelectFolderToZipa;
            DestinationFolderPath = SelectFolderToZipa.Path;
            TxbZipFolder.Text = DestinationFolderPath;
        }

        private async void BtnZip_Click(object sender, RoutedEventArgs e)
        {

            if (SourceFolder != null)
            {

                try
                {
                    string appFolderPath = ApplicationData.Current.LocalFolder.Path;
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", SourceFolder);
                    //StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", DestinationFolder);
                   //  StorageFolder destinationFolder = await StorageFolder.GetFolderFromPathAsync(appFolderPath);

                    //Gets the folder named TestFolder from Documents Library Folder  
                    StorageFolder sourceFolder = SourceFolder;

                    //Creates a zip file named TestFolder.zip in Local Folder  
                    StorageFile zipFile = await DestinationFolder.CreateFileAsync("TestFolder.zip", CreationCollisionOption.ReplaceExisting);
                    Stream zipToCreate = await zipFile.OpenStreamForWriteAsync();
                    ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create);

                    await ZipFolderContentsHelper(sourceFolder, archive, sourceFolder.Path);
                    archive.Dispose();
                    MessageDialog msg = new MessageDialog("Success");
                    await msg.ShowAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

            }
        }

       
        private async Task ZipFolderContentsHelper(StorageFolder sourceFolder, ZipArchive archive, string sourceFolderPath)
        {
            IReadOnlyList<StorageFile> files = await sourceFolder.GetFilesAsync();

            foreach (StorageFile file in files)
            {
                var path = file.Path.Remove(0, sourceFolderPath.Length);
                ZipArchiveEntry readmeEntry = archive.CreateEntry(file.Path.Remove(0, sourceFolderPath.Length));
                ulong fileSize = (await file.GetBasicPropertiesAsync()).Size;
                byte[] buffer = fileSize > 0 ? (await FileIO.ReadBufferAsync(file)).ToArray()
                : new byte[0];

              
                using (Stream entryStream = readmeEntry.Open())
                {
                    await entryStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }

            IReadOnlyList<StorageFolder> subFolders = await sourceFolder.GetFoldersAsync();

            if (subFolders.Count() == 0)
            {
                return;
            }

            foreach (StorageFolder subfolder in subFolders)
            {
                await ZipFolderContentsHelper(subfolder, archive, sourceFolderPath);
            }
        }
    }
}
