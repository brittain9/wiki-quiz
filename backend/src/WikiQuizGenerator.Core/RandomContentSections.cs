using System;
using System.Collections.Generic;
using System.Text;

namespace WikiQuizGenerator.Core;

public static class RandomContentSections
{
    private static readonly Random Random = new Random();
    
    public static string GetRandomContentSections(string extract, int requestedContentLength)
    {
        const int MIN_CONTENT_LENGTH = 500;
        const int MAX_CONTENT_LENGTH = 50000;
        const int LENGTH_FOR_LARGER_SECTIONS = 3000;
        const int LARGER_SECTION_SIZE = 1000;
        const int SMALLER_SECTION_SIZE = 500;

        // Early returns for edge cases
        if (string.IsNullOrEmpty(extract))
            return string.Empty;
            
        int contentLength = Math.Clamp(requestedContentLength, MIN_CONTENT_LENGTH, MAX_CONTENT_LENGTH);
        if (contentLength >= extract.Length)
            return extract;

        int sectionSize = contentLength >= LENGTH_FOR_LARGER_SECTIONS ? LARGER_SECTION_SIZE : SMALLER_SECTION_SIZE;
        var result = new StringBuilder(contentLength);
        var usedPositions = new HashSet<int>();  // Faster lookup than List of ranges

        int targetLength = Math.Min(contentLength, extract.Length);
        while (result.Length < targetLength)
        {
            int remainingSpace = targetLength - result.Length;
            int currentSectionSize = Math.Min(sectionSize, remainingSpace);
            int maxStart = extract.Length - currentSectionSize;
            int startPosition;

            // Find a non-overlapping position efficiently
            int attempts = 0;
            do
            {
                startPosition = Random.Next(maxStart + 1);
                attempts++;
                if (attempts > 100)  // Prevent infinite loop
                {
                    return FillToLength(extract, targetLength, result);
                }
            } while (!IsPositionAvailable(usedPositions, startPosition, currentSectionSize));

            // Add the section
            result.Append(extract, startPosition, currentSectionSize);
            MarkPositionUsed(usedPositions, startPosition, currentSectionSize);
        }

        return result.ToString();
    }

        private static bool IsPositionAvailable(HashSet<int> usedPositions, int start, int length)
    {
        int end = start + length;
        for (int i = start; i < end; i++)
        {
            if (usedPositions.Contains(i))
                return false;
        }
        return true;
    }

    private static void MarkPositionUsed(HashSet<int> usedPositions, int start, int length)
    {
        int end = start + length;
        for (int i = start; i < end; i++)
        {
            usedPositions.Add(i);
        }
    }

    private static (int Start, int End)? FindOverlappingRange(List<(int Start, int End)> ranges, int start, int end)
    {
        foreach (var range in ranges)
        {
            if (start < range.End && end > range.Start)
                return range;
        }
        return null;
    }

    private static void FillRemainingContent(string extract, int contentLength, StringBuilder result, List<(int Start, int End)> usedRanges)
    {
        var unusedRanges = GetUnusedRanges(extract.Length, usedRanges);
        foreach (var range in unusedRanges)
        {
            int remainingNeeded = contentLength - result.Length;
            if (remainingNeeded <= 0) break;

            int rangeSize = range.End - range.Start;
            int sizeToAdd = Math.Min(rangeSize, remainingNeeded);
            result.Append(extract.AsSpan(range.Start, sizeToAdd));
        }
    }

        private static string FillToLength(string extract, int targetLength, StringBuilder result)
    {
        // … existing code …
        int remaining = targetLength - result.Length;
        if (remaining > 0)
        {
            int start = 0;
            while (result.Length < targetLength && start < extract.Length)
            {
                int length = Math.Min(targetLength - result.Length, extract.Length - start);
                result.Append(extract, start, length);
                start += length;
            }
        }
        return result.ToString();
    }
    
    private static List<(int Start, int End)> GetUnusedRanges(int totalLength, List<(int Start, int End)> usedRanges)
    {
        var unusedRanges = new List<(int Start, int End)>();
        int currentPosition = 0;

        foreach (var range in usedRanges.OrderBy(r => r.Start))
        {
            if (currentPosition < range.Start)
            {
                unusedRanges.Add((currentPosition, range.Start));
            }
            currentPosition = range.End;
        }

        if (currentPosition < totalLength)
        {
            unusedRanges.Add((currentPosition, totalLength));
        }

        return unusedRanges;
    }
}
