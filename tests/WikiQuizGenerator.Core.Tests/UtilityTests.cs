using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WikiQuizGenerator.Core.Tests;

public class UtilityTests
{
    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("h", "H")]
    [InlineData("heLlo    WoRLD", "Hello    World")]
    [InlineData("hElLo wOrLD!", "Hello World!")]
    [InlineData("hello world123", "Hello World123")]

    public void ToTitleCase_VariousInputs_CorrectlyCapitalizes(string input, string expected)
    {
        Assert.Equal(expected, input.ToTitleCase());
    }
}

