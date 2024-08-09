using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core;

// These are the supported languages on wikipedia
public enum Languages
{
    English,
    German,
    Spanish,
    Chinese,
    // Farci, // This seems too difficult to support as it is read from right to left.
    Japanese,
    Russian,
    French,
    Italian,
    Portuguese
}
