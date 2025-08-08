using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickJump2022.Tools;

/// <summary>
/// Fuzzy search implementation based on VS Code's fuzzy scorer algorithm
/// </summary>
public static class FuzzySearch
{
    /// <summary>
    /// Represents a fuzzy search score with match positions
    /// </summary>
    public struct FuzzyScore
    {
        public int Score;
        public int[] MatchPositions;

        public FuzzyScore(int score, int[] matchPositions)
        {
            Score = score;
            MatchPositions = matchPositions ?? Array.Empty<int>();
        }

        public static readonly FuzzyScore NoMatch = new FuzzyScore(0, Array.Empty<int>());
    }

    /// <summary>
    /// Performs fuzzy search on a target string using the query
    /// </summary>
    /// <param name="target">The string to search in</param>
    /// <param name="query">The search query</param>
    /// <param name="allowNonContiguousMatches">Whether to allow non-contiguous matches</param>
    /// <returns>A fuzzy score with match positions</returns>
    public static FuzzyScore ScoreFuzzy(string target, string query, bool allowNonContiguousMatches = true)
    {
        if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(query))
        {
            return FuzzyScore.NoMatch;
        }

        var targetLength = target.Length;
        var queryLength = query.Length;

        if (targetLength < queryLength)
        {
            return FuzzyScore.NoMatch; // Impossible for query to be contained in target
        }

        var targetLower = target.ToLowerInvariant();
        var queryLower = query.ToLowerInvariant();

