using System.Collections.Generic;
using CompoundingPerf.Features;
using SPTarkov.Server.Core.DI;
using Xunit;

namespace CompoundingPerf.Tests;

/// <summary>
/// Pins the cacheability decision logic. The actual cache (and DI override) require a
/// live SPT container, but the path-whitelist and body-emptiness checks are pure and
/// worth covering in isolation — they're the rules that decide whether a per-session
/// endpoint accidentally lands in the global cache.
/// </summary>
public class CachingHttpRouterTests
{
    private static CachingHttpRouter MakeRouter(ResponseCacheOptions? opts = null)
    {
        // CachingHttpRouter's base ctor accepts empty router enumerables — fine for
        // testing the decision method which never dispatches.
        var router = new CachingHttpRouter(
            new List<StaticRouter>(),
            new List<DynamicRouter>());
        router.Configure(opts ?? new ResponseCacheOptions { Enabled = true });
        return router;
    }

    [Theory]
    [InlineData("/client/items",                       "", true)]
    [InlineData("/client/handbook/templates",          "", true)]
    [InlineData("/client/globals",                     "", true)]
    [InlineData("/client/hideout/production/recipes",  "", true)]
    [InlineData("/client/locations",                   "", true)]
    public void Whitelisted_paths_with_empty_body_are_cacheable(string path, string body, bool expected)
    {
        var router = MakeRouter();
        Assert.Equal(expected, router.IsCacheableForTesting(path, body));
    }

    [Theory]
    [InlineData("/client/profile/info",          "", false)]   // per-session
    [InlineData("/client/profile/status",        "", false)]   // per-session
    [InlineData("/client/quest/list",            "", false)]   // per-session
    [InlineData("/client/customization/storage", "", false)]   // per-session
    [InlineData("/client/match/local/end",       "", false)]   // raid state
    [InlineData("/client/game/profile/items/moving", "", false)] // mutation
    [InlineData("/client/items/prices/abc123",   "", false)]   // dynamic prices
    [InlineData("/random/unknown/path",          "", false)]   // not in whitelist
    public void Non_whitelisted_paths_are_not_cacheable(string path, string body, bool expected)
    {
        var router = MakeRouter();
        Assert.Equal(expected, router.IsCacheableForTesting(path, body));
    }

    [Theory]
    [InlineData("/client/items", "{\"foo\":1}", false)]
    [InlineData("/client/items", "   ",         true)]   // whitespace-only is fine
    [InlineData("/client/items", "",            true)]
    public void Non_empty_body_disqualifies_caching(string path, string body, bool expected)
    {
        var router = MakeRouter();
        Assert.Equal(expected, router.IsCacheableForTesting(path, body));
    }

    [Fact]
    public void AdditionalPaths_extend_the_whitelist()
    {
        var router = MakeRouter(new ResponseCacheOptions
        {
            Enabled = true,
            AdditionalPaths = new List<string> { "/custom/endpoint", "/another/one" },
        });

        Assert.True(router.IsCacheableForTesting("/custom/endpoint", ""));
        Assert.True(router.IsCacheableForTesting("/another/one", ""));
        // Defaults still in effect:
        Assert.True(router.IsCacheableForTesting("/client/items", ""));
        // Unknown still rejected:
        Assert.False(router.IsCacheableForTesting("/unknown/path", ""));
    }

    [Fact]
    public void Default_whitelist_has_no_per_session_endpoints()
    {
        // Belt and suspenders — if a future refactor accidentally adds a per-session
        // endpoint to the default whitelist, this test fails loudly. Anything matching
        // /profile, /quest, /match, /game/profile, /items/prices is per-session.
        var bannedPatterns = new[] {
            "/profile/", "/quest/", "/match/", "/game/profile", "/items/prices",
            "/insurance/", "/mail/", "/friend/", "/notifier/",
        };
        foreach (var path in CachingHttpRouter.DefaultCacheablePaths)
        {
            foreach (var bad in bannedPatterns)
            {
                Assert.False(path.Contains(bad),
                    $"Default whitelist contains per-session-looking path '{path}' (matched ban pattern '{bad}')");
            }
        }
    }
}
