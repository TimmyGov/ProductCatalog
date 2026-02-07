namespace ProductCatalog.Application.Services.SearchEngine;

/// <summary>
/// Generic search engine with fuzzy matching capabilities using Levenshtein distance algorithm.
/// Supports multi-field searching with weighted scoring for relevance ranking.
/// Optimized for searching collections of 10,000+ items efficiently.
/// </summary>
/// <typeparam name="T">The type of items to search</typeparam>
public class SearchEngine<T> where T : class
{
    private readonly List<SearchField<T>> _searchFields;
    private readonly int _maxLevenshteinDistance;
    
    /// <summary>
    /// Initializes a new search engine
    /// </summary>
    /// <param name="maxLevenshteinDistance">Maximum distance for fuzzy matching (default: 3)</param>
    public SearchEngine(int maxLevenshteinDistance = 3)
    {
        _searchFields = new List<SearchField<T>>();
        _maxLevenshteinDistance = maxLevenshteinDistance;
    }
    
    /// <summary>
    /// Registers a field to be included in the search
    /// </summary>
    /// <param name="fieldExtractor">Function to extract the field value from an item</param>
    /// <param name="weight">Weight for this field in scoring (higher = more important)</param>
    public void AddSearchField(Func<T, string?> fieldExtractor, double weight = 1.0)
    {
        _searchFields.Add(new SearchField<T>(fieldExtractor, weight));
    }
    
