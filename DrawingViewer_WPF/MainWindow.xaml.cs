using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using System.IO;
using System.IO.Compression;

namespace DrawingViewer_WPF
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>

    public class Project
    {
        public Project()
        {
            this.Tasks = new ObservableCollection<ProjectTask>();
        }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public ObservableCollection<ProjectTask> Tasks { get; set; }
    }

    public class ProjectTask
    {
        public string Name { get; set; }

    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            HideSubMenus();
            if (isDebug)
            {
                WelcomingLabel.Content = "DebugMode ON!";
                Test.Visibility = Visibility.Visible;
            }

            List<Project> projects = new List<Project>();

            Project project1 = new Project() { Name = "Panda" };
            project1.Tasks.Add(new ProjectTask() { Name = "Foots" });
            project1.Tasks.Add(new ProjectTask() { Name = "Ears" });
            project1.Tasks.Add(new ProjectTask() { Name = "Fur" });
            project1.Tasks.Add(new ProjectTask() { Name = "BackgroundwithRellyBigTrees" });
            projects.Add(project1);

            Project project2 = new Project() { Name = "DeepSeaMonsterWithSharpTooths" };
            project2.Tasks.Add(new ProjectTask() { Name = "Head" });
            project2.Tasks.Add(new ProjectTask() { Name = "Tooths" });
            project2.Tasks.Add(new ProjectTask() { Name = "Forground" });
            projects.Add(project2);

            Tree.ItemsSource = projects;
        }

#if DEBUG
        public static bool isDebug = true;
        public static string resourcesDirectory =
             @"C:\Users\jakpo\source\repos\DrawingViewer_WPF\Resources\";
#else
        public static bool isDebug = false;
        public static string resourcesDirectory =
            @"C:\Projekty\TempFiles\";
