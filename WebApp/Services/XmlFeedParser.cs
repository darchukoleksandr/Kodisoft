using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using WebApp.Models;

namespace WebApp.Services
{
    public class XmlFeedParser : IXmlParser
    {
        private readonly ILogger _iLogger;
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="iLogger"></param>
        public XmlFeedParser(ILogger<XmlFeedParser> iLogger)
        {
            _iLogger = iLogger;
        }
        /// <summary>
        /// Parses the given xml rss document.
        /// </summary>
        /// <returns></returns>
        public IList<Article> Parse(XDocument document)
        {
            switch (document.Root?.Name.LocalName)
            {
                case "rss":
                    return ParseRss(document);
                case "rdf":
                    return ParseRdf(document);
                case "feed":
                    return ParseAtom(document);
                default:
                    throw new NotSupportedException("Feed type is not supported");
            }
        }
        /// <summary>
        /// Parses an Atom feed and returns a <see cref="IList{Article}"/>>.
        /// </summary>
        private IList<Article> ParseAtom(XDocument document)
        {
            _iLogger.LogDebug("Starting parsing the xml document as ATOM feed.");

            var entries = from item in document.Root.Elements().Where(i => i.Name.LocalName == "entry")
                select new Article
                {
                    Content = item.Elements().First(i => i.Name.LocalName == "content").Value,
                    Link = item.Elements().First(i => i.Name.LocalName == "link").Attribute("href").Value,
                    PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "published").Value),
                    Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                };

            _iLogger.LogDebug($"Returning list with {entries.Count()} articles.");

            return entries.ToList();
        }
        /// <summary>
        /// Parses an RSS feed and returns a <see cref="IList{Article}"/>>.
        /// </summary>
        private IList<Article> ParseRss(XDocument document)
        {
            _iLogger.LogDebug("Starting parsing the xml document as RSS feed.");

            var entries = from item in document.Root?.Descendants().FirstOrDefault(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item")
                select new Article
                {
                    Content = item.Elements().First(i => i.Name.LocalName == "description").Value,
                    Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                    PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                    Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                };

            _iLogger.LogDebug($"Returning list with {entries.Count()} articles.");

            return entries.ToList();
        }
        /// <summary>
        /// Parses an RDF feed and returns a <see cref="IList{Article}"/>>.
        /// </summary>
        private IList<Article> ParseRdf(XDocument document)
        {
            _iLogger.LogDebug("Starting parsing the xml document as RDF feed.");
            
            var entries = from item in document.Root.Descendants().Where(i => i.Name.LocalName == "item")
                select new Article
                {
                    Content = item.Elements().First(i => i.Name.LocalName == "description").Value,
                    Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                    PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "date").Value),
                    Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                };

            _iLogger.LogDebug($"Returning list with {entries.Count()} articles.");

            return entries.ToList();
        }

        public static async Task<bool> IsFeedUrl(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    using (var response = await client.GetAsync(new Uri(url)))
                    {
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            try
                            {
                                var document = XDocument.Load(contentStream);
                                if (new[] { "rss", "feed", "rdf" }.Contains(document.Root?.Name.LocalName))
                                    return true;
                            }
                            catch (XmlException)
                            {
                                return false;
                            }
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Downloading went wrong!");
                }
            }
            return false;
        }

        private DateTime ParseDate(string date)
        {
            DateTime result;
            if (DateTime.TryParse(date, out result))
                return result;
            else
                return DateTime.MinValue;
        }
    }
}
