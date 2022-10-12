using Microsoft.AspNetCore.Mvc;
using CSVConverter.Enums;
using System.Net;
using System.Text;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using System.Xml;

namespace Convertor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConvertorController : ControllerBase
    {     
        private readonly ILogger<ConvertorController>? _logger;

        public ConvertorController()
        {
            _logger = null;
        }
        public ConvertorController(ILogger<ConvertorController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Produces("application/json", "text/xml")]
        public async Task<IActionResult> Post(string csvUri, FileTypes toFileType)
        {

            string convertedCSV = "";

            // Test if the uri is a link to a .csv file
            if(!UriIsCsvFile(csvUri))
            {
                return BadRequest("the link is not a csv file");
            }

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
                    BadDataFound = x =>
                    {
                        throw new CsvException("The CSV is badly formatted");
                    },
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
                        return Ok(convertEntriesToJson(recordList));
                    }

                    // Xml convertion
                    else
                    {                        
                        return Ok(convertEntriesToXml(recordList).OuterXml);
                    }
                }
            }

            // Error Code 400 = Bad CSV
            catch (CsvException ex)
            {
                return StatusCode(400, ex.Message);
            }

            // Error Code 500
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        [NonAction]
        public bool UriIsCsvFile (string uri)
        {
            if (uri.EndsWith(".csv"))
                return true;
            return false;
        }

        [NonAction]
        public string convertEntriesToJson(List<dynamic> entries)
        {
            return JsonConvert.SerializeObject(entries);
        }

        [NonAction]
        public XmlDocument convertEntriesToXml(List<dynamic> entries)
        {

            var entriesWrapper = new
            {
                row = entries
            };

            // En recherche d'une meilleure methode, actuellement pas assez optimise
            string convertedEntries = JsonConvert.SerializeObject(entriesWrapper);
            XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(convertedEntries, "root");
            return doc;
        }

    }

}

// Custom Exception
public class CsvException: Exception
{
    public CsvException()
    {
    }
    public CsvException(string message)
        : base(message)
    {
    }

    public CsvException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}