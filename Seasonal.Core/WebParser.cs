using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Seasonal.Core
{
    public class WebParser
    {
        private const string url = "https://myanimelist.net/anime/season";
        private readonly HttpClient _client = new();

        public async Task<List<Anime>> GetSeasonals()
        {
            var nodes = await GetNodes();
            var seasonals = ParseNodeContents(nodes);

            var min = seasonals.Min(x => x.Members);
            var max = seasonals.Max(x => x.Members);

            seasonals = seasonals.OrderBy(x => x.Members).ToList();
            seasonals.Reverse();

            foreach (var seasonal in seasonals)
            {
                seasonal.Normalised = GetNormalisedScore(seasonal, min, max);
            }

            Console.WriteLine("Seasonal Returned");
            return seasonals;
        }

        private async Task<IEnumerable<HtmlNode>> GetNodes()
        {
            var html = await _client.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.QuerySelectorAll("#content > div.js-categories-seasonal > div:nth-child(1)");
            return nodes;
        }

        private static List<Anime> ParseNodeContents(IEnumerable<HtmlNode> nodes)
        {
            var seasonals = new List<Anime>();

            foreach (var htmlNode in nodes)
            {
                foreach (var seasonalChildeNode in htmlNode.ChildNodes)
                {
                    TrimAndAddSeasonal(seasonalChildeNode, seasonals);
                }
            }

            return seasonals;
        }

        private static void TrimAndAddSeasonal(HtmlNode seasonalChildeNode, List<Anime> seasonals)
        {
            if (seasonalChildeNode.ChildNodes.Count < 4)
            {
                return;
            }

            var node = seasonalChildeNode.ChildNodes[2];

            var title = node
                .InnerText
                .Trim();

            var link = node
                .ChildNodes[0]
                .Attributes
                .First()
                .Value
                .Trim();

            var members = seasonalChildeNode
                .ChildNodes[6]
                .ChildNodes[3]
                .ChildNodes[1]
                .InnerText
                .Replace("\n", string.Empty)
                .Replace(",", string.Empty)
                .Trim();

            seasonals.Add(new Anime
            {
                Name = title,
                URL = link,
                Members = int.Parse(members)
            });
        }

        private static float GetNormalisedScore(Anime seasonal, int min, int max)
        {
            return ((float) seasonal.Members - min) / (max - min);
        }

        // private static void WriteToConsole(IEnumerable<Anime> seasonals, int min, int max)
        // {
        //     foreach (var seasonal in seasonals)
        //     {
        //         seasonal.Normalised = ((float) seasonal.Members - min) / (max - min);
        //         Console.WriteLine($"Name: {seasonal.Name} ({seasonal.URL})");
        //         Console.WriteLine($"Members: {seasonal.Members}");
        //         Console.WriteLine($"Min-Max Normalised: {seasonal.Normalised:0.#########}");
        //         Console.WriteLine();
        //     }
        // }
        //
        // private static async Task WriteToFile(string fileName, List<Anime> seasonals)
        // {
        //     var json = JsonSerializer.Serialize(seasonals, new JsonSerializerOptions {WriteIndented = true});
        //     await File.WriteAllTextAsync(fileName, json);
        // }
    }
}