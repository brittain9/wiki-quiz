using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core;

public static class RandomContentSections
{
    public static string GetRandomContentSections(string extract, int requestedContentLength)
    {
        const int MIN_CONTENT_LENGTH = 500;
        const int MAX_CONTENT_LENGTH = 50000;
        const int LENGTH_FOR_LARGER_SECTIONS = 3000;
        const int LARGER_SECTION_SIZE = 1000;
        const int SMALLER_SECTION_SIZE = 500;
        const int MAX_ATTEMPTS = 1000;

        if (string.IsNullOrEmpty(extract))
            return string.Empty;

        int contentLength = Math.Clamp(requestedContentLength, MIN_CONTENT_LENGTH, MAX_CONTENT_LENGTH);

        if (contentLength >= extract.Length)
            return extract;

        int sectionSize = contentLength >= LENGTH_FOR_LARGER_SECTIONS ? LARGER_SECTION_SIZE : SMALLER_SECTION_SIZE;

        var random = new Random();
        var result = new StringBuilder(contentLength);
        var usedRanges = new List<(int Start, int End)>();

        int attempts = 0;
        while (result.Length < contentLength && attempts < MAX_ATTEMPTS)
        {
            int remainingLength = extract.Length - usedRanges.Sum(r => r.End - r.Start);
            if (remainingLength <= 0) break;

            int maxStartPosition = Math.Max(0, extract.Length - sectionSize);
            int startPosition = random.Next(maxStartPosition + 1);
            int endPosition = Math.Min(startPosition + sectionSize, extract.Length);

            var overlapRange = FindOverlappingRange(usedRanges, startPosition, endPosition);
            if (overlapRange == null)
            {
                int actualSectionSize = Math.Min(endPosition - startPosition, contentLength - result.Length);
                result.Append(extract.AsSpan(startPosition, actualSectionSize));
                usedRanges.Add((startPosition, startPosition + actualSectionSize));
                usedRanges.Sort((a, b) => a.Start.CompareTo(b.Start));
                attempts = 0;
            }
            else
            {
                attempts++;
                // Try to adjust the start position to avoid overlap
                startPosition = overlapRange.Value.End;
                if (startPosition + sectionSize <= extract.Length)
                {
                    endPosition = startPosition + sectionSize;
                    continue;
                }
            }
        }

        // If we couldn't fill the requested length, fill with remaining content
        if (result.Length < contentLength)
        {
            FillRemainingContent(extract, contentLength, result, usedRanges);
        }

        return result.ToString();
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
