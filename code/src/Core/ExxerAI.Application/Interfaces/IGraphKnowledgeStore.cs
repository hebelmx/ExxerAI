using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for graph-based knowledge storage and retrieval
/// Manages relationships between documents, concepts, and entities
/// </summary>
public interface IGraphKnowledgeStore
{
    /// <summary>
    /// Initialize the graph database with necessary constraints and indexes
    /// </summary>
    Task<Result> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update a document node in the knowledge graph
    /// </summary>
    /// <param name="document">Document information to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> StoreDocumentAsync(GraphDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update concept nodes extracted from documents
    /// </summary>
    /// <param name="concepts">Concepts to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> StoreConceptsAsync(IEnumerable<GraphConcept> concepts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create relationships between documents and concepts
    /// </summary>
    /// <param name="relationships">Relationships to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> CreateRelationshipsAsync(IEnumerable<GraphRelationship> relationships, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find documents related to a specific concept
    /// </summary>
    /// <param name="conceptName">Name of the concept</param>
    /// <param name="relationshipTypes">Types of relationships to follow</param>
    /// <param name="maxDepth">Maximum traversal depth</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<IEnumerable<GraphDocument>>> FindRelatedDocumentsAsync(
        string conceptName,
        IEnumerable<string>? relationshipTypes = null,
        int maxDepth = 2,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find concepts related to a specific document
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="relationshipTypes">Types of relationships to follow</param>
    /// <param name="maxDepth">Maximum traversal depth</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<IEnumerable<GraphConcept>>> FindRelatedConceptsAsync(
        string documentId,
        IEnumerable<string>? relationshipTypes = null,
        int maxDepth = 2,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute complex graph queries using Cypher
    /// </summary>
    /// <param name="cypherQuery">Cypher query to execute</param>
    /// <param name="parameters">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<IEnumerable<Dictionary<string, object>>>> ExecuteQueryAsync(
        string cypherQuery,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find the shortest path between two entities
    /// </summary>
    /// <param name="fromEntityId">Starting entity ID</param>
    /// <param name="toEntityId">Target entity ID</param>
    /// <param name="relationshipTypes">Types of relationships to traverse</param>
    /// <param name="maxLength">Maximum path length</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<GraphPath>> FindShortestPathAsync(
        string fromEntityId,
        string toEntityId,
        IEnumerable<string>? relationshipTypes = null,
        int maxLength = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get statistics about the knowledge graph
    /// </summary>
    Task<Result<GraphKnowledgeStats>> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document and its relationships from the graph
    /// </summary>
    /// <param name="documentId">Document ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch operations for efficiency
    /// </summary>
    Task<Result> StoreBatchAsync(
        IEnumerable<GraphDocument> documents,
        IEnumerable<GraphConcept> concepts,
        IEnumerable<GraphRelationship> relationships,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a document in the knowledge graph
/// </summary>
public class GraphDocument
{
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Represents a concept extracted from documents
/// </summary>
public class GraphConcept
{
    public string ConceptId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // entity, topic, keyword, etc.
    public string Description { get; set; } = string.Empty;
    public float Confidence { get; set; } = 1.0f;
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<string> Aliases { get; set; } = new();
}

/// <summary>
/// Represents a relationship between nodes in the graph
/// </summary>
public class GraphRelationship
{
    public string FromNodeId { get; set; } = string.Empty;
    public string ToNodeId { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public float Weight { get; set; } = 1.0f;
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a path through the knowledge graph
/// </summary>
public class GraphPath
{
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphRelationship> Relationships { get; set; } = new();
    public int Length { get; set; }
    public float TotalWeight { get; set; }
}

/// <summary>
/// Represents a node in a graph path
/// </summary>
public class GraphNode
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty; // Document, Concept, etc.
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Statistics about the knowledge graph
/// </summary>
public class GraphKnowledgeStats
{
    public long TotalNodes { get; set; }
    public long TotalRelationships { get; set; }
    public long DocumentNodes { get; set; }
    public long ConceptNodes { get; set; }
    public Dictionary<string, long> NodeTypeDistribution { get; set; } = new();
    public Dictionary<string, long> RelationshipTypeDistribution { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}