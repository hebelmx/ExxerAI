# 🎯 DOMAIN COVERAGE ANALYSIS & MUTATION KILLING STRATEGY

**Project:** ExxerAI.Domain  
**Current Mutation Score:** 33.07%  
**Uncovered Mutants:** 519 (46.89%)  
**Target:** 90%+ mutation score

---

## 📊 COVERAGE GAP ANALYSIS

### ❌ **COMPLETELY UNTESTED CLASSES** (High Priority)

| Class | Size | Estimated Mutants | Priority | Test Type Needed |
|-------|------|-------------------|----------|------------------|
| **Orchestration.cs** | 436 lines | ~150-200 | 🚨 **CRITICAL** | Entity + Behavior |
| **LLMIntegration.cs** | 362 lines | ~120-150 | 🚨 **CRITICAL** | Entity + Behavior |
| **ProcessingHistory.cs** | 539 lines | ~180-220 | 🚨 **CRITICAL** | Complex Behavior |
| **MCPTypes.cs** | 336 lines | ~100-130 | ⚠️ **HIGH** | Entity + Enum |
| **Document.cs** | 108 lines | ~30-40 | ⚠️ **HIGH** | Entity |
| **ExtractionResult.cs** | 163 lines | ~50-70 | ⚠️ **HIGH** | Entity + Behavior |
| **GroundTruthContext.cs** | 419 lines | ~140-170 | ⚠️ **HIGH** | Complex Behavior |
| **ExtractionSchema.cs** | 212 lines | ~70-90 | ⚠️ **HIGH** | Entity + Validation |
| **ValidationRule.cs** | 327 lines | ~110-140 | ⚠️ **HIGH** | Complex Behavior |
| **AgentStatus.cs** | 32 lines | ~10-15 | 🔸 **MEDIUM** | Enum + Conversion |

### ⚠️ **PARTIALLY TESTED CLASSES** (Medium Priority)

| Class | Current Coverage | Missing Areas | Estimated Gap |
|-------|------------------|---------------|---------------|
| **ResultExtensions.cs** | ~60% | Complex validation logic | ~30-40 mutants |
| **Agent.cs** | ~80% | Edge cases, validation | ~10-20 mutants |
| **AgentTask.cs** | ~75% | Business rules, calculated properties | ~20-30 mutants |

---

## 🎯 **TACTICAL MUTATION KILLING PLAN**

### **Phase 1: Critical Domain Entities (Week 1)**

#### 🔥 **Target 1: Orchestration.cs** (~150-200 mutants)
**Classes to Test:**
- `Workflow` - Core entity with status transitions
- `WorkflowDefinition` - Complex composition 
- `WorkflowStep` - Step logic and conditions
- `StepConditions` - Validation rules
- `WorkflowConfiguration` - Settings and validation
- `WorkflowExecution` - State management
- `StepExecution` - Execution logic

**Key Test Scenarios:**
```csharp
// Entity Tests
- Property validation (Required, StringLength)
- Default value initialization  
- Status transitions (Draft → Active → Running → Completed)
- DateTime handling (CreatedAt, StartedAt, CompletedAt)

// Business Logic Tests  
- Workflow step ordering and dependencies
- Execution condition evaluation
- Timeout and retry logic
- Error handling and failure states
- Configuration validation
```

#### 🔥 **Target 2: LLMIntegration.cs** (~120-150 mutants)
**Classes to Test:**
- `LanguageModel` - Model configuration
- `ModelCapabilities` - Feature flags and limits
- `ModelConfiguration` - Settings validation
- `Conversation` - Session management
- `ConversationMessage` - Message handling with roles
- `ConversationMetadata` - Metrics and tracking

**Key Test Scenarios:**
```csharp
// Entity Tests
- Model availability and configuration
- Capability flag combinations
- Token counting and limits
- API configuration validation

// Behavior Tests
- Conversation lifecycle management
- Message role transitions
- Timestamp handling (Timestamp alias)
- Cost calculation and token tracking
```

### **Phase 2: Document Processing Logic (Week 2)**

