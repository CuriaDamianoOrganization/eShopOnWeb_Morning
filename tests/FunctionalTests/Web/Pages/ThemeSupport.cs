using Microsoft.eShopWeb.FunctionalTests.Web;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.WebRazorPages;

[Collection("Sequential")]
public class ThemeSupport : IClassFixture<TestApplication>
{
    public ThemeSupport(TestApplication factory)
    {
        Client = factory.CreateClient();
    }

    public HttpClient Client { get; }

    [Fact]
    public async Task HomePageIncludesBrandNameAndThemeSwitcher()
    {
        var response = await Client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var decoded = System.Net.WebUtility.HtmlDecode(html);

        Assert.Contains("DAMIANO's SHOP", decoded);
        Assert.DoesNotContain("Microsoft.eShopOnWeb", html);
        Assert.Contains("esh-theme-switcher", html);
        Assert.Contains("data-theme-value=\"light\"", html);
        Assert.Contains("data-theme-value=\"dark\"", html);
        Assert.Contains("data-theme-value=\"system\"", html);
        Assert.Contains("css/theme.css", html);
        Assert.Contains("js/theme.js", html);
        Assert.Contains("eshop-theme", html);
    }

    [Fact]
    public async Task ThemeAssetsSupportLightDarkAndSystemPreferences()
    {
        var cssResponse = await Client.GetAsync("/css/theme.css");
        cssResponse.EnsureSuccessStatusCode();
        var css = await cssResponse.Content.ReadAsStringAsync();
        Assert.Contains("html[data-theme=\"dark\"]", css);
        Assert.Contains("prefers-color-scheme: dark", css);
        Assert.Contains("--esh-bg", css);

        var jsResponse = await Client.GetAsync("/js/theme.js");
        jsResponse.EnsureSuccessStatusCode();
        var js = await jsResponse.Content.ReadAsStringAsync();
        Assert.Contains("eshop-theme", js);
        Assert.Contains("\"light\"", js);
        Assert.Contains("\"dark\"", js);
        Assert.Contains("\"system\"", js);
        Assert.Contains("prefers-color-scheme: dark", js);
    }
}
