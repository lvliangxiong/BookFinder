using HtmlAgilityPack;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BookFinderDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailPage : Page
    {
        private Book book = new Book();

        public DetailPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void Nav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            this.Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Book)
            {
                book = (Book)e.Parameter;

                Cover.Source = new BitmapImage(book.coverLink);
                Title.Visibility = Visibility.Collapsed;

            }
            else
            {
                Cover.Visibility = Visibility.Collapsed;
                Title.FontSize = 36;
                Title.Text = "BOOK NOT FOUND";
            }
            base.OnNavigatedTo(e);
        }



        private void ShowMenu(bool isTransient)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions();
            myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
            CommandBarFlyout1.ShowAt(Cover, myOption);
        }

        private void MyImageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenu((sender as Button).IsPointerOver);
        }

        private void MyImageButton_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            // always show a context menu in standard mode
            ShowMenu(false);
        }

        private async void OnElementClicked(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;

            switch (b.Name)
            {
                case "SaveButton":

                    StorageFile file = await DownloadBookAsync(book);
                    await ShowDownloadToastAsync(file);
                    break;

                case "ShareButton":
                    //StorageFile f = await DownloadsFolder.CreateFileAsync("demo.epub", CreationCollisionOption.GenerateUniqueName);
                    //await ShowDownloadToastAsync(f);
                    break;

                default:
                    break;
            }
        }

        private async Task ShowDownloadToastAsync(StorageFile file)
        {
            string title = "Book Downloaded Successfully!";
            string image = book.coverLink.ToString();

            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = title
                        },

                        new AdaptiveImage()
                        {
                            Source = image
                        }
                    },
                }

            };


            // Construct the actions for the toast (inputs and buttons)
            ToastActionsCustom actions = new ToastActionsCustom()
            {
                Buttons =
                {
                    new ToastButton("View", new QueryString()
                    {
                        { "action", "viewBook" },
                        { "filepath", Path.GetDirectoryName(file.Path) }

                    }.ToString())
                    {
                        ActivationType = ToastActivationType.Background
                    }
                }
            };

            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
                Actions = actions,
                // Arguments when the user taps body of toast
                Launch = new QueryString()
                {
                    { "action", "viewConversation" }
                }.ToString()
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml());

            toast.ExpirationTime = DateTime.Now.AddDays(2);

            ToastNotificationManager.CreateToastNotifier().Show(toast);

            const string taskName = "ToastBackgroundTask";

            // If background task is already registered, do nothing
            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(taskName)))
                return;

            // Otherwise request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // Create the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = taskName
            };

            // Assign the toast action trigger
            builder.SetTrigger(new ToastNotificationActionTrigger());

            // And register the task
            BackgroundTaskRegistration registration = builder.Register();


        }

        private async Task<StorageFile> DownloadBookAsync(Book book)
        {
            StorageFile file = await BOkCrawler.DownloadBookAsync(book);
            return file;
        }
    }
}
