using System.Threading.Tasks;

namespace Seasonal.Core
{
    public class Program
    {
        private static readonly WebParser _webParser = new();
        private static readonly SheetsLoader _sheetsLoader = new();
        
        public static async Task Main()
        {
            var seasonals = await _webParser.GetSeasonals();
            await _sheetsLoader.LoadIntoSheets(seasonals);
        }
    }
}