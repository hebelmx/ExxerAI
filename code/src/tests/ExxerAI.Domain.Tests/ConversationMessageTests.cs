namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for ConversationMessage domain entity
/// </summary>
public class ConversationMessageTests
{
    [Fact]
    public void Should_CreateMessage_When_ValidDataProvided()
    {
        // Arrange & Act
        var message = new ConversationMessage
        {
            ConversationId = Guid.NewGuid(),
            Role = MessageRole.User,
            Content = "Hello, world!",
            TokenCount = 3
        };

        // Assert
        message.Id.ShouldNotBe(Guid.Empty);
        message.ConversationId.ShouldNotBe(Guid.Empty);
        message.Role.ShouldBe(MessageRole.User);
        message.Content.ShouldBe("Hello, world!");
        message.TokenCount.ShouldBe(3);
        message.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        message.Metadata.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(MessageRole.System))]
    [InlineData(nameof(MessageRole.User))]
    [InlineData(nameof(MessageRole.Assistant))]
    [InlineData(nameof(MessageRole.Function))]
    public void Should_HandleMessageRoles_When_DifferentRolesProvided(string roleName)
    {
        // Arrange
        var message = new ConversationMessage();
        var expectedRole = EnumModelHelper.FromName<MessageRole>(roleName);

        // Act
        message.Role = expectedRole;

        // Assert
        message.Role.ShouldBe(expectedRole);
    }
}