#### 🔥 **Target 3: ProcessingHistory.cs** (~180-220 mutants)
**Classes to Test:**
- `ProcessingHistory` - Main aggregation logic
- `LearnedPattern` - Pattern learning and success tracking
- `ProcessingMetrics` - Calculation and analysis
- `SchemaEvolution` - Version tracking
- `AdaptationRecommendation` - AI recommendation logic

**Key Test Scenarios:**
```csharp
// Complex Business Logic
- AddProcessingResult() - Metrics updates
- AnalyzeAndGenerateRecommendations() - AI analysis
- GetResultsByConfidence() - Filtering logic
- GetCommonPatternsForField() - Pattern analysis
- UpdateMetrics() - Calculation correctness
- CalculateConfidenceDistribution() - Statistics

// Edge Cases
- Empty collections handling
- Null parameter validation
- Success rate calculations (divide by zero)
- Confidence thresholds and ranges
```

#### 🔥 **Target 4: ValidationRule.cs** (~110-140 mutants)
- Rule execution logic
- Validation chains
- Error collection and reporting
- Custom validation implementation

### **Phase 3: Supporting Classes (Week 3)**

#### 🔸 **Remaining DocumentProcessing Classes**
- `MCPTypes.cs` - Type definitions and conversions
- `Document.cs` - Core document entity  
- `ExtractionResult.cs` - Result aggregation
- `GroundTruthContext.cs` - Context management
- `ExtractionSchema.cs` - Schema validation

#### 🔸 **Enum and Small Classes** 
- `AgentStatus.cs` - Enum behaviors and conversions

---

## 📋 **IMPLEMENTATION CHECKLIST**

### **Per-Class Test Requirements:**

#### ✅ **Entity Tests (POCO Classes)**
```csharp
[Fact] 
public void PropertyName_Should_SetAndGetCorrectly()
[Fact]
public void Constructor_Should_InitializeWithDefaults()  
[Theory, InlineData(...)]
public void Validation_Should_FailForInvalidInput()
```

#### ✅ **Behavior Tests (Logic Classes)**  
```csharp
[Fact]
public void Method_Should_ReturnExpectedResult_When_ValidInput()
[Fact] 
public void Method_Should_HandleNullInput_Gracefully()
[Theory, InlineData(...)]
public void Method_Should_HandleEdgeCases()
```

#### ✅ **Business Logic Tests**
```csharp
[Fact]
public void BusinessRule_Should_EnforceConstraints()
[Fact]
public void CalculatedProperty_Should_ComputeCorrectly()
[Fact]
public void StateTransition_Should_BeValid()
```

#### ✅ **Collection/Aggregation Tests**
```csharp
[Fact]
public void AddItem_Should_UpdateCollection_And_RecalculateMetrics()
[Fact]
public void FilterMethod_Should_ReturnExpectedSubset()
[Fact]
public void EmptyCollection_Should_HandleGracefully()
```

---

## 📊 **SUCCESS METRICS**

### **Target Mutation Scores by Phase:**
- **Phase 1 Complete:** ~60% domain mutation score (+27%)
- **Phase 2 Complete:** ~80% domain mutation score (+20%) 
- **Phase 3 Complete:** ~90% domain mutation score (+10%)

### **Tracking Progress:**
```bash
# Run after each class completion
dotnet stryker --project ExxerAI.Domain --test-project ExxerAI.Domain.Tests --reporter progress

# Target metrics per class:
- 0 compilation errors
- 90%+ line coverage  
- 85%+ mutation score
- All business rules tested
```

---

## 🚀 **IMMEDIATE NEXT STEPS**

1. ✅ **Start with Orchestration.cs** - Biggest impact (~200 mutants)
2. ✅ **Create comprehensive WorkflowTests.cs** 
3. ✅ **Run incremental Stryker validation**
4. ✅ **Move to LLMIntegration.cs** 
5. ✅ **Continue systematic progression**

**Expected Outcome:** Transform **33.07% → 90%+ mutation score** through systematic domain coverage expansion. 