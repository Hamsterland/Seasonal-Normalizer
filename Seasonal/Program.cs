// USAGE EXAMPLES
// .\Seasonal.exe https://myanimelist.net/anime/season
// .\Seasonal.exe https://myanimelist.net/anime/season/2021/summer

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

if (args.Length is not 1)
    throw new InvalidOperationException("More than one command line argument detected. Please only provide the MyAnimeList seasonal page URL.");

var url = args[0];
var _client = new HttpClient();
var _seasonals = new List<Seasonal>();

var html = await _client.GetStringAsync(url);
var doc = new HtmlDocument();
doc.LoadHtml(html);

var nodes = doc.QuerySelectorAll("#content > div.js-categories-seasonal > div:nth-child(1)");

foreach (var n in nodes)
{
    foreach (var seasonal in n.ChildNodes)
    {
        if (seasonal.ChildNodes.Count < 4)
            continue;

        var node = seasonal.ChildNodes[2];
        
        var title = node
            .InnerText
            .Trim();

        var link = node
            .ChildNodes[0]
            .Attributes
            .First()
            .Value
            .Trim();

        var members = seasonal
            .ChildNodes[6]
            .ChildNodes[3]
            .ChildNodes[1]
            .InnerText
            .Replace("\n", string.Empty)
            .Replace(",", string.Empty)
            .Trim();

        _seasonals.Add(new Seasonal
        {
            Name = title,
            URL = link,
            Members = int.Parse(members)
        });
    }
}

var min = _seasonals.Min(x => x.Members);
var max = _seasonals.Max(x => x.Members);

_seasonals = _seasonals.OrderBy(x => x.Members).ToList();
_seasonals.Reverse();

foreach (var seasonal in _seasonals)
{
    seasonal.Normalised = ((float) seasonal.Members - min) / (max - min);
    Console.WriteLine($"Name: {seasonal.Name} ({seasonal.URL})");
    Console.WriteLine($"Members: {seasonal.Members}");
    Console.WriteLine($"Min-Max Normalised: {seasonal.Normalised:0.#########}");
    Console.WriteLine();
}

public class Seasonal
{
    public string Name { get; init; }
    public string URL { get; init; }
    public int Members { get; init; }
    public float Normalised { get; set; }
}
