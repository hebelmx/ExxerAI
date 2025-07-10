using ExxerAI.Domain;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// ?? HYBRID DOCUMENT PROCESSOR COMPREHENSIVE TESTS FOR EXXERAI PROJECT
///
/// Comprehensive unit tests for HybridDocumentProcessor using xUnit v3, Shouldly, and NSubstitute.
/// Tests all major functionality including multi-stage processing pipeline, batch operations, and learning.
/// 
/// This processor implements the core document processing pipeline:
/// - Direct Text Extraction ? OCR Fallback ? Pattern Matching ? Field Extraction ? Validation
/// 
/// Technology Stack:
/// - xUnit v3 for test framework  
/// - Shouldly for assertions (NOT FluentAssertions)
/// - NSubstitute for mocking (NOT Moq)
/// - Result&lt;T&gt; for functional error handling
/// - Hybrid processing with learning capabilities
/// 
/// -------------------------------------------------------------------------------------------------
/// PRAGMA WARNING SUPPRESSIONS FOR PRODUCTION QUALITY TESTING
/// -------------------------------------------------------------------------------------------------
/// 
/// This solution ships with TreatWarningsAsErrors=true for production quality.
/// Pragma warning suppressions in these tests are necessary to verify cancellation behavior.
/// 
/// ASYNCFIXER02 SUPPRESSION JUSTIFICATION:
/// ---------------------------------------
/// AsyncFixer02 warns: "Long-running or blocking operations inside an async method"
/// 
/// WHY SUPPRESSION IS NECESSARY FOR CANCELLATION TESTS:
/// • CancellationTokenSource.Cancel() is NOT a long-running operation (executes in microseconds)
/// • Deterministic cancellation is REQUIRED for batch processing cancellation tests
/// • Alternative approaches (Task.Delay