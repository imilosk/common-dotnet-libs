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
        string[] navigation =
        [
            "//*[@class='o']//li//a/@href"
        ];

        var pages = _htmlLoop.Parse(
            _baseUrl,
            navigation,
            "",
            "",
            CultureInfo.InvariantCulture,
            false,
            ParseArticle
        );

        await foreach (var page in pages)
        {
            Assert.IsType<Article>(page);
        }
    }

    [Fact]
    public async Task Test2()
    {
        _baseUrl = new Uri("https://www.stevejgordon.co.uk");
        string[] navigation =
        [
        ];

        var pages = _htmlLoop.Parse(
            _baseUrl,
            navigation,
            "//article",
            "",
            CultureInfo.InvariantCulture,
            false,
            ParseArticle
        );

        await foreach (var page in pages)
        {
            Assert.IsType<Article>(page);
        }
    }

    private Article ParseArticle(XPathNavigator navigator)
    {
        var article = new Article
        {
            Title =
                navigator
                    .GetValueOrDefault("descendant::*[contains(@class, 'entry-title')]//*//text()",
                        string.Empty,
                        _defaultCultureInfo),
            Link = new Uri("https://www.meziantou.net/using-mutex-t-to-synchronize-access-to-a-shared-resource.htm")
        };

        return article;
    }
}