using Convertor.Controllers;
using System;
using System.Xml;
using System.Xml.Linq;
using TechTalk.SpecFlow;

namespace Testing.StepDefinitions
{
    [Binding]
    public class ConvertorStepDefinitions
    {
        public ConvertorController _convertor = null;

        public class Book
        {
            public string Title { get; set; }
            public int Number { get; set; }
            public bool IsAvailable { get; set; }
        }

        [Given(@"I am using the Convertor api")]
        public void GivenIAmUsingTheConvertorApi()
        {
            _convertor = new ConvertorController();
        }

        [When(@"The URI is a link to a csv file")]
        public void WhenTheURIIsALinkToACsvFile()
        {
            string uri = "http://beezupcdn.blob.core.windows.net/recruitment/bigfile.csv";
            _convertor.UriIsCsvFile(uri);
            _convertor.Should().NotBeNull();
        }

        [Then(@"I receive a json or a xml as a string")]
        public void ThenIReceiveAJsonOrAXmlAsAString()
        {
            var books = new List<dynamic>(){
                new Book {
                    Title = "Book1",
                    Number = 25, 
                    IsAvailable = true
                },
                new Book {
                    Title = "Book2",
                    Number = 0,
                    IsAvailable = false
                }
            };

            string json = _convertor.convertEntriesToJson(books);
            json.Should().NotBeNullOrEmpty();

            try
            {
                var js = new Newtonsoft.Json.JsonSerializer();
                js.Deserialize(new Newtonsoft.Json.JsonTextReader(new StringReader(json)));
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                throw;
            }

            XmlDocument xml = _convertor.convertEntriesToXml(books);
            string xmlAsString = xml.OuterXml;
            xmlAsString.Should().NotBeNullOrEmpty();

            try
            {
                var doc = XDocument.Parse(xmlAsString);
            }
            catch
            {
                throw;
            }
        }
    }
}
