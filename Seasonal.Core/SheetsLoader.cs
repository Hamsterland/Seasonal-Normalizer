using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace Seasonal.Core
{
    public class SheetsLoader
    {
        private readonly SheetsService _sheetsService;

        public SheetsLoader()
        {
            using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);

            var credential = GoogleCredential
                .FromStream(stream)
                .CreateScoped();

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Hiromi"
            });
        }
        
        private const string spreadsheetId = "136fP5wJMrb65JQZIiwdIGCtaTgmh5w_FR_fKfdBNuXU";
        
        public async Task LoadIntoSheets(List<Anime> animes)
        {
            var requests = new List<Request>();
            var tiers = new List<List<Anime>>();
            
            SortTiers(animes, tiers);
            Console.WriteLine("All Entries Tiered");
            
            for (var i = 0; i < tiers.Count; i++)
            {
                var tier = tiers[i];
                
                for (var j = 0; j < tier.Count; j++)
                {
                    AddCellRequest(requests, tier, j, i);
                }
            }
            
            await ExecuteBatchUpdate(requests);
            Console.WriteLine("All Entries Loaded into Sheet");
        }

        private async Task ExecuteBatchUpdate(List<Request> requests)
        {
            var requestBody = new BatchUpdateSpreadsheetRequest
            {
                Requests = requests,
            };

            var request = _sheetsService
                .Spreadsheets
                .BatchUpdate(requestBody, spreadsheetId);

            await request.ExecuteAsync();
        }

        private static void AddCellRequest(List<Request> requests, List<Anime> tier, int j, int i)
        {
            requests.Add(new Request
            {
                UpdateCells = new UpdateCellsRequest
                {
                    Rows = new List<RowData>
                    {
                        new()
                        {
                            Values = new List<CellData>
                            {
                                new()
                                {
                                    UserEnteredValue = new ExtendedValue
                                    {
                                        FormulaValue = $"=HYPERLINK(\"{tier[j].URL}\", \"{tier[j].Name}\")"
                                    }
                                }
                            }
                        }
                    },

                    Fields = "*",
                    Start = new GridCoordinate
                    {
                        ColumnIndex = i,
                        RowIndex = j + 1
                    }
                }
            });
        }

        private static void SortTiers(List<Anime> animes, List<List<Anime>> tiers)
        {
            var t1 = animes.Where(x => x.Normalised is > 0.6f).ToList();
            var t2 = animes.Where(x => x.Normalised is < 0.6f and > 0.2f).ToList();
            var t3 = animes.Where(x => x.Normalised is < 0.2f).ToList();

            tiers.Add(t1);
            tiers.Add(t2);
            tiers.Add(t3);
        }
    }
}