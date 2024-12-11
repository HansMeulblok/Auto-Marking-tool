using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

public class GoogleSheetsHelper
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string ApplicationName = "AutoMarkingApp";
    private readonly SheetsService _service;
    private const int MaxRetries = 5;

    public GoogleSheetsHelper()
    {
        GoogleCredential credential;

        using (var stream = new FileStream(@"Resources/credentials.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    public (int Row, int Column)? FindCell(string spreadsheetId, string range, string searchText)
    {
        var values = ReadSheet(spreadsheetId, range);
        if (values == null) return null;

        // Normalize and clean the OCR-detected text otherwise it cant find IDs (future improvement)
        string cleanedSearchText = NormalizeText(searchText);

        for (int rowIndex = 0; rowIndex < values.Count; rowIndex++)
        {
            var row = values[rowIndex];
            if (row.Count > 0)
            {
                string cellValue = row[0]?.ToString() ?? string.Empty;
                string cleanedCellValue = NormalizeText(cellValue);

                // Check if the cleaned cell value is contained in the cleaned OCR text
                if (cleanedSearchText.Contains(cleanedCellValue, StringComparison.OrdinalIgnoreCase))
                {
                    return (rowIndex, 0); // Assuming single-column Text ID
                }
            }
        }
        return null;
    }

// Helper method to normalize and clean text because it wrongly captures buttons and spaces etc 
private string NormalizeText(string input)
{
    if (string.IsNullOrWhiteSpace(input)) return string.Empty;

    // Remove non-alphanumeric characters
    char[] validChars = input.ToCharArray();
    validChars = Array.FindAll(validChars, char.IsLetterOrDigit);

    return new string(validChars);
}


    public IList<IList<object>> ReadSheet(string spreadsheetId, string range)
    {
        return ExecuteWithRetry(() =>
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            return response.Values;
        });
    }

    public void WriteToSheet(string spreadsheetId, string range, IList<IList<object>> values)
    {
        ExecuteWithRetry(() =>
        {
            var valueRange = new ValueRange() { Values = values };

            var updateRequest = _service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            updateRequest.Execute();
            return true;
        });
    }

    public void UpdateCellBackground(string spreadsheetId, int sheetId, int rowIndex, int columnIndex, string color)
    {
        ExecuteWithRetry(() =>
        {
            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    new Request
                    {
                        RepeatCell = new RepeatCellRequest
                        {
                            Cell = new CellData
                            {
                                UserEnteredFormat = new CellFormat
                                {
                                    BackgroundColor = ParseColor(color)
                                }
                            },
                            Range = new GridRange
                            {
                                SheetId = sheetId,
                                StartRowIndex = rowIndex,
                                EndRowIndex = rowIndex + 1,
                                StartColumnIndex = columnIndex,
                                EndColumnIndex = columnIndex + 1,
                            },
                            Fields = "userEnteredFormat.backgroundColor"
                        }
                    }
                }
            };

            _service.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetId).Execute();
            return true;
        });
    }

    private Google.Apis.Sheets.v4.Data.Color ParseColor(string colorHex)
    {
        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(colorHex);
        return new Google.Apis.Sheets.v4.Data.Color
        {
            Red = color.R / 255f,
            Green = color.G / 255f,
            Blue = color.B / 255f
        };
    }

    private T ExecuteWithRetry<T>(Func<T> action)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                return action();
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.TooManyRequests || ex.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
            {
                if (attempt == MaxRetries - 1) throw;
                int delay = (int)Math.Pow(2, attempt) * 1000;
                Thread.Sleep(delay);
            }
        }
        throw new InvalidOperationException("Retry logic failed unexpectedly.");
    }
}
