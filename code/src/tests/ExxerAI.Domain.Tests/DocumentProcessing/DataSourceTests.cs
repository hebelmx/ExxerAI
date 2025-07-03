using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for DataSource domain entity
/// </summary>
public class DataSourceTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DataSourceCreated()
    {
        // Act
        var dataSource = new DataSource();

        // Assert
        dataSource.Type.ShouldBe(string.Empty);
        dataSource.Id.ShouldBe(string.Empty);
        dataSource.Path.ShouldBe(string.Empty);
        dataSource.ProcessedBy.ShouldBe(string.Empty);
        dataSource.MCPSessionId.ShouldBeNull();
        dataSource.Metadata.ShouldNotBeNull();
        dataSource.Metadata.ShouldBeEmpty();
        dataSource.AccessedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Should_SetProperties_When_DataSourceInitializedWithValues()
    {
        // Arrange
        var accessTime = DateTime.UtcNow.AddHours(-2);

        // Act
        var dataSource = new DataSource
        {
            Type = "GoogleDrive",
            Id = "gdrive-doc-123",
            Path = "/business/finance/imss_payments/payment_12_2023.pdf",
            ProcessedBy = "DocumentIntelligenceAgent",
            ProcessedAt = DateTime.UtcNow,
            MCPSessionId = "mcp-session-456",
            AccessedAt = accessTime
        };

        dataSource.Metadata["FileSize"] = 2048;
        dataSource.Metadata["MimeType"] = "application/pdf";

        // Assert
        dataSource.Type.ShouldBe("GoogleDrive");
        dataSource.Id.ShouldBe("gdrive-doc-123");
        dataSource.Path.ShouldBe("/business/finance/imss_payments/payment_12_2023.pdf");
        dataSource.ProcessedBy.ShouldBe("DocumentIntelligenceAgent");
        dataSource.MCPSessionId.ShouldBe("mcp-session-456");
        dataSource.AccessedAt.ShouldBe(accessTime);
        dataSource.Metadata["FileSize"].ShouldBe(2048);
        dataSource.Metadata["MimeType"].ShouldBe("application/pdf");
    }
}