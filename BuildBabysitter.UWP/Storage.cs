using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using BuildBabysitter;
using Windows.Storage;
using System.IO;

[assembly: Dependency(typeof(BuildBabysitter.UWP.Storage))]
namespace BuildBabysitter.UWP
{
    public class Storage : IStorage
    {
        private readonly string _fileName = "saved-pull-request-status.json";
        public string LoadText
        {
            get
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                try
                {
                    StorageFile file =
                       storageFolder.GetFileAsync(_fileName).GetAwaiter().GetResult();
                    return FileIO.ReadTextAsync(file).GetAwaiter().GetResult();
                }
                catch (FileNotFoundException)
                {
                    return "";
                }
            }
        }

        public void SaveText(string value)
        {
            StorageFolder storageFolder =
                    ApplicationData.Current.LocalFolder;
            StorageFile file;
            try
            {
                file = storageFolder.GetFileAsync(_fileName).GetAwaiter().GetResult();
            }
            catch (FileNotFoundException)
            {
                storageFolder.CreateFileAsync(_fileName,
                    CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
                file = storageFolder.GetFileAsync(_fileName).GetAwaiter().GetResult();
            }
            FileIO.WriteTextAsync(file, value).GetAwaiter().GetResult();
        }
    }
}
