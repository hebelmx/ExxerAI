namespace ExxerAI.Application.Tests.Data;

/// <summary>
/// Comprehensive unit tests for KpiExxerproPatternSeed static data class
/// Tests pattern data integrity, filtering methods, and statistical operations
/// </summary>
public class KpiExxerproPatternSeedTests
{
    public class InitialPatternsTests
    {
        [Fact]
        public void InitialPatterns_ShouldNotBeNull()
        {
            // Act & Assert
            KpiExxerproPatternSeed.InitialPatterns.ShouldNotBeNull();
        }

        [Fact]
        public void InitialPatterns_ShouldContainPatterns()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            patterns.ShouldNotBeEmpty();
            patterns.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void InitialPatterns_ShouldHaveValidPatternStructure()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            foreach (var pattern in patterns)
            {
                pattern.ShouldNotBeNull();
                pattern.FieldName.ShouldNotBeNullOrWhiteSpace();
                pattern.DocumentType.ShouldNotBeNullOrWhiteSpace();
                pattern.PatternType.ShouldNotBeNullOrWhiteSpace();
                pattern.PatternExpression.ShouldNotBeNullOrWhiteSpace();
                pattern.ConfidenceScore.ShouldBeGreaterThan(0f);
                pattern.ConfidenceScore.ShouldBeLessThanOrEqualTo(1f);
                pattern.SuccessCount.ShouldBeGreaterThanOrEqualTo(0);
                pattern.TotalAttempts.ShouldBeGreaterThan(0);
                pattern.CreatedBy.ShouldNotBeNullOrWhiteSpace();
                pattern.IsActive.ShouldBeTrue();
            }
        }

        [Fact]
        public void InitialPatterns_ShouldContainRegistroPatronalPatterns()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;
            var registroPatterns = patterns.Where(p => p.FieldName == "registro_patronal").ToList();

