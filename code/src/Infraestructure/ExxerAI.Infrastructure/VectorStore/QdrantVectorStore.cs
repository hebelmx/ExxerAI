using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.RegularExpressions;

namespace ExxerAI.Infrastructure.VectorStore;

/// <summary>
/// Qdrant implementation of vector store for semantic search
/// Provides high-performance vector similarity search capabilities
/// </summary>
public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly string _collectionName;
    private readonly int _vectorSize;

    private static readonly object _initLock = new();
    private bool _isInitialized = false;

    public QdrantVectorStore(
        QdrantClient client,
        ILogger<QdrantVectorStore> logger,
        string collectionName = "exxerai_documents",
        int vectorSize = 1536) // OpenAI text-embedding-3-small default
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collectionName = collectionName;
        _vectorSize = vectorSize;
    }

    /// <summary>
    /// Initialize collection with proper configuration for document embeddings
    /// </summary>
    public async Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_initLock)
            {
                if (_isInitialized) return Result.Success();
            }

            _logger.LogInformation("Initializing Qdrant collection: {CollectionName}", _collectionName);

            // Check if collection exists
            var collections = await _client.ListCollectionsAsync(cancellationToken);
            var collectionExists = collections.Any(c => c == _collectionName);

            if (!collectionExists)
            {
                _logger.LogInformation("Creating new collection: {CollectionName} with vector size: {VectorSize}",
                    _collectionName, _vectorSize);

                // Create collection with optimized settings for document search
                await _client.CreateCollectionAsync(
                    collectionName: _collectionName,
                    vectorsConfig: new VectorParams
                    {
                        Size = (ulong)_vectorSize,
                        Distance = Distance.Cosine, // Best for text embeddings
                        HnswConfig = new HnswConfigDiff
                        {
                            M = 16, // Number of bi-directional links for every new element during construction
                            EfConstruct = 100, // Size of dynamic candidate list
                            FullScanThreshold = 10000, // Minimal size (in KiB) of payload index to use full scan
                            MaxIndexingThreads = 0 // Auto-detect
                        },
                        QuantizationConfig = new QuantizationConfig
                        {
                            Scalar = new ScalarQuantization
                            {
                                Type = QuantizationType.Int8, // 8-bit quantization for memory efficiency
                                Quantile = 0.99f,
                                AlwaysRam = false
                            }
                        }
                    },
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Collection {CollectionName} created successfully", _collectionName);
            }
            else
            {
                _logger.LogInformation("Collection {CollectionName} already exists", _collectionName);
            }

            // Note: Field indexes are created automatically when documents are inserted

            lock (_initLock)
            {
                _isInitialized = true;
            }

            _logger.LogInformation("Qdrant vector store initialized successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Qdrant vector store");
            return Result.WithFailure($"Vector store initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Store document embedding with metadata
    /// </summary>
    public async Task<Result> StoreEmbeddingAsync(
        string documentId,
        string content,
        float[] embeddings,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.WithFailure("Document ID cannot be null or empty");

            if (embeddings == null || embeddings.Length != _vectorSize)
                return Result.WithFailure($"Embeddings must be exactly {_vectorSize} dimensions");

            await EnsureInitializedAsync(cancellationToken);

            var payload = CreatePayload(documentId, content, metadata ?? []);
            var pointId = Guid.NewGuid().ToString();

            var pointStruct = new PointStruct
            {
                Id = new PointId { Uuid = pointId },
                Vectors = embeddings,
                Payload = { payload }
            };

            await _client.UpsertAsync(
                collectionName: _collectionName,
                points: new[] { pointStruct },
                cancellationToken: cancellationToken);

            _logger.LogDebug("Stored embedding for document: {DocumentId} with {Dimensions} dimensions",
                documentId, embeddings.Length);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store embedding for document: {DocumentId}", documentId);
            return Result.WithFailure($"Failed to store embedding: {ex.Message}");
        }
    }

    /// <summary>
    /// Search for similar documents using vector similarity
    /// </summary>
    public async Task<Result<IEnumerable<VectorSearchResult>>> SearchSimilarAsync(
        float[] queryEmbedding,
        int limit = 10,
        float threshold = 0.7f,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (queryEmbedding == null || queryEmbedding.Length != _vectorSize)
                return Result<IEnumerable<VectorSearchResult>>.WithFailure(
                    $"Query embedding must be exactly {_vectorSize} dimensions");

            await EnsureInitializedAsync(cancellationToken);

            var searchParams = new SearchParams
            {
                HnswEf = 128, // Size of dynamic candidate list for search
                Exact = false // Use approximate search for speed
            };

            Filter searchFilter = null!;
            if (filter != null && filter.Any())
            {
                searchFilter = BuildFilter(filter);
            }

            var searchResults = await _client.SearchAsync(
                collectionName: _collectionName,
                vector: queryEmbedding,
                limit: (ulong)limit,
                scoreThreshold: threshold,
                filter: searchFilter,
                searchParams: searchParams,
                cancellationToken: cancellationToken);

            var results = searchResults.Select(result => new VectorSearchResult
            {
                DocumentId = result.Payload["document_id"].StringValue,
                Content = result.Payload.ContainsKey("content") ? result.Payload["content"].StringValue : string.Empty,
                Score = result.Score,
                Metadata = ExtractMetadata(result.Payload)
            }).ToList();

            _logger.LogDebug("Found {Count} similar documents with threshold {Threshold}", results.Count, threshold);

            return Result<IEnumerable<VectorSearchResult>>.Success(results.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search similar documents");
            return Result<IEnumerable<VectorSearchResult>>.WithFailure($"Search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete document embedding
    /// </summary>
    public async Task<Result> DeleteEmbeddingAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.WithFailure("Document ID cannot be null or empty");

            await EnsureInitializedAsync(cancellationToken);

            var filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "document_id",
                            Match = new Qdrant.Client.Grpc.Match { Text = documentId }
                        }
                    }
                }
            };

            await _client.DeleteAsync(
                collectionName: _collectionName,
                filter: filter,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Deleted embedding for document: {DocumentId}", documentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete embedding for document: {DocumentId}", documentId);
            return Result.WithFailure($"Failed to delete embedding: {ex.Message}");
        }
    }

    /// <summary>
    /// Update existing document embedding
    /// </summary>
    public async Task<Result> UpdateEmbeddingAsync(
        string documentId,
        string content,
        float[] embeddings,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        // For Qdrant, update is the same as upsert
        return await StoreEmbeddingAsync(documentId, content, embeddings, metadata, cancellationToken);
    }

    /// <summary>
    /// Get collection statistics
    /// </summary>
    public async Task<Result<VectorStoreStats>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureInitializedAsync(cancellationToken);

            var collectionInfo = await _client.GetCollectionInfoAsync(_collectionName, cancellationToken);

            var stats = new VectorStoreStats
            {
                TotalVectors = (long)collectionInfo.PointsCount,
                CollectionSize = (long)((ulong)collectionInfo.PointsCount * (ulong)_vectorSize * sizeof(float)),
                VectorDimensions = _vectorSize,
                IndexingStatus = collectionInfo.Status.ToString(),
                LastUpdated = DateTime.UtcNow,
                AdditionalStats = new Dictionary<string, object>
                {
                    ["segments_count"] = collectionInfo.SegmentsCount,
                    ["indexed_vectors_count"] = collectionInfo.IndexedVectorsCount,
                    ["config"] = collectionInfo.Config?.ToString() ?? "N/A"
                }
            };

            return Result<VectorStoreStats>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vector store statistics");
            return Result<VectorStoreStats>.WithFailure($"Failed to get stats: {ex.Message}");
        }
    }

    /// <summary>
    /// Batch store operations for efficiency
    /// </summary>
    public async Task<Result> StoreBatchAsync(
        IEnumerable<VectorStoreItem> items,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var itemList = items.ToList();
            if (!itemList.Any())
                return Result.Success();

            await EnsureInitializedAsync(cancellationToken);

            var points = itemList.Select(item =>
            {
                if (item.Embeddings.Length != _vectorSize)
                    throw new ArgumentException($"All embeddings must be {_vectorSize} dimensions");

                var payload = CreatePayload(item.DocumentId, item.Content, item.Metadata);
                return new PointStruct
                {
                    Id = new PointId { Uuid = Guid.NewGuid().ToString() },
                    Vectors = item.Embeddings,
                    Payload = { payload }
                };
            }).ToList();

            const int batchSize = 100; // Qdrant recommended batch size
            for (int i = 0; i < points.Count; i += batchSize)
            {
                var batch = points.Skip(i).Take(batchSize).ToList();
                await _client.UpsertAsync(
                    collectionName: _collectionName,
                    points: batch,
                    cancellationToken: cancellationToken);
            }

            _logger.LogInformation("Stored {Count} embeddings in batch", itemList.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store batch embeddings");
            return Result.WithFailure($"Batch store failed: {ex.Message}");
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (!_isInitialized)
        {
            var result = await InitializeAsync(cancellationToken);
            if (result.IsFailure)
                throw new InvalidOperationException($"Vector store initialization failed: {result.Error}");
        }
    }

    private static Dictionary<string, Value> CreatePayload(
        string documentId,
        string content,
        Dictionary<string, object> metadata)
    {
        var payload = new Dictionary<string, Value>
        {
            ["document_id"] = documentId,
            ["content"] = content ?? string.Empty,
            ["created_at"] = DateTime.UtcNow.ToString("O")
        };

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                payload[$"meta_{kvp.Key}"] = ConvertToValue(kvp.Value);
            }
        }

        return payload;
    }

    private static Value ConvertToValue(object obj)
    {
        return obj switch
        {
            string s => s,
            int i => i,
            long l => l,
            double d => d,
            float f => f,
            bool b => b,
            _ => obj?.ToString() ?? string.Empty
        };
    }

    private static Filter BuildFilter(Dictionary<string, object> filterConditions)
    {
        var conditions = filterConditions.Select(kvp => new Condition
        {
            Field = new FieldCondition
            {
                Key = $"meta_{kvp.Key}",
                Match = new Qdrant.Client.Grpc.Match { Text = ConvertToValue(kvp.Value).ToString() }
            }
        }).ToList();

        return new Filter { Must = { conditions } };
    }

    private static Dictionary<string, object> ExtractMetadata(IDictionary<string, Value> payload)
    {
        var metadata = new Dictionary<string, object>();

        foreach (var kvp in payload.Where(p => p.Key.StartsWith("meta_")))
        {
            var key = kvp.Key[5..]; // Remove "meta_" prefix
            metadata[key] = ConvertFromValue(kvp.Value);
        }

        return metadata;
    }

    private static object ConvertFromValue(Value value)
    {
        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue,
            Value.KindOneofCase.DoubleValue => value.DoubleValue,
            Value.KindOneofCase.BoolValue => value.BoolValue,
            _ => value.ToString()
        };
    }
}