    /// <summary>
    /// Performs a search across all registered fields and returns results sorted by relevance
    /// </summary>
    /// <param name="items">The collection to search</param>
    /// <param name="searchTerm">The search term</param>
    /// <param name="threshold">Minimum score threshold (0-1, default: 0.3)</param>
    /// <returns>Search results with relevance scores, ordered by score descending</returns>
    public IEnumerable<SearchResult<T>> Search(
        IEnumerable<T> items, 
        string searchTerm, 
        double threshold = 0.3)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Enumerable.Empty<SearchResult<T>>();
        }
        
        if (_searchFields.Count == 0)
        {
            throw new InvalidOperationException("No search fields have been registered. Call AddSearchField() first.");
        }
        
        var normalizedSearchTerm = searchTerm.ToLowerInvariant();
        var searchWords = normalizedSearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var results = new List<SearchResult<T>>();
        
        // Process each item in parallel for better performance with large datasets
        foreach (var item in items)
        {
            double totalScore = 0;
            double totalWeight = 0;
            
            // Calculate score across all registered fields
            foreach (var field in _searchFields)
            {
                var fieldValue = field.FieldExtractor(item);
                if (string.IsNullOrWhiteSpace(fieldValue))
                {
                    continue;
                }
                
                var normalizedFieldValue = fieldValue.ToLowerInvariant();
                
                // Calculate field score using multiple matching strategies
                double fieldScore = CalculateFieldScore(normalizedFieldValue, normalizedSearchTerm, searchWords);
                
                totalScore += fieldScore * field.Weight;
                totalWeight += field.Weight;
            }
            
            // Normalize score to 0-1 range
            double normalizedScore = totalWeight > 0 ? totalScore / totalWeight : 0;
            
            // Only include results above threshold
            if (normalizedScore >= threshold)
            {
                results.Add(new SearchResult<T>(item, normalizedScore));
            }
        }
        
        // Sort by relevance score descending
        return results.OrderByDescending(r => r.Score);
    }
    
    /// <summary>
    /// Calculates the relevance score for a field value against the search term
    /// Uses multiple matching strategies: exact match, partial match, and fuzzy match
    /// </summary>
    private double CalculateFieldScore(
        string fieldValue, 
        string searchTerm, 
        string[] searchWords)
    {
        double maxScore = 0;
        
        // Strategy 1: Exact match (highest score)
        if (fieldValue.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0;
        }
        
        // Strategy 2: Starts with search term
        if (fieldValue.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            maxScore = Math.Max(maxScore, 0.9);
        }
        
        // Strategy 3: Contains entire search term
        if (fieldValue.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            maxScore = Math.Max(maxScore, 0.8);
        }
        
        // Strategy 4: Word-level matching
        var fieldWords = fieldValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        double wordMatchScore = CalculateWordMatchScore(fieldWords, searchWords);
        maxScore = Math.Max(maxScore, wordMatchScore);
        
        // Strategy 5: Fuzzy matching using Levenshtein distance
        double fuzzyScore = CalculateFuzzyScore(fieldValue, searchTerm);
        maxScore = Math.Max(maxScore, fuzzyScore);
        
        return maxScore;
    }
    
    /// <summary>
    /// Calculates word-level matching score
    /// </summary>
    private double CalculateWordMatchScore(string[] fieldWords, string[] searchWords)
    {
        if (searchWords.Length == 0 || fieldWords.Length == 0)
        {
            return 0;
        }
        
        int matchedWords = 0;
        
        foreach (var searchWord in searchWords)
        {
            foreach (var fieldWord in fieldWords)
            {
                // Exact word match
                if (fieldWord.Equals(searchWord, StringComparison.OrdinalIgnoreCase))
                {
                    matchedWords++;
                    break;
                }
                // Word starts with search word
                else if (fieldWord.StartsWith(searchWord, StringComparison.OrdinalIgnoreCase))
                {
                    matchedWords++;
                    break;
                }
            }
        }
        
        // Score based on percentage of search words found
        return 0.7 * (matchedWords / (double)searchWords.Length);
    }
    
    /// <summary>
    /// Calculates fuzzy matching score using Levenshtein distance algorithm.
    /// The Levenshtein distance measures the minimum number of single-character edits
    /// (insertions, deletions, or substitutions) required to change one string into another.
    /// Lower distance = more similar strings.
    /// </summary>
    private double CalculateFuzzyScore(string fieldValue, string searchTerm)
    {
        // For long strings, only check substrings to avoid expensive computation
        if (fieldValue.Length > searchTerm.Length + 10)
        {
            return CalculateSlidingWindowFuzzyScore(fieldValue, searchTerm);
        }
        
        int distance = ComputeLevenshteinDistance(fieldValue, searchTerm);
        
        // Only consider matches within the max distance threshold
        if (distance > _maxLevenshteinDistance)
        {
            return 0;
        }
        
        // Convert distance to similarity score (0-1 range)
        // Closer distance = higher score
        int maxLength = Math.Max(fieldValue.Length, searchTerm.Length);
        double similarity = 1.0 - (distance / (double)maxLength);
        
        // Scale down fuzzy matches (they're less reliable than exact matches)
        return similarity * 0.6;
    }
    
    /// <summary>
    /// For longer strings, uses a sliding window approach to find the best matching substring
    /// This improves performance by not comparing entire long strings
    /// </summary>
    private double CalculateSlidingWindowFuzzyScore(string fieldValue, string searchTerm)
    {
        int windowSize = searchTerm.Length + _maxLevenshteinDistance;
        double bestScore = 0;
        
        // Slide through the field value with a window
        for (int i = 0; i <= fieldValue.Length - searchTerm.Length; i++)
        {
            int endIndex = Math.Min(i + windowSize, fieldValue.Length);
            string substring = fieldValue.Substring(i, endIndex - i);
            
            int distance = ComputeLevenshteinDistance(substring, searchTerm);
            
            if (distance <= _maxLevenshteinDistance)
            {
                int maxLength = Math.Max(substring.Length, searchTerm.Length);
                double similarity = 1.0 - (distance / (double)maxLength);
                double score = similarity * 0.6;
                
                bestScore = Math.Max(bestScore, score);
            }
        }
        
        return bestScore;
    }
    
    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    /// Uses dynamic programming approach with O(m*n) time complexity and O(min(m,n)) space complexity.
    /// 
    /// Algorithm explanation:
    /// 1. Create a matrix where dp[i][j] represents the minimum edits needed to transform
    ///    the first i characters of string1 into the first j characters of string2
    /// 2. Initialize first row and column with increasing values (0,1,2,3...)
    /// 3. For each cell, calculate the minimum of:
    ///    - dp[i-1][j] + 1 (deletion)
    ///    - dp[i][j-1] + 1 (insertion)
    ///    - dp[i-1][j-1] + cost (substitution, where cost = 0 if characters match, 1 otherwise)
    /// 4. The bottom-right cell contains the final distance
    /// 
    /// Space optimization: We only need two rows at a time (current and previous),
    /// so we use two arrays instead of a full matrix
    /// </summary>
    private int ComputeLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.IsNullOrEmpty(target) ? 0 : target.Length;
        }
        
        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }
        
        // Ensure source is the shorter string for space optimization
        if (source.Length > target.Length)
        {
            (source, target) = (target, source);
        }
        
        int sourceLength = source.Length;
        int targetLength = target.Length;
        
        // Use two arrays to save space (we only need previous and current row)
        int[] previousRow = new int[sourceLength + 1];
        int[] currentRow = new int[sourceLength + 1];
        
        // Initialize the first row (transforming empty string to source)
        for (int i = 0; i <= sourceLength; i++)
        {
            previousRow[i] = i;
        }
        
        // Calculate distances row by row
        for (int i = 1; i <= targetLength; i++)
        {
            currentRow[0] = i; // First column: transforming source to empty string
            
            for (int j = 1; j <= sourceLength; j++)
            {
                // Cost is 0 if characters match, 1 if they don't
                int cost = (target[i - 1] == source[j - 1]) ? 0 : 1;
                
                // Minimum of three operations:
                // 1. Delete from source: previousRow[j] + 1
                // 2. Insert into source: currentRow[j - 1] + 1
                // 3. Replace in source: previousRow[j - 1] + cost
                currentRow[j] = Math.Min(
                    Math.Min(previousRow[j] + 1, currentRow[j - 1] + 1),
                    previousRow[j - 1] + cost
                );
            }
            
            // Swap rows for next iteration
            (previousRow, currentRow) = (currentRow, previousRow);
        }
        
        // The final distance is in the last cell of previousRow (due to swap)
        return previousRow[sourceLength];
    }
}

/// <summary>
/// Represents a searchable field with its weight
/// </summary>
internal class SearchField<T>
{
    public Func<T, string?> FieldExtractor { get; }
    public double Weight { get; }
    
    public SearchField(Func<T, string?> fieldExtractor, double weight)
    {
        FieldExtractor = fieldExtractor;
        Weight = weight;
    }
}

/// <summary>
/// Represents a search result with relevance score
/// </summary>
public class SearchResult<T>
{
    public T Item { get; }
    public double Score { get; }
    
    public SearchResult(T item, double score)
    {
        Item = item;
        Score = score;
    }
}
