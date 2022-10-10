using Microsoft.AspNetCore.Mvc;
using CSVConverter.Enums;
using System.Net;
using System.Text;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using System.Xml;

namespace CSVConverter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CSVConverterController : ControllerBase
    {     
        private readonly ILogger<CSVConverterController> _logger;

        public CSVConverterController(ILogger<CSVConverterController> logger)
        {
            _logger = logger;
        }


        // http://beezupcdn.blob.core.windows.net/recruitment/bigfile.csv

        [HttpPost(Name = "Convertor")]
        [Produces("application/json", "text/xml")]
        public async Task<IActionResult> Post(string csvUri, FileTypes toFileType)
        {

            string convertedCSV = "";

            // Creates an HttpWebRequest with the specified URL.
            HttpClient myHttpWebClient = new HttpClient();

            // Sends the HttpWebRequest and waits for the response.			
            var myHttpWebResponse = await myHttpWebClient.GetAsync(csvUri);

            // Gets the stream associated with the response.
            Stream receiveStream = await myHttpWebResponse.Content.ReadAsStreamAsync();

            Encoding encode = Encoding.GetEncoding("utf-8");

            // Pipes the stream to a higher level stream reader with the required encoding format.
            StreamReader readStream = new StreamReader(receiveStream, encode);

            try
            {
                // read header
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    IgnoreBlankLines = true,
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    ShouldSkipRecord = record => string.IsNullOrEmpty(record.ToString()),
                    Delimiter = ";",
                    Quote = '"',
                };

                using (var csv = new CsvReader(readStream, config))
                {
                    List<dynamic> recordList = new List<dynamic>();
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())                    
                    {
                        recordList.Add(csv.GetRecord<dynamic>());
                    }

                    // Json convertion
                    if (toFileType.Equals(FileTypes.Json))
                    {
                        convertedCSV = JsonConvert.SerializeObject(recordList);
                        return Ok(convertedCSV);
                    }

                    // Xml convertion
                    else
                    {
                        var recordWrapper = new
                        {
                            row = recordList
                        };

                        // En recherche d'une meilleure methode, actuellement pas assez optimise
                        convertedCSV = JsonConvert.SerializeObject(recordWrapper);
                        XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(convertedCSV, "root");
                        return Ok(doc.OuterXml);
                    }
                }
            }

            // Error Code
            catch (Exception ex)
            {
                return StatusCode(500, "internal server error");
            }

            // Close reader and reset memory
            finally
            {
                // Releases the resources of the response.
                myHttpWebResponse.Dispose();

                // Releases the resources of the Stream.
                readStream.Close();
            }
        }
    }
}