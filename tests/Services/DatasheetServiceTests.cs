using System.Threading.Tasks;
using Xunit;
using SKFProductAssistant.Services;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SKFProductAssistant.Tests.Services
{
    public class DatasheetServiceTests
    {
        private readonly string _testDatasheetFolder;
        private readonly DatasheetService _datasheetService;

        public DatasheetServiceTests()
        {
            // Setup test datasheet folder
            _testDatasheetFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestDatasheets");
            Directory.CreateDirectory(_testDatasheetFolder);

            // Create sample CSV file
            File.WriteAllText(Path.Combine(_testDatasheetFolder, "test.csv"), "Product,Width,Height\n6205,15mm,20mm\n6205 N,16mm,21mm");

            // Create sample JSON file
            var jsonData = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "Product", "6206" }, { "Width", "17mm" }, { "Height", "22mm" } },
                new Dictionary<string, string> { { "Product", "6206 N" }, { "Width", "18mm" }, { "Height", "23mm" } }
            };
            File.WriteAllText(Path.Combine(_testDatasheetFolder, "test.json"), JsonConvert.SerializeObject(jsonData));

            // Initialize DatasheetService
            _datasheetService = new DatasheetService(_testDatasheetFolder);
        }

        [Fact]
        public async Task GetProductAttributeAsync_ReturnsCorrectValue_FromCsv()
        {
            var result = await _datasheetService.GetProductAttributeAsync("6205", "Width");
            Assert.Equal("15mm", result);
        }

        [Fact]
        public async Task GetProductAttributeAsync_ReturnsCorrectValue_FromJson()
        {
            var result = await _datasheetService.GetProductAttributeAsync("6206", "Height");
            Assert.Equal("22mm", result);
        }

        [Fact]
        public async Task GetProductAttributeAsync_ReturnsNull_WhenProductNotFound()
        {
            var result = await _datasheetService.GetProductAttributeAsync("9999", "Width");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductAttributeAsync_ReturnsNull_WhenAttributeNotFound()
        {
            var result = await _datasheetService.GetProductAttributeAsync("6205", "Diameter");
            Assert.Null(result);
        }

        ~DatasheetServiceTests()
        {
            // Cleanup test datasheet folder
            if (Directory.Exists(_testDatasheetFolder))
            {
                Directory.Delete(_testDatasheetFolder, true);
            }
        }
    }
}
