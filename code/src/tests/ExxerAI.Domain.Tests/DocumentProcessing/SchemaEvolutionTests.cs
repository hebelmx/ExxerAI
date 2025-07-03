using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for SchemaEvolution entity
/// </summary>
public class SchemaEvolutionTests
{
	[Fact]
	public void Should_InitializeWithDefaults_When_UsingDefaultConstructor()
	{
		// Act
		var evolution = new SchemaEvolution();

		// Assert
		evolution.Id.ShouldNotBeNullOrEmpty();
		evolution.SchemaId.ShouldBe(string.Empty);
		evolution.PreviousVersion.ShouldBe(0);
		evolution.NewVersion.ShouldBe(0);
		evolution.Changes.ShouldNotBeNull();
		evolution.Changes.ShouldBeEmpty();
		evolution.Reason.ShouldBe(string.Empty);
		evolution.AccuracyImprovement.ShouldBe(0.0f);
		(DateTime.UtcNow - evolution.EvolvedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
	}

	[Fact]
	public void Should_AllowSettingProperties_When_ValidValuesProvided()
	{
		// Arrange
		var evolution = new SchemaEvolution();

		// Act
		evolution.SchemaId = "schema-v2";
		evolution.PreviousVersion = 1;
		evolution.NewVersion = 2;
		evolution.Changes = ["Added field: TaxRate", "Updated validation"];
		evolution.Reason = "Improve accuracy";
		evolution.AccuracyImprovement = 0.15f;

		// Assert
		evolution.SchemaId.ShouldBe("schema-v2");
		evolution.PreviousVersion.ShouldBe(1);
		evolution.NewVersion.ShouldBe(2);
		evolution.Changes.Count.ShouldBe(2);
		evolution.Changes[0].ShouldBe("Added field: TaxRate");
		evolution.Changes[1].ShouldBe("Updated validation");
		evolution.Reason.ShouldBe("Improve accuracy");
		evolution.AccuracyImprovement.ShouldBe(0.15f);
	}
} 