        return DoScoreFuzzy(query, queryLower, queryLength, target, targetLower, targetLength, allowNonContiguousMatches);
    }

    /// <summary>
    /// Checks if a string matches a query using fuzzy search
    /// </summary>
    /// <param name="target">The string to check</param>
    /// <param name="query">The search query</param>
    /// <param name="allowNonContiguousMatches">Whether to allow non-contiguous matches</param>
    /// <returns>True if the target matches the query</returns>
    public static bool IsMatch(string target, string query, bool allowNonContiguousMatches = true)
    {
        var score = ScoreFuzzy(target, query, allowNonContiguousMatches);
        return score.Score > 0;
    }

    /// <summary>
    /// Gets the fuzzy score for a string against a query
    /// </summary>
    /// <param name="target">The string to score</param>
    /// <param name="query">The search query</param>
    /// <param name="allowNonContiguousMatches">Whether to allow non-contiguous matches</param>
    /// <returns>The fuzzy score (higher is better)</returns>
    public static int GetScore(string target, string query, bool allowNonContiguousMatches = true)
    {
        var score = ScoreFuzzy(target, query, allowNonContiguousMatches);
        return score.Score;
    }

    private static FuzzyScore DoScoreFuzzy(string query, string queryLower, int queryLength, 
        string target, string targetLower, int targetLength, bool allowNonContiguousMatches)
    {
        var scores = new int[queryLength * targetLength];
        var matches = new int[queryLength * targetLength];

        // Build scorer matrix
        for (int queryIndex = 0; queryIndex < queryLength; queryIndex++)
        {
            var queryIndexOffset = queryIndex * targetLength;
            var queryIndexPreviousOffset = queryIndexOffset - targetLength;
            var queryIndexGtNull = queryIndex > 0;
            var queryCharAtIndex = query[queryIndex];
            var queryLowerCharAtIndex = queryLower[queryIndex];

            for (int targetIndex = 0; targetIndex < targetLength; targetIndex++)
            {
                var targetIndexGtNull = targetIndex > 0;
                var currentIndex = queryIndexOffset + targetIndex;
                var leftIndex = currentIndex - 1;
                var diagIndex = queryIndexPreviousOffset + targetIndex - 1;

                var leftScore = targetIndexGtNull ? scores[leftIndex] : 0;
                var diagScore = queryIndexGtNull && targetIndexGtNull ? scores[diagIndex] : 0;
                var matchesSequenceLength = queryIndexGtNull && targetIndexGtNull ? matches[diagIndex] : 0;

                // If we are not matching on the first query character any more, we only produce a
                // score if we had a score previously for the last query index (by looking at the diagScore).
                // This makes sure that the query always matches in sequence on the target.
                int score;
                if (diagScore == 0 && queryIndexGtNull)
                {
                    score = 0;
                }
                else
                {
                    score = ComputeCharScore(queryCharAtIndex, queryLowerCharAtIndex, target, targetLower, targetIndex, matchesSequenceLength);
                }

                // We have a score and its equal or larger than the left score
                // Match: sequence continues growing from previous diag value
                // Score: increases by diag score value
                var isValidScore = score > 0 && diagScore + score >= leftScore;
                if (isValidScore && (
                    // We don't need to check if it's contiguous if we allow non-contiguous matches
                    allowNonContiguousMatches ||
                    // We must be looking for a contiguous match.
                    // Looking at an index higher than 0 in the query means we must have already
                    // found out this is contiguous otherwise there wouldn't have been a score
                    queryIndexGtNull ||
                    // lastly check if the query is completely contiguous at this index in the target
                    targetLower.Substring(targetIndex).StartsWith(queryLower)
                ))
                {
                    matches[currentIndex] = matchesSequenceLength + 1;
                    scores[currentIndex] = diagScore + score;
                }
                // We either have no score or the score is lower than the left score
                // Match: reset to 0
                // Score: pick up from left hand side
                else
                {
                    matches[currentIndex] = 0;
                    scores[currentIndex] = leftScore;
                }
            }
        }

        // Restore Positions (starting from bottom right of matrix)
        var positions = new List<int>();
        var queryIndex2 = queryLength - 1;
        var targetIndex2 = targetLength - 1;

        while (queryIndex2 >= 0 && targetIndex2 >= 0)
        {
            var currentIndex = queryIndex2 * targetLength + targetIndex2;
            var match = matches[currentIndex];

            if (match == 0)
            {
                targetIndex2--; // go left
            }
            else
            {
                positions.Add(targetIndex2); // go up and left
                queryIndex2--;
                targetIndex2--;
            }
        }

        positions.Reverse();
        return new FuzzyScore(scores[queryLength * targetLength - 1], positions.ToArray());
    }

    private static int ComputeCharScore(char queryCharAtIndex, char queryLowerCharAtIndex, 
        string target, string targetLower, int targetIndex, int matchesSequenceLength)
    {
        int score = 0;

        if (!ConsiderAsEqual(queryLowerCharAtIndex, targetLower[targetIndex]))
        {
            return score; // no match of characters
        }

        // Character match bonus
        score += 1;

        // Consecutive match bonus: sequences up to 3 get the full bonus (6)
        // and the remainder gets half the bonus (3). This helps reduce the
        // overall boost for long sequence matches.
        if (matchesSequenceLength > 0)
        {
            score += (Math.Min(matchesSequenceLength, 3) * 6) + (Math.Max(0, matchesSequenceLength - 3) * 3);
        }

        // Same case bonus
        if (queryCharAtIndex == target[targetIndex])
        {
            score += 1;
        }

        // Start of word bonus
        if (targetIndex == 0)
        {
            score += 8;
        }
        else
        {
            // After separator bonus
            var separatorBonus = ScoreSeparatorAtPos(target[targetIndex - 1]);
            if (separatorBonus > 0)
            {
                score += separatorBonus;
            }
            // Inside word upper case bonus (camel case). We only give this bonus if we're not in a contiguous sequence.
            // For example:
            // NPE => NullPointerException = boost
            // HTTP => HTTP = not boost
            else if (char.IsUpper(target[targetIndex]) && matchesSequenceLength == 0)
            {
                score += 2;
            }
        }

        return score;
    }

    private static bool ConsiderAsEqual(char a, char b)
    {
        if (a == b)
        {
            return true;
        }

        // Special case path separators: ignore platform differences
        if (a == '/' || a == '\\')
        {
            return b == '/' || b == '\\';
        }

        return false;
    }

    private static int ScoreSeparatorAtPos(char charCode)
    {
        // Path separators
        if (charCode == '/' || charCode == '\\')
        {
            return 5;
        }

        // Word separators
        if (charCode == '-' || charCode == '_' || charCode == ' ' || charCode == '.')
        {
            return 5;
        }

        // Case changes
        if (char.IsUpper(charCode))
        {
            return 2;
        }

        return 0;
    }

    /// <summary>
    /// Sorts a list of items by their fuzzy search scores
    /// </summary>
    /// <typeparam name="T">The type of items to sort</typeparam>
    /// <param name="items">The items to sort</param>
    /// <param name="query">The search query</param>
    /// <param name="nameSelector">Function to get the name to search in</param>
    /// <param name="allowNonContiguousMatches">Whether to allow non-contiguous matches</param>
    public static void SortByFuzzyScore<T>(List<T> items, string query, Func<T, string> nameSelector, bool allowNonContiguousMatches = true)
    {
        if (string.IsNullOrEmpty(query))
        {
            return; // No sorting needed for empty query
        }

        items.Sort((a, b) =>
        {
            var scoreA = GetScore(nameSelector(a), query, allowNonContiguousMatches);
            var scoreB = GetScore(nameSelector(b), query, allowNonContiguousMatches);

            // Higher scores first
            if (scoreA != scoreB)
            {
                return scoreB.CompareTo(scoreA);
            }

            // If scores are equal, sort alphabetically
            return string.Compare(nameSelector(a), nameSelector(b), StringComparison.OrdinalIgnoreCase);
        });
    }
}
