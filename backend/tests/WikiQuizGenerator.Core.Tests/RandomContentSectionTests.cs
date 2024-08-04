using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using WikiQuizGenerator.Core;

namespace WikiRandomContentSections.Core.Tests;

public class GetRandomContentSectionsTests
{
    [Fact]
    public void EmptyInputString_ReturnsEmptyString()
    {
        string result = RandomContentSections.GetRandomContentSections("", 1000);
        Assert.Equal("", result);
    }

    [Fact]
    public void NullInputString_ReturnsEmptyString()
    {
        string result = RandomContentSections.GetRandomContentSections(null, 1000);
        Assert.Equal("", result);
    }

    [Fact]
    public void RequestedLengthBelowMinimum_ReturnsMinimumLength()
    {
        string input = new string('a', 1000);
        string result = RandomContentSections.GetRandomContentSections(input, 100);
        Assert.Equal(500, result.Length);
    }

    [Fact]
    public void RequestedLengthAboveMaximum_ReturnsMaximumLength()
    {
        string input = new string('a', 100000);
        string result = RandomContentSections.GetRandomContentSections(input, 60000);
        Assert.Equal(50000, result.Length);
    }

    [Fact]
    public void InputShorterThanRequestedLength_ReturnsEntireInput()
    {
        string input = "Short string";
        string result = RandomContentSections.GetRandomContentSections(input, 1000);
        Assert.Equal(input, result);
    }

    [Fact]
    public void InputEqualToRequestedLength_ReturnsEntireInput()
    {
        string input = new string('a', 1000);
        string result = RandomContentSections.GetRandomContentSections(input, 1000);
        Assert.Equal(input, result);
    }

    [Fact]
    public void RequestedLengthJustBelowLargerSectionsThreshold_UsesSmallerSections()
    {
        string input = new string('a', 5000);
        string result = RandomContentSections.GetRandomContentSections(input, 2999);
        Assert.Equal(2999, result.Length);
    }

    [Fact]
    public void RequestedLengthJustAboveLargerSectionsThreshold_UsesLargerSections()
    {
        string input = new string('a', 5000);
        string result = RandomContentSections.GetRandomContentSections(input, 3001);
        Assert.Equal(3001, result.Length);
    }

    [Fact]
    public void RepeatedContent_ReturnsRandomSections()
    {
        string pattern = "abcdefghij";
        string input = string.Concat(Enumerable.Repeat(pattern, 1000)); // 10000 characters

        string result1 = RandomContentSections.GetRandomContentSections(input, 2000);
        string result2 = RandomContentSections.GetRandomContentSections(input, 2000);

        Assert.Equal(2000, result1.Length);
        Assert.Equal(2000, result2.Length);
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void VeryLongInputString_ReturnsCorrectLength()
    {
        string input = new string('a', 100000);
        string result = RandomContentSections.GetRandomContentSections(input, 40000);
        Assert.Equal(40000, result.Length);
    }

    [Fact]
    public void InputWithSpecialCharacters_ReturnsCorrectResult()
    {
        string input = "áéíóú" + new string('a', 1000) + "ñçß";
        string result = RandomContentSections.GetRandomContentSections(input, 600);
        Assert.Equal(600, result.Length);
        Assert.Contains(result, c => c != 'a');
    }

    [Fact]
    public void ConsistentLengthAcrossMultipleCalls()
    {
        string input = new string('a', 10000);
        for (int i = 0; i < 100; i++)
        {
            string result = RandomContentSections.GetRandomContentSections(input, 2000);
            Assert.Equal(2000, result.Length);
        }
    }

    [Fact]
    public void DoesNotExceedRequestedLength()
    {
        string input = new string('a', 10000);
        for (int i = 0; i < 100; i++)
        {
            int requestedLength = new Random().Next(500, 5000);
            string result = RandomContentSections.GetRandomContentSections(input, requestedLength);
            Assert.True(result.Length <= requestedLength);
            Assert.True(result.Length >= Math.Min(500, input.Length));
        }
    }
}