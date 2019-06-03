using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace BookFinderDemo
{
    public static class BOkCrawler
    {
        private static Uri siteUri = new Uri("https://b-ok.cc/");
        private static Uri popularBooksUri = new Uri("https://b-ok.org/popular.php");


        private static ObservableCollection<Book> popularBooks = new ObservableCollection<Book>();
        public static ObservableCollection<Book> PopularBooks { get { return popularBooks; } }


        public static async Task<Book> GetBookInfoFromDetailPage(Uri detailPageUri)
        {
            Book book = new Book();

            HttpClient httpClient = new HttpClient();
            string html = await httpClient.GetStringAsync(detailPageUri);
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
             
            //Get the title of the book.
            HtmlNode titleH1 = htmlDocument.DocumentNode.Descendants("h1").
                Where(node =>
                {
                    if (node == null)
                    {
                        throw new ArgumentNullException(nameof(node));
                    }
                    return node.GetAttributeValue("itemprop", "").Equals("name");
                }).FirstOrDefault();

            if (titleH1 != null)
            {
                book.title = titleH1.InnerText;
            }

            //Get book cover link

            HtmlNode coverImage = htmlDocument.DocumentNode.Descendants("a").
                Where(node =>
                {
                    if (node == null)
                    {
                        throw new ArgumentNullException(nameof(node));
                    }
                    return node.GetAttributeValue("itemprop", "").Equals("image");
                }).FirstOrDefault();

            book.coverLink = new Uri(siteUri, coverImage.GetAttributeValue("href",""));

            //Get book download link

            HtmlNode downloadBn = htmlDocument.DocumentNode.Descendants("a").
                Where(node =>
                {
                    if (node == null)
                    {
                        throw new ArgumentNullException(nameof(node));
                    }
                    return node.GetAttributeValue("class", "").Equals("btn btn-primary dlButton");
                }).FirstOrDefault();
            book.downloadLink = new Uri(siteUri, downloadBn.GetAttributeValue("href", ""));

            //get book detail page link

            book.detailPageLink = detailPageUri;

            //Get the divs contains pubulish year, filesize & etc...
            HtmlNode divs = htmlDocument.DocumentNode.Descendants("div").
                Where(node =>
                {
                    if (node == null)
                    {
                        throw new ArgumentNullException(nameof(node));
                    }
                    return node.GetAttributeValue("class", "").Equals("bookDetailsBox");
                }).FirstOrDefault();

            //Get the publish year
            HtmlNode yearDiv = divs.Descendants("div").Where(node =>
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }
                return node.GetAttributeValue("class", "").Equals("bookProperty property_year");
            }).FirstOrDefault();

            if (yearDiv != null)
            {
                book.year = Convert.ToInt32(yearDiv.LastChild.InnerText.Trim());
            }

            //Get the file format and file size

            HtmlNode fileDiv = divs.Descendants("div").Where(node =>
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }
                return node.GetAttributeValue("class", "").Equals("bookProperty property__file");
            }).FirstOrDefault();

            if (fileDiv != null)
            {
                string fileInfo = fileDiv.LastChild.InnerText.Trim();

                if (fileInfo.ToUpper().Contains("EPUB"))
                {
                    book.fileFormat = FileFormat.EPUB;
                }
                else
                {
                    if (fileInfo.ToUpper().Contains("PDF"))
                    {
                        book.fileFormat = FileFormat.PDF;
                    }
                    else
                    {
                        if (fileInfo.ToUpper().Contains("MOBI"))
                        {
                            book.fileFormat = FileFormat.MOBI;
                        }
                    }
                }
            }

            book.isInfoComplete = true;

            return book;
        }


        public static async void GetPopularBooksCoverAsync()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string html = await httpClient.GetStringAsync(popularBooksUri);

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                List<HtmlNode> divs = htmlDocument.DocumentNode.Descendants("div")
                    .Where(node =>
                    {
                        if (node == null)
                        {
                            throw new ArgumentNullException(nameof(node));
                        }
                        return node.GetAttributeValue("class", "").Equals("brick");
                    }).ToList();

                foreach (HtmlNode div in divs)
                {
                    Book popularbook = new Book();
                    //Fetch the cover image of the book
                    popularbook.coverLink = new Uri(siteUri, div.Descendants("img").FirstOrDefault().ChildAttributes("src").FirstOrDefault().Value);
                    popularbook.detailPageLink = new Uri(siteUri, div.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value);

                    popularBooks.Add(popularbook);
                }
            }
            catch (Exception)
            {
                MessageDialog m = new MessageDialog("Network Error!");
                await m.ShowAsync();
            }
            
        }

        public static async void GetPopularBooksDetailsAsync()
        {
            for (int i = 1; i <= popularBooks.Count; i++)
            {
                var book = popularBooks[i-1];
                HttpClient httpClient = new HttpClient();
                string html = await httpClient.GetStringAsync(book.detailPageLink);
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                //Get the title of the book.
                HtmlNode titleH1 = htmlDocument.DocumentNode.Descendants("h1").
                    Where(node =>
                    {
                        if (node == null)
                        {
                            throw new ArgumentNullException(nameof(node));
                        }
                        return node.GetAttributeValue("itemprop", "").Equals("name");
                    }).FirstOrDefault();

                if (titleH1 != null)
                {
                    book.title = titleH1.InnerText;
                }

                //Get book download link

                HtmlNode downloadBn = htmlDocument.DocumentNode.Descendants("a").
                    Where(node =>
                    {
                        if (node == null)
                        {
                            throw new ArgumentNullException(nameof(node));
                        }
                        return node.GetAttributeValue("class", "").Equals("btn btn-primary dlButton");
                    }).FirstOrDefault();
                book.downloadLink = new Uri(siteUri, downloadBn.GetAttributeValue("href", ""));

                //Get the divs contains pubulish year, filesize & etc...
                HtmlNode divs = htmlDocument.DocumentNode.Descendants("div").
                    Where(node =>
                    {
                        if (node == null)
                        {
                            throw new ArgumentNullException(nameof(node));
                        }
                        return node.GetAttributeValue("class", "").Equals("bookDetailsBox");
                    }).FirstOrDefault();

                //Get the publish year
                HtmlNode yearDiv = divs.Descendants("div").Where(node =>
                {
                    if (node == null)
                    {
                        throw new ArgumentNullException(nameof(node));
                    }
                    return node.GetAttributeValue("class", "").Equals("bookProperty property_year");
                }).FirstOrDefault();

                if (yearDiv != null)
                {
                    book.year = Convert.ToInt32(yearDiv.LastChild.InnerText.Trim());
                }

                //Get the file format and file size
                
                HtmlNode fileDiv = divs.Descendants("div").Where(node =>
                {
                    if (node == null)
                    {
                        throw new ArgumentNullException(nameof(node));
                    }
                    return node.GetAttributeValue("class", "").Equals("bookProperty property__file");
                }).FirstOrDefault();

            if (fileDiv != null)
            {
                string fileInfo = fileDiv.LastChild.InnerText.Trim();

                if (fileInfo.ToUpper().Contains("EPUB"))
                {
                    book.fileFormat = FileFormat.EPUB;
                }
                else
                {
                    if (fileInfo.ToUpper().Contains("PDF"))
                    {
                        book.fileFormat = FileFormat.PDF;
                    }
                    else
                    {
                        if (fileInfo.ToUpper().Contains("MOBI"))
                        {
                            book.fileFormat = FileFormat.MOBI;
                        }
                    }
                }
            }


                book.isInfoComplete = true;
            }

        }

        public static void GetPopularBooks()
        {
            GetPopularBooksCoverAsync();
            //GetPopularBooksDetailsAsync();
        }

        public static async Task<bool> DownloadBookAsync(Book book)
        {
            if (book.detailPageLink == null)
            {
                MessageDialog m = new MessageDialog("Couldn't fetch the book info about its detail page!");
                await m.ShowAsync();
                return false;
            }

            if (book.downloadLink == null)
            {
                book = await BOkCrawler.GetBookInfoFromDetailPage(book.detailPageLink);
            }

            try
            {
                HttpWebRequest req = WebRequest.Create(book.downloadLink) as HttpWebRequest;

                //Headers definition
                req.Headers.Add(HttpRequestHeader.Referer, book.detailPageLink.ToString());
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";
                req.Method = "GET";
                req.Accept = "accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

                HttpWebResponse response = req.GetResponse() as HttpWebResponse;

                //When the response was ok
                var m = new MessageDialog("You have reached the limit of 5 downloads in 24 hours.");

                // Set the command that will be invoked by default
                m.DefaultCommandIndex = 0;

                // Set the command to be invoked when escape is pressed
                m.CancelCommandIndex = 1;

                // Show the message dialog
                await m.ShowAsync();


            }
            //When the response was 410
            catch (WebException e)
            {
                try
                {
                    Uri d = new Uri(e.Response.Headers.Get("Location").Replace("http","https"));


                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(d);

                    //Headers definition
                    req.AllowAutoRedirect =true;

                    req.Method = "GET";
                    req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";
                    req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
                    req.Host = "dl123.zlibcdn.com";

                    WebResponse response = await req.GetResponseAsync();

                    string contentDisposition = response.Headers.Get("Content-Disposition");

                    string pattern = "filename=\"(.+?)\"$";

                    string fileName = Regex.Matches(contentDisposition, pattern).First().Groups[1].ToString();

                    StorageFile destinationFile;

                    if (BookView.DownloadPath == null)
                    {
                        destinationFile = await DownloadsFolder.CreateFileAsync(GetSafeFilename(fileName), CreationCollisionOption.GenerateUniqueName);
                        BookView.DownloadPathString = Path.GetDirectoryName(destinationFile.Path);
                    }
                    else
                    {
                        destinationFile = await BookView.DownloadPath.CreateFileAsync(GetSafeFilename(fileName), CreationCollisionOption.GenerateUniqueName);
                    }

                    Stream s = response.GetResponseStream();
                    await Task.Run(()=> FileIO.WriteBytesAsync(destinationFile, ReadStream(s)));
                    return true;

                }
                catch (WebException)
                {
                    MessageDialog m = new MessageDialog("Couldn't download the file!");
                    await m.ShowAsync();
                }
            }
            return false;
        }

        private static byte[] ReadStream(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static string GetSafeFilename(string arbitraryString)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var replaceIndex = arbitraryString.IndexOfAny(invalidChars, 0);
            if (replaceIndex == -1) return arbitraryString;

            var r = new StringBuilder();
            var i = 0;

            do
            {
                r.Append(arbitraryString, i, replaceIndex - i);

                switch (arbitraryString[replaceIndex])
                {
                    case '"':
                        r.Append("''");
                        break;
                    case '<':
                        r.Append('\u02c2'); // '˂' (modifier letter left arrowhead)
                        break;
                    case '>':
                        r.Append('\u02c3'); // '˃' (modifier letter right arrowhead)
                        break;
                    case '|':
                        r.Append('\u2223'); // '∣' (divides)
                        break;
                    case ':':
                        r.Append('-');
                        break;
                    case '*':
                        r.Append('\u2217'); // '∗' (asterisk operator)
                        break;
                    case '\\':
                    case '/':
                        r.Append('\u2044'); // '⁄' (fraction slash)
                        break;
                    case '\0':
                    case '\f':
                    case '?':
                        break;
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\v':
                        r.Append(' ');
                        break;
                    default:
                        r.Append('_');
                        break;
                }

                i = replaceIndex + 1;
                replaceIndex = arbitraryString.IndexOfAny(invalidChars, i);
            } while (replaceIndex != -1);

            r.Append(arbitraryString, i, arbitraryString.Length - i);

            return r.ToString();
        }

    }

    public class BookView
    {
        private ObservableCollection<Book> popularBooks = new ObservableCollection<Book>();
        public ObservableCollection<Book> PopularBooks { get { return this.popularBooks; } }

        private static StorageFolder downloadPath;
        public static StorageFolder DownloadPath { get => downloadPath; }
        private static string downloadPathString;

        public static string DownloadPathString { get => (downloadPath == null) ? downloadPathString : downloadPath.Path; set => downloadPathString = value;  }


        //constructor
        public BookView()
        {
            //Attention here, the private field was bound together, Changes will impact both of them.
            BOkCrawler.GetPopularBooks();
            this.popularBooks = BOkCrawler.PopularBooks;
        }

    }

}
