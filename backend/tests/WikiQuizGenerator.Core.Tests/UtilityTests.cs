using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WikiQuizGenerator.Core.Tests;

public class UtilityTests
{
    [Fact]
    public void RemoveFormatting_RemovesHTMLTags()
    {
        string input = "<this will be removed> this will stay </this will be removed>";
        string expected = "this will stay";

        string result = Utility.RemoveFormatting(input);

        Assert.Equal(expected, result);
    }
}

