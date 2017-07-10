using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// <see cref="ISourceSaver"/> implementation for xml feeds (ATOM, RSS, RDF).
    /// </summary>
    public class XmlSourceSaver : ISourceSaver
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _iLogger;
        private readonly IXmlParser _parser;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="iLogger"></param>
        /// <param name="parser"></param>
        public XmlSourceSaver(ApplicationDbContext dbContext, ILogger<XmlSourceSaver> iLogger, IXmlParser parser)
        {
            _dbContext = dbContext;
            _iLogger = iLogger;
            _parser = parser;
        }

        public async Task Save(Source source)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    using (var response = await client.GetAsync(new Uri(source.UrlRss)))
                    {
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            var result = _parser.Parse(XDocument.Load(contentStream));
                            foreach (var rssArticle in result)
                            {
                                rssArticle.SourceId = source.Id;
                                if (!_dbContext.Articles.Any(article => article.SourceId == source.Id &&
                                                                        article.Link == rssArticle.Link))
                                {
                                    await _dbContext.Articles.AddAsync(rssArticle);
                                }
                            }

                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch(HttpRequestException)
                {
                    _iLogger.LogWarning("Downloading went wrong!");
                }
            }
        }
    }
}
