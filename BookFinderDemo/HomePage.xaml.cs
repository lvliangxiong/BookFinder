using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BookFinderDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            this.ViewModel = new BookViewModel();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }


        public BookViewModel ViewModel { get; set; }


        private void PopularBookCollections_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(DetailPage), e.ClickedItem);
        }
    }

    public class BookViewModel
    {
        private ObservableCollection<Book> popularBooks = new ObservableCollection<Book>();
        public ObservableCollection<Book> PopularBooks { get { return this.popularBooks; } }

        //constructor
        public BookViewModel()
        {
            //Attention here, the private field was bound together, Changes will impact both of them.
            BOkCrawler.GetPopularBooks();
            this.popularBooks = BOkCrawler.PopularBooks;

        }

    }

}
