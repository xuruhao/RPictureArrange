using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            this.AllowDrop = true;


            
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

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFilenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
        }
    }
}
