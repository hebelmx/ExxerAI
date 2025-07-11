namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for Conversation domain entity
/// </summary>
public class ConversationTests
{
    [Fact]
    public void Should_CreateConversation_When_ValidDataProvided()
    {
        // Arrange & Act
        var conversation = new Conversation
        {
            Title = "Test Conversation",
            AgentId = Guid.NewGuid(),
            LanguageModelId = Guid.NewGuid(),
            Status = ConversationStatus.Active
        };

        // Assert
        conversation.Id.ShouldNotBe(Guid.Empty);
        conversation.Title.ShouldBe("Test Conversation");
        conversation.AgentId.ShouldNotBe(Guid.Empty);
        conversation.LanguageModelId.ShouldNotBe(Guid.Empty);
        conversation.Status.ShouldBe(ConversationStatus.Active);
        conversation.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        conversation.Messages.ShouldNotBeNull();
        conversation.Metadata.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(ConversationStatus.Active))]
    [InlineData(nameof(ConversationStatus.Paused))]
    [InlineData(nameof(ConversationStatus.Completed))]
    [InlineData(nameof(ConversationStatus.Archived))]
    public void Should_HandleConversationStatuses_When_DifferentStatesProvided(string statusName)
    {
        // Arrange
        var conversation = new Conversation();
        var expectedStatus = EnumModelHelper.FromName<ConversationStatus>(statusName);

        // Act
        conversation.Status = expectedStatus;

        // Assert
        conversation.Status.ShouldBe(expectedStatus);
    }
}