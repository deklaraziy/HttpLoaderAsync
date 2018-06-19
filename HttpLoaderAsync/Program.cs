using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.ComponentModel;

namespace HttpLoaderAsync
{
    class Loader : WebClient
    {
        public DownloadComponent component = new DownloadComponent();
        private string Currentfile;
        private void SetCurrentfile(string value)
        {
            Currentfile = value;
        }
        public string GetCurrentfile()
        {
            return Currentfile;
        }
        public void StartLoad(Uri adress, string filename)
        {
            SetCurrentfile(value: filename);
            this.DownloadFileAsync(adress, filename);
        }
    }
    class DownloadComponent
    {
        public DownloadComponent(string str)
        {
            var parsing = str.Split();
            AdressUrl = parsing[0];
            TypeHashSum = parsing[1].ToLower();
            HashSum = parsing[2].ToUpper();
            FileOfName = Path.GetFileName(AdressUrl);
            ChkedUrlAdress = new Uri(AdressUrl);
        }
        public DownloadComponent()
        {

        }
        public void SetAll(string str)
        {
            var parsing = str.Split();
            AdressUrl = parsing[0];
            TypeHashSum = parsing[1].ToLower();
            HashSum = parsing[2].ToUpper();
            FileOfName = Path.GetFileName(AdressUrl);
            ChkedUrlAdress = new Uri(AdressUrl);
        }
        public string AdressUrl { get; private set; }
        public string TypeHashSum { get; private set; }
        public string HashSum { get; private set; }
        public string FileOfName { get; private set; }
        public Uri ChkedUrlAdress { get; private set; }
    }
    class ChekHash
    {
        public void ProgressEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("{0}    downloaded {1} of {2} bytes. {3} % complete... \r",
        (string)e.UserState,
        e.BytesReceived,
        e.TotalBytesToReceive,
        e.ProgressPercentage);
        }
        public void LoadCompleteEvent(object sender, AsyncCompletedEventArgs e)
        {
            Loader loader = sender as Loader;
            if (loader.component.TypeHashSum == null)
            {
                return;
            }
            string ResultJfCheck = Check(loader.component.TypeHashSum, loader.component.FileOfName, loader.component.HashSum);
            Console.WriteLine("{0} complete hash {1}", loader.component.FileOfName, ResultJfCheck);
        }
        private string Check(string TypeHash, string FileName, string Hash)
        {
            switch (TypeHash)
            {
                case "sha1":
                    if (ChekHashSHA1(FileName, Hash))
                        return "OK";
                    if (!ChekHashSHA1(FileName, Hash))
                        return "faild!";
                    break;
                case "md5":
                    if (ChekHashMD5(FileName, Hash))
                        return "OK";
                    if (!ChekHashMD5(FileName, Hash))
                        return "faild!";
                    break;
                case "sha256":
                    if (ChekHashSHA256(FileName, Hash))
                        return "OK";
                    if (!ChekHashSHA256(FileName, Hash))
                        return "faild!";
                    break;
                default:
                    return "Error chek sum select";
            }
            return "Uniknow error";
        }
        private bool ChekHashMD5(string pathFile, string HashSum)
        {
            bool result;
            using (FileStream fs = File.OpenRead(pathFile))
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] chekSum = md5.ComputeHash(fs);
                string resultSum = BitConverter.ToString(chekSum).Replace("-", string.Empty);
                result = String.Equals(resultSum, HashSum);
            }
            return result;
        }
        private bool ChekHashSHA1(string pathFile, string HashSum)
        {
            bool result;
            using (FileStream fs = File.OpenRead(pathFile))
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] chekSum = sha1.ComputeHash(fs);
                string resultSum = BitConverter.ToString(chekSum).Replace("-", string.Empty);
                result = String.Equals(resultSum, HashSum);
                return result;
            }
        }
        private bool ChekHashSHA256(string pathFile, string HashSum)
        {
            bool result;
            using (FileStream fs = File.OpenRead(pathFile))
            using (SHA256 sha256 = new SHA256CryptoServiceProvider())
            {
                byte[] chekSum = sha256.ComputeHash(fs);
                string resultSum = BitConverter.ToString(chekSum).Replace("-", string.Empty);
                result = String.Equals(resultSum, HashSum);
                return result;
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<Loader> loaders = new List<Loader>();
            ChekHash chekHash = new ChekHash();


            string pathInputFile = "input.txt";
            try
            {
                using (StreamReader inread = new StreamReader(pathInputFile))
                {
                    int j = 0;
                    while (!inread.EndOfStream)
                    {
                        string line = inread.ReadLine();
                        if (line.Length != 0)
                        {
                            loaders.Add(new Loader());
                            loaders[j].component.SetAll(line);
                            loaders[j].StartLoad(loaders[j].component.ChkedUrlAdress, loaders[j].component.FileOfName);
                            loaders[j].DownloadFileCompleted += new AsyncCompletedEventHandler(chekHash.LoadCompleteEvent);
                            j++;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("File input.txt not found");
                Console.WriteLine(e.Message);
            }

            foreach (var l in loaders)
            {
                while (l.IsBusy) ;
            }
        }
    }
}
