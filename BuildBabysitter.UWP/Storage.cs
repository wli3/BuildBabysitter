using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using BuildBabysitter;
using Windows.Storage;
using System.IO;
using Microsoft.FSharp.Core;

[assembly: Dependency(typeof(BuildBabysitter.UWP.Storage))]
namespace BuildBabysitter.UWP
{
    public class Storage : IStorage
    {
        private readonly string _fileName = "saved-pull-request-status.json";
        public FSharpOption<string> LoadText
        {
            get
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                try
                {
                    StorageFile file =
                       storageFolder.GetFileAsync(_fileName).GetAwaiter().GetResult();
                    return new FSharpOption<string>(FileIO.ReadTextAsync(file).GetAwaiter().GetResult());
                }
                catch (FileNotFoundException)
                {
                    return FSharpOption<string>.None;
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
