using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Defines a contract that represents the save method for the articles in specified <see cref="Source"/>.
    /// </summary>
    public interface ISourceSaver
    {
        Task Save(Source source);
    }
}
