using System.Globalization;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Moq;

namespace IMilosk.WebParsingUtils.UnitTests;

public class HtmlLoopTests
{
    private readonly HtmlLoop _htmlLoop;
    private Uri _baseUrl = null!;
    private readonly CultureInfo _defaultCultureInfo = CultureInfo.InvariantCulture;

    public HtmlLoopTests()
    {
        var mock = new Mock<ILogger>();
        var logger = mock.Object;

        _htmlLoop = new HtmlLoop(logger);
    }

    [Fact]
    public async Task Test1()
    {
        _baseUrl = new Uri("https://www.meziantou.net/archives.htm");
        Navigation[] navigationLevels =
        [
            new Navigation
            {
                MainElementXPath = "//*[@class='o']//li//a/@href",
                NextPageXPath = string.Empty,
            },
            new Navigation
            {
                MainElementXPath = "/",
            },
        ];

        var articles = _htmlLoop.Parse(
            _baseUrl,
            navigationLevels,
            _defaultCultureInfo,
            false,
            ParseArticle
        );

        await foreach (var article in articles)
        {
            Assert.IsType<Article>(article);
        }
    }

    [Fact]
    public async Task Test2()
    {
        _baseUrl = new Uri("https://ayende.com/blog/");

        Navigation[] navigationLevels =
        [
            new Navigation
            {
                MainElementXPath = "//article",
                NextPageXPath = "//*[contains(@class, 'next')]//a/@href",
            }
        ];

        var articles = _htmlLoop.Parse(
            _baseUrl,
            navigationLevels,
            _defaultCultureInfo,
            false,
            ParseArticle
        );

        await foreach (var article in articles)
        {
            Assert.IsType<Article>(article);
        }
    }

    private Article ParseArticle(XPathNavigator navigator, Uri uri)
    {
        var article = new Article
        {
            Title =
                navigator
                    .GetValueOrDefault("//article//header//h1/text()", string.Empty, _defaultCultureInfo),
            Summary = StripHtml.StripHtmlTagsRegex().Replace(
                navigator.GetValueOrDefault("substring(//article/div, 0, 300)", string.Empty, _defaultCultureInfo),
                string.Empty),
            Author = navigator.GetValueOrDefault("//article[@class='v']//li", string.Empty, _defaultCultureInfo),
            Link = uri,
            PublishDate =
                navigator.GetValueOrDefault("//div[@class='x']//time", DateTime.MinValue, _defaultCultureInfo),
            LastUpdatedTime =
                navigator.GetValueOrDefault("//div[@class='x']//time", DateTime.MinValue, _defaultCultureInfo),
            Source = "XPath",
        };

        return article;
    }
}