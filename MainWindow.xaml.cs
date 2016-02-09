using ExifLib;
using MediaInfoDotNet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RPictureArrange
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string targetPath;
        public MainWindow()
        {
            InitializeComponent();
            RegistryKey pRegKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\LimInfi", true);
           
            if (pRegKey == null)
            {
                pRegKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\LimInfi");
            }

            targetPath = pRegKey.GetValue("TargetPath") as string;

            if (targetPath == null)
            {
                pRegKey.SetValue("TargetPath", @"C:\PHOTOS\");
                targetPath = @"C:\PHOTOS\";
            }


            targetPath = @"C:\PHOTOS\";
            this.AllowDrop = true;
            targetDirTextBox.Text = targetPath;
        }

        private void selectPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                targetDirTextBox.Text = openFileDialog.FileName;
            }


        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }





        MD5 md5 = new MD5CryptoServiceProvider();

        private void ProcessOnFile(string sf, string extName)
        {
            // #1 Get MD5 Filename
            byte[] contents = File.ReadAllBytes(sf);
            byte[] output = md5.ComputeHash(contents);
            string newFileName = BitConverter.ToString(output).Replace("-", "") + extName;


            // #2 Get Taken DateTime
            string outDirName = "UNKNOWN";

            try
            {
                using (ExifReader reader = new ExifReader(sf))
                {
                    DateTime datePictureTaken;
                    if (reader.GetTagValue(ExifTags.DateTime, out datePictureTaken))
                    {
                        outDirName = string.Format("{0}-{1}", datePictureTaken.Year.ToString("D4"), datePictureTaken.Month.ToString("D2"));
                    }
                }

            }
            catch(Exception)
            {
            }


            // #3 Create directory if not exists
            Directory.CreateDirectory(this.targetPath + outDirName);
            File.WriteAllBytes(this.targetPath + outDirName + @"\" + newFileName + extName, contents);
            File.Delete(sf);
        }



        private void ProcessMovieFile(string sf, string extName)
        {
            // #1 Get MD5 Filename
            byte[] output = null;
            using (FileStream fs = new FileStream(sf, FileMode.Open))
            {
                output = md5.ComputeHash(fs);
            }
            string newFileName = BitConverter.ToString(output).Replace("-", "") + extName;


            // #2 Get Taken DateTime
            string outDirName = "UNKNOWN";

            try
            {

                MediaFile mdd = new MediaFile(sf);
                DateTime datePictureTaken = mdd.Video[0].EncodedDate;

                outDirName = string.Format("Movie\\{0}-{1}", datePictureTaken.Year.ToString("D4"), datePictureTaken.Month.ToString("D2"));
            }
            catch (Exception)
            {
            }


            // #3 Create directory if not exists
            Directory.CreateDirectory(this.targetPath + outDirName);


            string nf = "";
            if (extName == ".mts" || extName == ".avi" || extName == ".mpg")
            {
                nf = System.IO.Path.GetFileName(sf);
            }
            else nf = newFileName + extName;


            // Move File
            File.Move(sf, this.targetPath + outDirName + @"\" + nf);
        }


        private void GetAllDirList(string strBaseDir)
        {
            DirectoryInfo di = new DirectoryInfo(strBaseDir);


            // #1.1 Get All Files
            FileInfo[] fis = di.GetFiles("*.jpg");
            foreach(FileInfo fi in fis)
            {
                ProcessOnFile(fi.FullName, ".jpg");
            }

            // #1.2 Get All MTS File
            fis = di.GetFiles("*.mts");
            foreach (FileInfo fi in fis)
            {
                ProcessMovieFile(fi.FullName, ".mts");
            }

            // #1.3 Get All MOV File
            fis = di.GetFiles("*.mov");
            foreach (FileInfo fi in fis)
            {
                ProcessMovieFile(fi.FullName, ".mov");
            }

            // #1.4 Get All MOV File
            fis = di.GetFiles("*.mp4");
            foreach (FileInfo fi in fis)
            {
                ProcessMovieFile(fi.FullName, ".mp4");
            }

            // #1.5 Get All MOV File
            fis = di.GetFiles("*.avi");
            foreach (FileInfo fi in fis)
            {
                ProcessMovieFile(fi.FullName, ".avi");
            }


            // #1.6 Get All MOV File
            fis = di.GetFiles("*.mpg");
            foreach (FileInfo fi in fis)
            {
                ProcessMovieFile(fi.FullName, ".mpg");
            }


            // #2 Recursive to directory
            foreach (DirectoryInfo subdi in di.GetDirectories())
            {
                GetAllDirList(subdi.FullName);
            }
        }


        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFilenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];


            foreach(string sf in droppedFilenames)
            {
                FileAttributes attr = File.GetAttributes(sf);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    GetAllDirList(sf);
                }
                else
                {
                    string ext = System.IO.Path.GetExtension(sf).ToLower();
                    //if (ext == ".jpg" || ext == ".png") ProcessOnFile(sf, extName);
                    if (ext == ".jpg") ProcessOnFile(sf, ext);

                }

            }


        }
    }
}
