using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BookFinderDemo
{
    public enum FileFormat { UNKNOW, PDF, EPUB, MOBI, ZIP };

    public class Book
    {
        public string title;
        public FileFormat fileFormat;
        public int year;
        //unit KB
        public double fileSize;
        public Uri coverLink;
        public Uri downloadLink;
        public Uri detailPageLink;

        public bool isInfoComplete = false;

        public string GetFileFormatString() {
            string fileFormatString = "unknow";
            switch (fileFormat)
            {
                case FileFormat.UNKNOW:
                    fileFormatString = "unknow";
                    break;
                case FileFormat.PDF:
                    fileFormatString = "pdf";
                    break;
                case FileFormat.EPUB:
                    fileFormatString = "epub";
                    break;
                case FileFormat.MOBI:
                    fileFormatString = "mobi";
                    break;
                default:
                    break;
            }

            return fileFormatString;

        }

        public Book()
        {

        }

        public Book(string t, FileFormat fF, int y, double fS, Uri cL, Uri dL, Uri dPL)
        {
            title = t;
            fileFormat = fF;
            year = y;
            fileSize = fS;
            coverLink = cL;
            downloadLink = dL;
            detailPageLink = dPL;
            isInfoComplete = true;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ContentFrame.Navigate(typeof(HomePage));
            Nav.IsBackEnabled = ContentFrame.CanGoBack;
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Nav.IsBackEnabled = ContentFrame.CanGoBack;
        }

        private bool On_BackRequested()
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                return true;
            }
            return false;
        }

        private void Nav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }
    }




}
