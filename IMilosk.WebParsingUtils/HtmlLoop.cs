using System.Globalization;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace IMilosk.WebParsingUtils;

public class HtmlLoop
{
    private readonly ILogger _logger;
    private readonly HtmlWeb _htmlWeb = new();
    private IBrowser? _browser;

    public HtmlLoop(ILogger logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<IEnumerable<T>> Parse<T>(
        Uri baseUrl,
        string[] navigation,
        string mainElementXPath,
        string nextPageXPath,
        CultureInfo cultureInfo,
        bool useJs,
        Func<XPathNavigator, T> delegateAction
    )
    {
        var currentPage = baseUrl;

        var items = new List<T>();

        do
        {
            var htmlDocument = await LoadHtmlDocument(currentPage, useJs);
            var rootNode = htmlDocument.DocumentNode ?? throw new Exception("Root element is null");

            foreach (var navigationXpath in navigation)
            {
                var navigationPages = rootNode.SelectNodes(navigationXpath) ??
                                      throw new Exception("Navigation pages not found");

                foreach (var navigationPage in navigationPages)
                {
                    var navigationUrl = ExtractNavigationUrl(baseUrl, navigationPage);
                    await foreach (var result in Parse(navigationUrl, navigation[..^1], mainElementXPath,
                                       nextPageXPath, cultureInfo, useJs, delegateAction))
                    {
                        items.AddRange(result);
                    }
                }
            }

            if (navigation.Length == 0)
            {
                var results = ScrapePage(rootNode, mainElementXPath, delegateAction);
                items.AddRange(results);
                yield return items;

                _logger.LogInformation("Scraped page: {page}", currentPage.ToString());
            }
            else
            {
                currentPage = GetNextPageUrl(rootNode, baseUrl, nextPageXPath, cultureInfo);
            }
        } while (currentPage != baseUrl);
    }

    private async Task<HtmlDocument> LoadHtmlDocument(Uri url, bool useJs)
    {
        return useJs ? await LoadHtmlUsingJs(url) : _htmlWeb.Load(url);
    }

    private async Task<HtmlDocument> LoadHtmlUsingJs(Uri url)
    {
        await using var browser = await GetOrInitiateBrowser();

        var page = await browser.NewPageAsync();
        await page.GoToAsync(url.ToString());

        // TODO: Figure out something better
        await Task.Delay(1000);

        var htmlContent = await page.GetContentAsync();
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        return htmlDocument;
    }

    private async Task<IBrowser> GetOrInitiateBrowser()
    {
        if (_browser is not null)
        {
            return _browser;
        }

        await new BrowserFetcher().DownloadAsync();

        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args =
            [
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--disable-extensions",
                "--disable-web-security",
                "--ignore-certificate-errors"
            ]
        });

        return _browser;
    }

    private static Uri ExtractNavigationUrl(Uri baseUrl, HtmlNode navigationPage)
    {
        var navigator = navigationPage.CreateNavigator() ?? throw new Exception("Cannot create navigator");
        var relativeUrl = navigator.GetAttribute("href", "");

        return UriConverter.ToAbsoluteUrl(baseUrl, relativeUrl);
    }

    private static Uri GetNextPageUrl(HtmlNode rootNode, Uri baseUrl, string nextPageXPath, CultureInfo cultureInfo)
    {
        var navigator = rootNode.CreateNavigator() ?? throw new Exception("Cannot create navigator");
        var nextPageUrl = navigator.GetValueOrDefault(nextPageXPath, string.Empty, cultureInfo);
        return UriConverter.ToAbsoluteUrl(baseUrl, nextPageUrl);
    }

    private static IEnumerable<T> ScrapePage<T>(
        HtmlNode rootNode,
        string mainElementXPath,
        Func<XPathNavigator, T> delegateAction
    )
    {
        mainElementXPath = string.IsNullOrWhiteSpace(mainElementXPath) ? "/" : mainElementXPath;
        var nodes = rootNode.SelectNodes(mainElementXPath);

        if (nodes is null)
        {
            yield break;
        }

        foreach (var node in nodes)
        {
            var navigator = node.CreateNavigator() ?? throw new Exception("Node navigator is null");

            yield return delegateAction(navigator);
        }
    }
}