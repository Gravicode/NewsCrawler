using Abot2.Crawler;
using Abot2.Poco;
using HtmlAgilityPack;
using Newtonsoft.Json;

var crawl = new WebCrawler();
var url = "https://kompas.com/";
crawl.Start(url);
Console.ReadLine();

public class WebCrawler
{
    List<Crawled>? CrawledItems { set; get; }
    string[] topics = new string[] { "jokowi", "ikn", "ibukota" };
    public void Start(string UrlWeb)
    {
        Console.WriteLine("start crawler..");
        CrawledItems = new List<Crawled>();
        DoCrawl(UrlWeb);
        Console.ReadLine();
    }

    async void DoCrawl(string UrlWeb)
    {
        CrawlConfiguration crawlConfig = new CrawlConfiguration();
        crawlConfig.CrawlTimeoutSeconds = 100;
        crawlConfig.MaxConcurrentThreads = 10;
        crawlConfig.MaxPagesToCrawl = 5000;
        crawlConfig.UserAgentString = "abot v1.0 http://code.google.com/p/abot";

        //Will use app.config for confguration
        PoliteWebCrawler crawler = new PoliteWebCrawler();

        crawler.PageCrawlStarting += Crawler_PageCrawlStarting;
        crawler.PageCrawlCompleted += Crawler_PageCrawlCompleted;
        crawler.PageCrawlDisallowed += Crawler_PageCrawlDisallowed;
        crawler.PageLinksCrawlDisallowed += Crawler_PageLinksCrawlDisallowed;

        CrawlResult result = await crawler.CrawlAsync(new Uri(UrlWeb));
        Console.WriteLine("jumlah crawled content :" + result.CrawlContext.CrawledCount);
        if (result.ErrorOccurred)
            Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
        else
            Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);
        //save result
        SaveToDB();
    }

    void Crawler_PageLinksCrawlDisallowed(object? sender, PageLinksCrawlDisallowedArgs e)
    {
        CrawledPage crawledPage = e.CrawledPage;
        Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
    }

    void Crawler_PageCrawlDisallowed(object? sender, PageCrawlDisallowedArgs e)
    {
        PageToCrawl pageToCrawl = e.PageToCrawl;
        Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
    }

    void Crawler_PageCrawlCompleted(object? sender, PageCrawlCompletedArgs e)
    {
        CrawledPage crawledPage = e.CrawledPage;
        if (isContains(crawledPage.Uri.AbsoluteUri))
        {
            var item = new Crawled() { Url = crawledPage.Uri.AbsoluteUri, Description = crawledPage.Content.Text };
            CrawledItems.Add(item);
            Console.WriteLine($"{item.Url} => ");

        }
         if (crawledPage.HttpRequestException != null || crawledPage.HttpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
            Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);

        Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);

        if (string.IsNullOrEmpty(crawledPage.Content.Text))
            Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
    }

    void Crawler_PageCrawlStarting(object? sender, PageCrawlStartingArgs e)
    {
        PageToCrawl pageToCrawl = e.PageToCrawl;
        Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
    }

    /// <summary>
    /// Untuk filter url 
    /// </summary>
    /// <param name="URL"></param>
    /// <returns></returns>
    bool isContains(string URL)
    {

        if (string.IsNullOrEmpty(URL)) return false;
        foreach(var topic in topics)
        {
            if (URL.Contains(topic, StringComparison.InvariantCultureIgnoreCase)) return true;
        }
        return false;
    }

    /// <summary>
    /// Untuk save ke file atau db
    /// </summary>
    /// <param name="SaveToFile"></param>
    void SaveToDB(bool SaveToFile=false)
    {
        if (SaveToFile)
        {
            //save to file 
            var fileContent = JsonConvert.SerializeObject(CrawledItems);
            File.WriteAllText("crawled-content.txt", fileContent);

        }
        else
        {
            var htmlDoc = new HtmlDocument();
            foreach (var item in CrawledItems)
            {
                //get body
                htmlDoc.LoadHtml(item.Description);
                item.Description = htmlDoc.DocumentNode.SelectSingleNode("//body").InnerText;

                //save to db, lengkapi sendiri
            }
        }

    }


}
public class Crawled
{
    public string Url { get; set; }
    public string Description { get; set; }

}