            // Assert
            registroPatterns.ShouldNotBeEmpty();
            registroPatterns.Count.ShouldBeGreaterThan(0);
            registroPatterns.All(p => p.DocumentType == "IMSSPayment").ShouldBeTrue();
        }

        [Fact]
        public void InitialPatterns_ShouldContainPeriodoImssPatterns()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;
            var periodoPatterns = patterns.Where(p => p.FieldName == "periodo_imss").ToList();

            // Assert
            periodoPatterns.ShouldNotBeEmpty();
            periodoPatterns.Count.ShouldBeGreaterThan(0);
            periodoPatterns.All(p => p.DocumentType == "IMSSPayment").ShouldBeTrue();
        }

        [Fact]
        public void InitialPatterns_ShouldContainTotalPagarPatterns()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;
            var totalPatterns = patterns.Where(p => p.FieldName == "total_pagar").ToList();

            // Assert
            totalPatterns.ShouldNotBeEmpty();
            totalPatterns.Count.ShouldBeGreaterThan(0);
            totalPatterns.All(p => p.DocumentType == "IMSSPayment").ShouldBeTrue();
        }

        [Fact]
        public void InitialPatterns_ShouldHaveVariousPatternTypes()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;
            var patternTypes = patterns.Select(p => p.PatternType).Distinct().ToList();

            // Assert
            patternTypes.ShouldNotBeEmpty();
            patternTypes.ShouldContain("Regex");
            patternTypes.ShouldContain("Keyword");
            patternTypes.ShouldContain("OCRRegion");
        }

        [Fact]
        public void InitialPatterns_ShouldHaveHighConfidenceScores()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;
            var averageConfidence = patterns.Average(p => p.ConfidenceScore);

            // Assert
            averageConfidence.ShouldBeGreaterThan(0.8f);
            patterns.All(p => p.ConfidenceScore >= 0.8f).ShouldBeTrue();
        }

        [Fact]
        public void InitialPatterns_ShouldHaveValidSuccessRates()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            foreach (var pattern in patterns)
            {
                var successRate = (float)pattern.SuccessCount / pattern.TotalAttempts;
                successRate.ShouldBeGreaterThanOrEqualTo(0f);
                successRate.ShouldBeLessThanOrEqualTo(1f);
                successRate.ShouldBeGreaterThanOrEqualTo(0.8f); // All patterns should have >= 80% success rate
            }
        }

        [Fact]
        public void InitialPatterns_ShouldHaveCreatedByKpiExxerpro()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            patterns.All(p => p.CreatedBy == "KpiExxerpro_Port").ShouldBeTrue();
        }

        [Fact]
        public void InitialPatterns_ShouldAllBeActive()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            patterns.All(p => p.IsActive).ShouldBeTrue();
        }
    }

    public class GetPatternsForDocumentTypeTests
    {
        [Fact]
        public void GetPatternsForDocumentType_WithIMSSPayment_ShouldReturnIMSSPatterns()
        {
            // Arrange
            var documentType = "IMSSPayment";

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForDocumentType(documentType);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldNotBeEmpty();
            patterns.All(p => p.DocumentType == documentType).ShouldBeTrue();
        }

        [Fact]
        public void GetPatternsForDocumentType_WithNonExistentType_ShouldReturnEmptyList()
        {
            // Arrange
            var documentType = "NonExistentDocumentType";

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForDocumentType(documentType);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldBeEmpty();
        }

        [Fact]
        public void GetPatternsForDocumentType_WithNullType_ShouldReturnEmptyList()
        {
            // Arrange
            string documentType = null!;

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForDocumentType(documentType);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldBeEmpty();
        }

        [Fact]
        public void GetPatternsForDocumentType_WithEmptyType_ShouldReturnEmptyList()
        {
            // Arrange
            var documentType = "";

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForDocumentType(documentType);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldBeEmpty();
        }

        [Fact]
        public void GetPatternsForDocumentType_WithCaseSensitive_ShouldMatchExactCase()
        {
            // Arrange
            var documentType = "imsspayment"; // Lowercase

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForDocumentType(documentType);

            // Assert
            patterns.ShouldBeEmpty(); // Should not match "IMSSPayment"
        }
    }

    public class GetPatternsForFieldTests
    {
        [Fact]
        public void GetPatternsForField_WithRegistroPatronal_ShouldReturnOrderedByConfidence()
        {
            // Arrange
            var fieldName = "registro_patronal";

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForField(fieldName);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldNotBeEmpty();
            patterns.All(p => p.FieldName == fieldName).ShouldBeTrue();
            
            // Verify descending order by confidence
            for (int i = 1; i < patterns.Count; i++)
            {
                patterns[i - 1].ConfidenceScore.ShouldBeGreaterThanOrEqualTo(patterns[i].ConfidenceScore);
            }
        }

        [Fact]
        public void GetPatternsForField_WithPeriodoImss_ShouldReturnOrderedByConfidence()
        {
            // Arrange
            var fieldName = "periodo_imss";

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForField(fieldName);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldNotBeEmpty();
            patterns.All(p => p.FieldName == fieldName).ShouldBeTrue();
            
            // Verify descending order by confidence
            for (int i = 1; i < patterns.Count; i++)
            {
                patterns[i - 1].ConfidenceScore.ShouldBeGreaterThanOrEqualTo(patterns[i].ConfidenceScore);
            }
        }

        [Fact]
        public void GetPatternsForField_WithNonExistentField_ShouldReturnEmptyList()
        {
            // Arrange
            var fieldName = "non_existent_field";

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForField(fieldName);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldBeEmpty();
        }

        [Fact]
        public void GetPatternsForField_WithNullField_ShouldReturnEmptyList()
        {
            // Arrange
            string fieldName = null!;

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForField(fieldName);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldBeEmpty();
        }

        [Fact]
        public void GetPatternsForField_WithEmptyField_ShouldReturnEmptyList()
        {
            // Arrange
            var fieldName = "";

            // Act
            var patterns = KpiExxerproPatternSeed.GetPatternsForField(fieldName);

            // Assert
            patterns.ShouldNotBeNull();
            patterns.ShouldBeEmpty();
        }
    }

    public class GetBestPatternForFieldTests
    {
        [Fact]
        public void GetBestPatternForField_WithValidFieldAndDocumentType_ShouldReturnHighestConfidencePattern()
        {
            // Arrange
            var fieldName = "registro_patronal";
            var documentType = "IMSSPayment";

            // Act
            var bestPattern = KpiExxerproPatternSeed.GetBestPatternForField(fieldName, documentType);

            // Assert
            bestPattern.ShouldNotBeNull();
            bestPattern.FieldName.ShouldBe(fieldName);
            bestPattern.DocumentType.ShouldBe(documentType);
            
            // Verify it's the highest confidence for this field/document type combination
            var allPatternsForField = KpiExxerproPatternSeed.InitialPatterns
                .Where(p => p.FieldName == fieldName && p.DocumentType == documentType)
                .ToList();
            
            var maxConfidence = allPatternsForField.Max(p => p.ConfidenceScore);
            bestPattern.ConfidenceScore.ShouldBe(maxConfidence);
        }

        [Fact]
        public void GetBestPatternForField_WithNonExistentField_ShouldReturnNull()
        {
            // Arrange
            var fieldName = "non_existent_field";
            var documentType = "IMSSPayment";

            // Act
            var bestPattern = KpiExxerproPatternSeed.GetBestPatternForField(fieldName, documentType);

            // Assert
            bestPattern.ShouldBeNull();
        }

        [Fact]
        public void GetBestPatternForField_WithNonExistentDocumentType_ShouldReturnNull()
        {
            // Arrange
            var fieldName = "registro_patronal";
            var documentType = "NonExistentType";

            // Act
            var bestPattern = KpiExxerproPatternSeed.GetBestPatternForField(fieldName, documentType);

            // Assert
            bestPattern.ShouldBeNull();
        }

        [Fact]
        public void GetBestPatternForField_WithNullParameters_ShouldReturnNull()
        {
            // Act & Assert
            KpiExxerproPatternSeed.GetBestPatternForField(null!, "IMSSPayment").ShouldBeNull();
            KpiExxerproPatternSeed.GetBestPatternForField("registro_patronal", null!).ShouldBeNull();
            KpiExxerproPatternSeed.GetBestPatternForField(null!, null!).ShouldBeNull();
        }

        [Fact]
        public void GetBestPatternForField_WithEmptyParameters_ShouldReturnNull()
        {
            // Act & Assert
            KpiExxerproPatternSeed.GetBestPatternForField("", "IMSSPayment").ShouldBeNull();
            KpiExxerproPatternSeed.GetBestPatternForField("registro_patronal", "").ShouldBeNull();
            KpiExxerproPatternSeed.GetBestPatternForField("", "").ShouldBeNull();
        }
    }

    public class GetStatisticsTests
    {
        [Fact]
        public void GetStatistics_ShouldReturnValidStatistics()
        {
            // Act
            var statistics = KpiExxerproPatternSeed.GetStatistics();

            // Assert
            statistics.ShouldNotBeNull();
            statistics.TotalPatterns.ShouldBeGreaterThan(0);
            statistics.UniqueFields.ShouldBeGreaterThan(0);
            statistics.UniqueDocumentTypes.ShouldBeGreaterThan(0);
            statistics.AverageConfidence.ShouldBeGreaterThan(0f);
            statistics.AverageConfidence.ShouldBeLessThanOrEqualTo(1f);
            statistics.TotalSuccessfulExtractions.ShouldBeGreaterThan(0);
            statistics.TotalAttempts.ShouldBeGreaterThan(0);
            statistics.OverallSuccessRate.ShouldBeGreaterThan(0f);
            statistics.OverallSuccessRate.ShouldBeLessThanOrEqualTo(1f);
        }

        [Fact]
        public void GetStatistics_ShouldHaveConsistentCounts()
        {
            // Act
            var statistics = KpiExxerproPatternSeed.GetStatistics();
            var actualPatterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            statistics.TotalPatterns.ShouldBe(actualPatterns.Count);
            
            var actualUniqueFields = actualPatterns.Select(p => p.FieldName).Distinct().Count();
            statistics.UniqueFields.ShouldBe(actualUniqueFields);
            
            var actualUniqueDocumentTypes = actualPatterns.Select(p => p.DocumentType).Distinct().Count();
            statistics.UniqueDocumentTypes.ShouldBe(actualUniqueDocumentTypes);
            
            var actualTotalSuccessful = actualPatterns.Sum(p => p.SuccessCount);
            statistics.TotalSuccessfulExtractions.ShouldBe(actualTotalSuccessful);
            
            var actualTotalAttempts = actualPatterns.Sum(p => p.TotalAttempts);
            statistics.TotalAttempts.ShouldBe(actualTotalAttempts);
        }

        [Fact]
        public void GetStatistics_ShouldHaveCorrectAverageConfidence()
        {
            // Act
            var statistics = KpiExxerproPatternSeed.GetStatistics();
            var actualPatterns = KpiExxerproPatternSeed.InitialPatterns;
            var expectedAverageConfidence = actualPatterns.Average(p => p.ConfidenceScore);

            // Assert
            Math.Abs(statistics.AverageConfidence - expectedAverageConfidence).ShouldBeLessThan(0.0001f);
        }

        [Fact]
        public void GetStatistics_ShouldHaveCorrectOverallSuccessRate()
        {
            // Act
            var statistics = KpiExxerproPatternSeed.GetStatistics();
            var actualPatterns = KpiExxerproPatternSeed.InitialPatterns;
            var expectedSuccessRate = (float)actualPatterns.Sum(p => p.SuccessCount) / actualPatterns.Sum(p => p.TotalAttempts);

            // Assert
            Math.Abs(statistics.OverallSuccessRate - expectedSuccessRate).ShouldBeLessThan(0.0001f);
        }

        [Fact]
        public void GetStatistics_ShouldHaveHighQualityMetrics()
        {
            // Act
            var statistics = KpiExxerproPatternSeed.GetStatistics();

            // Assert
            statistics.AverageConfidence.ShouldBeGreaterThan(0.85f); // High average confidence
            statistics.OverallSuccessRate.ShouldBeGreaterThan(0.85f); // High overall success rate
            statistics.UniqueFields.ShouldBeGreaterThan(10); // Good field coverage
        }
    }

    public class PatternSeedStatisticsTests
    {
        [Fact]
        public void PatternSeedStatistics_ShouldHaveDefaultValues()
        {
            // Act
            var statistics = new PatternSeedStatistics();

            // Assert
            statistics.TotalPatterns.ShouldBe(0);
            statistics.UniqueFields.ShouldBe(0);
            statistics.UniqueDocumentTypes.ShouldBe(0);
            statistics.AverageConfidence.ShouldBe(0f);
            statistics.TotalSuccessfulExtractions.ShouldBe(0);
            statistics.TotalAttempts.ShouldBe(0);
            statistics.OverallSuccessRate.ShouldBe(0f);
        }

        [Fact]
        public void PatternSeedStatistics_ShouldAllowPropertyAssignment()
        {
            // Arrange
            var statistics = new PatternSeedStatistics();

            // Act
            statistics.TotalPatterns = 100;
            statistics.UniqueFields = 15;
            statistics.UniqueDocumentTypes = 3;
            statistics.AverageConfidence = 0.92f;
            statistics.TotalSuccessfulExtractions = 95000;
            statistics.TotalAttempts = 100000;
            statistics.OverallSuccessRate = 0.95f;

            // Assert
            statistics.TotalPatterns.ShouldBe(100);
            statistics.UniqueFields.ShouldBe(15);
            statistics.UniqueDocumentTypes.ShouldBe(3);
            statistics.AverageConfidence.ShouldBe(0.92f);
            statistics.TotalSuccessfulExtractions.ShouldBe(95000);
            statistics.TotalAttempts.ShouldBe(100000);
            statistics.OverallSuccessRate.ShouldBe(0.95f);
        }
    }

    public class DataIntegrityTests
    {
        [Fact]
        public void InitialPatterns_ShouldHaveExpectedFieldNames()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;
            var fieldNames = patterns.Select(p => p.FieldName).Distinct().ToList();

            // Assert
            fieldNames.ShouldContain("registro_patronal");
            fieldNames.ShouldContain("periodo_imss");
            fieldNames.ShouldContain("periodo_rcv");
            fieldNames.ShouldContain("dias_cotizar");
            fieldNames.ShouldContain("num_cotizantes");
            fieldNames.ShouldContain("valor_uma");
            fieldNames.ShouldContain("cuota_fija");
            fieldNames.ShouldContain("riesgos_trabajo");
            fieldNames.ShouldContain("guarderias");
            fieldNames.ShouldContain("subtotal_imss");
            fieldNames.ShouldContain("rcv");
            fieldNames.ShouldContain("total_pagar");
        }

        [Fact]
        public void InitialPatterns_ShouldHaveValidPatternExpressions()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;
            var regexPatterns = patterns.Where(p => p.PatternType == "Regex").ToList();

            // Assert
            foreach (var pattern in regexPatterns)
            {
                // Test that regex patterns don't throw exceptions when created
                Should.NotThrow(() => new System.Text.RegularExpressions.Regex(pattern.PatternExpression, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase));
            }
        }

        [Fact]
        public void InitialPatterns_ShouldHaveLogicalSuccessRates()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            foreach (var pattern in patterns)
            {
                pattern.SuccessCount.ShouldBeLessThanOrEqualTo(pattern.TotalAttempts);
                
                var successRate = (float)pattern.SuccessCount / pattern.TotalAttempts;
                // Success rate should be reasonable (not too perfect, not too poor)
                successRate.ShouldBeGreaterThan(0.7f);
                successRate.ShouldBeLessThanOrEqualTo(1.0f);
            }
        }

        [Fact]
        public void InitialPatterns_ShouldHaveReasonableSampleSizes()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            foreach (var pattern in patterns)
            {
                // All patterns should be based on reasonable sample sizes
                pattern.TotalAttempts.ShouldBeGreaterThanOrEqualTo(1000);
                pattern.TotalAttempts.ShouldBeLessThanOrEqualTo(50000);
                pattern.SuccessCount.ShouldBeGreaterThan(0);
            }
        }
    }
} 