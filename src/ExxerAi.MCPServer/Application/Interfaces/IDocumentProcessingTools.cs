namespace ExxerAi.MCPServer.Application.Interfaces;

/// <summary>
/// Interface for document processing MCP tools providing adaptive document intelligence capabilities
/// </summary>
public interface IDocumentProcessingTools
{
	/// <summary>
	/// Processes a document with adaptive polymorphic intelligence
	/// </summary>
	/// <param name="documentPath">Path to the document to process</param>
	/// <param name="documentType">Document type (invoice, contract, report, etc.)</param>
	/// <param name="extractionLevel">Level of extraction detail (basic, detailed, comprehensive)</param>
	/// <param name="learningMode">Whether to enable adaptive learning</param>
	/// <returns>Document processing result with extracted data and confidence scores</returns>
	Task<string> ProcessDocumentAsync(string documentPath, string documentType = "auto", string extractionLevel = "detailed", bool learningMode = true);

	/// <summary>
	/// Extracts specific fields from a document using a predefined schema
	/// </summary>
	/// <param name="documentPath">Path to the document</param>
	/// <param name="schemaName">Name of the extraction schema to use</param>
	/// <param name="fieldNames">Comma-separated list of specific fields to extract</param>
	/// <returns>Extracted field data with confidence scores</returns>
	Task<string> ExtractFieldsAsync(string documentPath, string schemaName, string fieldNames = "");

	/// <summary>
	/// Validates and grounds extracted data against business rules and context
	/// </summary>
	/// <param name="extractionId">ID of the extraction to validate</param>
	/// <param name="businessRules">Business rules to apply during validation</param>
	/// <param name="contextData">Additional context data for grounding</param>
	/// <returns>Validation results with confidence and error details</returns>
	Task<string> ValidateExtractedDataAsync(string extractionId, string businessRules = "moderate", string contextData = "");

	/// <summary>
	/// Learns and adapts document processing schemas from sample documents
	/// </summary>
	/// <param name="sampleDocuments">Paths to sample documents for learning</param>
	/// <param name="documentType">Type of documents for schema learning</param>
	/// <param name="learningMode">Learning mode: incremental, full_retrain, adaptive</param>
	/// <returns>Schema learning results and confidence improvements</returns>
	Task<string> LearnDocumentSchemaAsync(string sampleDocuments, string documentType, string learningMode = "adaptive");

	/// <summary>
	/// Gets processing confidence for a specific document type
	/// </summary>
	/// <param name="documentType">Document type to assess</param>
	/// <param name="complexityLevel">Document complexity: simple, medium, complex</param>
	/// <returns>Confidence assessment and processing capabilities</returns>
	Task<string> GetProcessingConfidenceAsync(string documentType, string complexityLevel = "medium");
} 