#endif


        //// Decompression ////
        public void Decompress(string kraPath, string FileName)
        {
            string decompressPath = resourcesDirectory + @"DecompressedFiles\" + FileName.Remove(FileName.Length - 4);     // kraPath.Remove(kraPath.Length - 4);
            if (File.Exists(kraPath) && Directory.Exists(decompressPath) == false)
            {
                ZipFile.ExtractToDirectory(kraPath, decompressPath);

                //MessageBox.Show($"File from path: \n {kraPath} \nis decompressed to: \n {decompressPath}.", 
                //                "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadImage(decompressPath + "\\mergedimage.png", FileName);

                if (isDebug == false)
                {
                    //// Delete decompressed Directory ////
                    if (Directory.Exists(decompressPath))
                    {
                        Directory.Delete(decompressPath, true);
                    }
                }
            }
            else
            {
                //MessageBox.Show("Program can't decompress this file. \n" +
                //                "Try another file or delete directory named same as this file.", "Warning!",
                //                MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadImage(decompressPath + "\\mergedimage.png", FileName);
            }
        }

        //// Load Image ////
        public void LoadImage(string FilePath, string FileName)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(FilePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            Image1.Source = bitmap;
            NameLabel.Text = FileName;
            PathLabel.Text = FilePath;
            Reset();
        }

        //// Browse files //// 
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //// Open Explorer ////
            OpenFileDialog openDlg = new OpenFileDialog
            {
                Filter = "Krita files (*.kra)|*.kra" +
                         "|Image files (*.jpg; *.png; *.btm; *.gif)" +
                         "|*.jpg;*.png;*.btm;*.gif" +
                         "|All Files (*.*)|*.*"
            };
            if (openDlg.ShowDialog() == IsActive)
            {
                string FilePath = openDlg.FileName;
                string FileName = openDlg.SafeFileName;
                
                //// Open .kra file ////
                if(openDlg.SafeFileName.EndsWith(".kra"))
                {
                    //MessageBox.Show("File extension is \".kra\". ");
                    Decompress(FilePath, FileName);
                }

                //// Open other than .kra file //// 
                else
                {
                    LoadImage(FilePath, FileName);
                }
            }
        }

        //// Border Action ////

        //// Pan & Zoom ////
        private Point Pos;
        private void Border1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //// Pan starting position ////
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Pos = e.GetPosition(Image1);
            }

            //// Zoom & Pan Reset ////
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Reset();
            }
        }

        //// Pan (hand) ////
        private void Border1_MouseMove(object sender, MouseEventArgs e)
        {
            Point nPos;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                nPos = e.GetPosition(Canvas1);
                Canvas.SetLeft(Image1, (nPos.X - Pos.X));
                Canvas.SetTop(Image1, (nPos.Y - Pos.Y));
                Test.Text = " Pos: " + Pos.ToString() + "\n nPos: " + nPos.ToString();
            }
        }

        //// Zoom ////
        private void Border1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (e.Delta > 0) { SliderZoom.Value += SliderZoom.SmallChange; }
            else if (e.Delta < 0) { SliderZoom.Value -= SliderZoom.SmallChange; }
        }

        //// Reset Zoom ////
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        //// Reset Zoom & Pan ////
        public void Reset()
        {
            SliderZoom.Value = 1;
            Canvas.SetLeft(Image1, 0);
            Canvas.SetTop(Image1, 0);
        }

        //// Hide ZoomPanel ////
        public bool IsWindowSmall()
        {
            if ((MainWindow1.Height < 200 || MainWindow1.Width < 600) 
                && MainWindow1.WindowState == WindowState.Normal)
            {
                //MessageBox.Show("Size of window is too small. ", "Warrning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            else { return false; }
        }

        private void MainWindow1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsWindowSmall())
            {
                ZoomPanel.Visibility = Visibility.Collapsed;
                //MessageBox.Show("Size of window is too small. " + 
                //                MainWindow1.Height.ToString() + " x " + MainWindow1.Width.ToString(), 
                //                "Warrning!", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            else { ZoomPanel.Visibility = Visibility.Visible; }
        }

        private void MainWindow1_StateChanged(object sender, EventArgs e)
        {

            if (MainWindow1.WindowState == WindowState.Maximized)
            {
                //MessageBox.Show("Size of window is " + MainWindow1.WindowState.ToString(),
                //                "InfoBox", MessageBoxButton.OK, MessageBoxImage.Information);
                ZoomPanel.Visibility = Visibility.Visible;
                //MessageBox.Show("ZoomPanel Visibility: " + ZoomPanel.Visibility.ToString(),
                //                "InfoBox", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //// Delete temp files ////
        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            if (isDebug)
            {
                //// Delete decompressed Directory ////
                string decompressPath = resourcesDirectory + @"DecompressedFiles\";
                if (Directory.Exists(decompressPath))
                {
                    Directory.Delete(decompressPath, true);
                }
            }
        }

        //// Show/Hide SubMenu Panels ////
        private void HideSubMenus()
        {
            StackPanel_Properties.Visibility = Visibility.Collapsed;
            StackPanel_List.Visibility = Visibility.Collapsed;
        }

        private void Btn_Properties_Click(object sender, RoutedEventArgs e)
        {
            if(StackPanel_Properties.Visibility == Visibility.Collapsed)
            {
                StackPanel_Properties.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_Properties.Visibility = Visibility.Collapsed;
            }
        }

        private void Btn_List_Click(object sender, RoutedEventArgs e)
        {
            if(StackPanel_List.Visibility == Visibility.Collapsed)
            {
                StackPanel_List.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_List.Visibility = Visibility.Collapsed;
            }
        }

        //// Add new Project to the List ////
        private void RBtn_Big_Click(object sender, RoutedEventArgs e)
        {

        }

        //// Add new Item to the Project ////
        private void RBtn_Small_Click(object sender, RoutedEventArgs e)
        {

        }


        /* TO DO:
         * -Add list view;
         * -Add struct (name, file, childs);
         * -Add methodes for loading and saving list;
         * -Add sublist with round buttons;
         * -Add methode for showing & hiding sublist;
         * -Add dialog window with struct editor;
         * -Add Buttons click events;
         * -Add methodes for adding, deleting and moving items;
         * -Add methode for list order changing;
         * -Add methode for opening conected file;
         * -Improve Pan;
         * -Add Styles;
         * -Fix Slider style;
         * -Designe better UI;
         * -
         * -Add layer viewer;
         * -
         * -Add ToolTips;
         * -Clear the code;
         * -Transfer to mobile platform;
         */
    }
}
