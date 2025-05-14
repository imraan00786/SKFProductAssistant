using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SKFProductAssistant.Services
{
    public class DatasheetService
    {
        private readonly string _datasheetFolder;

        public DatasheetService(string datasheetFolder)
        {
            _datasheetFolder = datasheetFolder;
        }

        public async Task<List<Dictionary<string, object>>> GetAllDatasheetsAsync()
        {
            var allDatasheets = new List<Dictionary<string, object>>();

            foreach (var file in Directory.GetFiles(_datasheetFolder, "*.json"))
            {
                var jsonData = await File.ReadAllTextAsync(file);
                var datasheet = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
                allDatasheets.Add(datasheet);
            }

            return allDatasheets;
        }

        public async Task<string> GetProductAttributeAsync(string product, string attribute)
        {
            var allDatasheets = await GetAllDatasheetsAsync();

            foreach (var record in allDatasheets)
            {
                if (record.ContainsKey("designation") && record["designation"].ToString() == product)
                {
                    // Search in dimensions
                    if (record.ContainsKey("dimensions"))
                    {
                        var dimensions = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(record["dimensions"].ToString());
                        foreach (var dimension in dimensions)
                        {
                            if (dimension["name"].ToString().Equals(attribute, StringComparison.OrdinalIgnoreCase))
                            {
                                return FormatAttributeValue(dimension);
                            }
                        }
                    }

                    // Search in properties, performance, logistics, and specifications
                    var sections = new[] { "properties", "performance", "logistics", "specifications" };
                    foreach (var section in sections)
                    {
                        if (record.ContainsKey(section))
                        {
                            var items = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(record[section].ToString());
                            foreach (var item in items)
                            {
                                if (item["name"].ToString().Equals(attribute, StringComparison.OrdinalIgnoreCase))
                                {
                                    return FormatAttributeValue(item);
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private string FormatAttributeValue(Dictionary<string, object> attributeData)
        {
            var value = attributeData["value"].ToString();
            var unit = attributeData.ContainsKey("unit") ? attributeData["unit"].ToString() : string.Empty;

            return string.IsNullOrEmpty(unit) ? value : $"{value} {unit}";
        }
    }
}
