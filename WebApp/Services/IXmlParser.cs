using System.Collections.Generic;
using System.Xml.Linq;
using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Defines a contract that represents the parse method for xml feed document.
    /// </summary>
    public interface IXmlParser
    {
        IList<Article> Parse(XDocument document);
    }
}
