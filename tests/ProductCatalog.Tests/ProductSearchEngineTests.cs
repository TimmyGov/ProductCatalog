using ProductCatalog.Application.Services.SearchEngine;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Tests;

public class ProductSearchEngineTests
{
    private class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
    }

    [Fact]
    public void Search_ExactMatch_ReturnsHighestScore()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Laptop" },
            new() { Id = 2, Name = "Laptop Pro" },
            new() { Id = 3, Name = "Desktop" }
        };

        // Act
        var results = searchEngine.Search(products, "Laptop", threshold: 0.3).ToList();

        // Assert
        Assert.NotEmpty(results);
        var exactMatch = results.First(r => r.Item.Id == 1);
        Assert.Equal(1.0, exactMatch.Score);
        Assert.Equal("Laptop", exactMatch.Item.Name);
    }

    [Fact]
    public void Search_FuzzyMatch_ReturnsMatchesWithinLevenshteinDistance()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>(maxLevenshteinDistance: 2);
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Laptop" },  // Distance: 1 from "Laptap"
            new() { Id = 2, Name = "Desktop" }, // No match
            new() { Id = 3, Name = "Lapton" }   // Distance: 1 from "Laptap"
        };

        // Act
        var results = searchEngine.Search(products, "Laptap", threshold: 0.1).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Item.Name == "Laptop");
        Assert.DoesNotContain(results, r => r.Item.Name == "Desktop");
    }

    [Fact]
    public void Search_MultiFieldWeightedScoring_PrioritizesHigherWeightFields()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 2.0);        // Higher weight
        searchEngine.AddSearchField(p => p.Description, weight: 1.0); // Lower weight
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Other", Description = "Gaming Laptop" },
            new() { Id = 2, Name = "Gaming Laptop", Description = "Other" }
        };

        // Act
        var results = searchEngine.Search(products, "Gaming Laptop", threshold: 0.3).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        // Product with match in higher-weighted field (Name) should score higher
        Assert.Equal(2, results[0].Item.Id);
        Assert.True(results[0].Score > results[1].Score);
    }

    [Fact]
    public void Search_Performance_HandlesLargeDataset()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        searchEngine.AddSearchField(p => p.Description, weight: 0.8);
        searchEngine.AddSearchField(p => p.SKU, weight: 0.5);
        
        // Generate 10,000+ products
        var products = new List<TestProduct>();
        for (int i = 0; i < 10000; i++)
        {
            products.Add(new TestProduct
            {
                Id = i,
                Name = $"Product {i}",
                Description = $"Description for product {i}",
                SKU = $"SKU-{i:D5}"
            });
        }
        
        // Add some target products
        products.Add(new TestProduct { Id = 10000, Name = "Gaming Laptop", Description = "High performance", SKU = "GAME-001" });
        products.Add(new TestProduct { Id = 10001, Name = "Gaming Mouse", Description = "Precision gaming", SKU = "GAME-002" });
        products.Add(new TestProduct { Id = 10002, Name = "Gaming Keyboard", Description = "Mechanical gaming", SKU = "GAME-003" });

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var results = searchEngine.Search(products, "Gaming", threshold: 0.3).ToList();
        stopwatch.Stop();

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Item.Name == "Gaming Laptop");
        Assert.Contains(results, r => r.Item.Name == "Gaming Mouse");
        Assert.Contains(results, r => r.Item.Name == "Gaming Keyboard");
        
        // Performance assertion: should complete in reasonable time (< 5 seconds for 10k+ items)
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Search took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public void Search_CaseInsensitive_MatchesRegardlessOfCase()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "LAPTOP" },
            new() { Id = 2, Name = "laptop" },
            new() { Id = 3, Name = "LaPtOp" },
            new() { Id = 4, Name = "Laptop" }
        };

        // Act
        var results = searchEngine.Search(products, "laptop", threshold: 0.3).ToList();

        // Assert
        Assert.Equal(4, results.Count);
        Assert.All(results, r => Assert.Equal(1.0, r.Score)); // All should be exact matches
    }

    [Fact]
    public void Search_EmptySearchTerm_ReturnsEmptyResults()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Laptop" },
            new() { Id = 2, Name = "Desktop" }
        };

        // Act & Assert - Empty string
        var results1 = searchEngine.Search(products, "", threshold: 0.3).ToList();
        Assert.Empty(results1);

        // Act & Assert - Null string
        var results2 = searchEngine.Search(products, null!, threshold: 0.3).ToList();
        Assert.Empty(results2);

        // Act & Assert - Whitespace only
        var results3 = searchEngine.Search(products, "   ", threshold: 0.3).ToList();
        Assert.Empty(results3);
    }

    [Fact]
    public void Search_NoResults_ReturnsEmptyCollection()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Laptop" },
            new() { Id = 2, Name = "Desktop" },
            new() { Id = 3, Name = "Monitor" }
        };

        // Act
        var results = searchEngine.Search(products, "Smartphone", threshold: 0.3).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Search_PartialMatch_ReturnsRelevantResults()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Gaming Laptop" },
            new() { Id = 2, Name = "Gaming Mouse" },
            new() { Id = 3, Name = "Office Laptop" },
            new() { Id = 4, Name = "Desktop Computer" }
        };

        // Act
        var results = searchEngine.Search(products, "Gaming", threshold: 0.3).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Item.Name == "Gaming Laptop");
        Assert.Contains(results, r => r.Item.Name == "Gaming Mouse");
    }

    [Fact]
    public void Search_StartsWithMatch_ScoresHigherThanContains()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Laptop Gaming" },  // Contains
            new() { Id = 2, Name = "Gaming Laptop" }   // Starts with
        };

        // Act
        var results = searchEngine.Search(products, "Gaming", threshold: 0.3).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(2, results[0].Item.Id); // "Gaming Laptop" should score higher
        Assert.True(results[0].Score > results[1].Score);
    }

    [Fact]
    public void Search_MultipleWords_MatchesAllWords()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Gaming Laptop Professional" },
            new() { Id = 2, Name = "Gaming Mouse" },
            new() { Id = 3, Name = "Professional Desktop" }
        };

        // Act
        var results = searchEngine.Search(products, "Gaming Professional", threshold: 0.3).ToList();

        // Assert
        Assert.NotEmpty(results);
        var bestMatch = results.First();
        Assert.Equal(1, bestMatch.Item.Id);
    }

    [Fact]
    public void Search_ThresholdFiltering_ExcludesLowScores()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Computer" },         // No match
            new() { Id = 2, Name = "Desktop Gaming PC" }, // Contains: score ~ 0.7-0.8
            new() { Id = 3, Name = "Gaming Mouse" }      // Starts with: score ~ 0.9
        };

        // Act - High threshold (only "Gaming Mouse" should pass)
        var resultsHighThreshold = searchEngine.Search(products, "Gaming", threshold: 0.85).ToList();
        
        // Act - Low threshold (should get both Gaming items)
        var resultsLowThreshold = searchEngine.Search(products, "Gaming", threshold: 0.3).ToList();

        // Assert
        Assert.Single(resultsHighThreshold);
        Assert.Equal("Gaming Mouse", resultsHighThreshold[0].Item.Name);
        Assert.Equal(2, resultsLowThreshold.Count);
        Assert.True(resultsLowThreshold.Count > resultsHighThreshold.Count);
    }

    [Fact]
    public void Search_NullFieldValues_HandlesGracefully()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        searchEngine.AddSearchField(p => p.Description, weight: 0.8);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Laptop", Description = null! },
            new() { Id = 2, Name = null!, Description = "Gaming" },
            new() { Id = 3, Name = "Gaming Mouse", Description = "Wireless" }
        };

        // Act
        var results = searchEngine.Search(products, "Gaming", threshold: 0.3).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Item.Id == 3);
    }

    [Fact]
    public void Search_NoFieldsRegistered_ThrowsException()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        var products = new List<TestProduct> { new() { Id = 1, Name = "Laptop" } };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            searchEngine.Search(products, "Laptop", threshold: 0.3).ToList());
        
        Assert.Contains("No search fields have been registered", exception.Message);
    }

    [Fact]
    public void Search_OrdersByScoreDescending()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, Name = "Professional Gaming Laptop" }, // Contains
            new() { Id = 2, Name = "Gaming" },                     // Exact
            new() { Id = 3, Name = "Gaming Laptop" },              // Starts with
            new() { Id = 4, Name = "Laptop Gaming Computer" }      // Contains
        };

        // Act
        var results = searchEngine.Search(products, "Gaming", threshold: 0.3).ToList();

        // Assert
        Assert.True(results.Count > 1);
        for (int i = 1; i < results.Count; i++)
        {
            Assert.True(results[i - 1].Score >= results[i].Score,
                $"Results should be ordered by score descending. Index {i-1} score: {results[i-1].Score}, Index {i} score: {results[i].Score}");
        }
    }

    [Fact]
    public void Search_WithRealProductEntity_WorksCorrectly()
    {
        // Arrange
        var searchEngine = new SearchEngine<Product>();
        searchEngine.AddSearchField(p => p.Name, weight: 2.0);
        searchEngine.AddSearchField(p => p.Description, weight: 1.5);
        searchEngine.AddSearchField(p => p.SKU, weight: 1.0);
        
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Gaming Laptop", Description = "High performance laptop", SKU = "GAME-LAP-001", Price = 1200 },
            new() { Id = 2, Name = "Office Laptop", Description = "Business laptop", SKU = "OFF-LAP-001", Price = 800 },
            new() { Id = 3, Name = "Gaming Mouse", Description = "RGB gaming mouse", SKU = "GAME-MOU-001", Price = 50 }
        };

        // Act
        var results = searchEngine.Search(products, "Gaming", threshold: 0.3).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Item.Name == "Gaming Laptop");
        Assert.Contains(results, r => r.Item.Name == "Gaming Mouse");
        
        // Verify all results meet minimum threshold
        Assert.All(results, r => Assert.True(r.Score >= 0.3));
        
        // "Gaming Mouse" should score highest as it has "gaming" in both name and description
        Assert.Equal("Gaming Mouse", results[0].Item.Name);
    }

    [Fact]
    public void Search_EmptyProductList_ReturnsEmptyResults()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.Name, weight: 1.0);
        var products = new List<TestProduct>();

        // Act
        var results = searchEngine.Search(products, "Laptop", threshold: 0.3).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Search_SpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var searchEngine = new SearchEngine<TestProduct>();
        searchEngine.AddSearchField(p => p.SKU, weight: 1.0);
        
        var products = new List<TestProduct>
        {
            new() { Id = 1, SKU = "PROD-001" },
            new() { Id = 2, SKU = "PROD-002" },
            new() { Id = 3, SKU = "PROD#001" }
        };

        // Act
        var results = searchEngine.Search(products, "PROD-001", threshold: 0.9).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("PROD-001", results[0].Item.SKU);
        Assert.Equal(1.0, results[0].Score); // Exact match
    }
}
