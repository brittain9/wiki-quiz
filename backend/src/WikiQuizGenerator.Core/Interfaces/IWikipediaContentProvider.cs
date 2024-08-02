using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IWikipediaContentProvider
{
    Task<WikipediaPage> GetWikipediaPage(string topic, Languages language);
    Task<string> GetWikipediaExactTitle(string query);
}
