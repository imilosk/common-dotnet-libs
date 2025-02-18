using System.Globalization;
using System.Text;
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
        _htmlWeb.OverrideEncoding = Encoding.UTF8;
    }

    public async IAsyncEnumerable<T> Parse<T>(
        Uri baseUrl,
        Navigation[] navigationLevels,
        CultureInfo cultureInfo,
        bool useJs,
        Func<XPathNavigator, Uri, T> delegateAction
    )
    {
        await foreach (var item in Parse(baseUrl, navigationLevels, cultureInfo, useJs, delegateAction,
                           navigationLevels.Length))
        {
            yield return item;
        }
    }

    private async IAsyncEnumerable<T> Parse<T>(
        Uri baseUrl,
        Navigation[] navigationLevels,
        CultureInfo cultureInfo,
        bool useJs,
        Func<XPathNavigator, Uri, T> delegateAction,
        int depth
    )
    {
        var currentPage = baseUrl;

        var navigation = navigationLevels[0];

        var mainElementXPath = navigation.MainElementXPath;
        var nextPageXPath = navigation.NextPageXPath;

        do
        {
            var htmlDocument = await LoadHtmlDocument(currentPage, useJs);
            var rootNode = htmlDocument.DocumentNode ?? throw new Exception("Root element is null");

            var mainElements = ScrapePageForMainElements(rootNode, mainElementXPath);

            if (mainElements is null)
            {
                continue;
            }

            if (depth == 1)
            {
                foreach (var item in ScrapeElements(mainElements, baseUrl, delegateAction))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var mainElement in mainElements)
                {
                    var navigationUrl = ExtractNavigationUrl(baseUrl, mainElement);

                    await foreach (var item in Parse(
                                       navigationUrl,
                                       navigationLevels[1..],
                                       cultureInfo,
                                       useJs,
                                       delegateAction,
                                       depth - 1
                                   ))
                    {
                        yield return item;
                    }
                }
            }

            if (string.IsNullOrEmpty(nextPageXPath))
            {
                continue;
            }

            currentPage = GetNextPageUrl(rootNode, baseUrl, nextPageXPath, cultureInfo);
        } while (currentPage != baseUrl && currentPage.PathAndQuery is not ("/" or ""));
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

    private static HtmlNodeCollection? ScrapePageForMainElements(
        HtmlNode rootNode,
        string mainElementXPath
    )
    {
        mainElementXPath = string.IsNullOrWhiteSpace(mainElementXPath) ? "/" : mainElementXPath;

        return rootNode.SelectNodes(mainElementXPath);
    }

    private static IEnumerable<T> ScrapeElements<T>(
        HtmlNodeCollection? elements,
        Uri baseUrl,
        Func<XPathNavigator, Uri, T> delegateAction
    )
    {
        if (elements is null)
        {
            yield break;
        }

        foreach (var node in elements)
        {
            if (node is null)
            {
                continue;
            }

            var navigator = node.CreateNavigator() ?? throw new Exception("Navigator is null");

            yield return delegateAction(navigator, baseUrl);
        }
    }
}