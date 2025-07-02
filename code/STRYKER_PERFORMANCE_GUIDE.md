# Stryker Mutation Testing - High Performance Configuration Guide

## 🚀 Performance-Optimized Configuration for ExxerAI

This configuration is specifically tuned for maximum performance on your life project - from Windows Forms to .NET 10! Built for projects with comprehensive test suites ready for mutation hunting.

## 🎯 Key Performance Optimizations

### **Concurrency & Coverage**
- `concurrency: 6` - Optimized for multi-core systems
- `coverageAnalysis: "perTest"` - **CRITICAL**: Only runs tests that cover each mutant
- `ignoreStatic: true` - Skips static mutants that cause performance penalties

### **Smart Target Selection**
- Targets **only production code** in all 8 main projects
- Excludes test files, generated files, build artifacts
- Ignores heavy documentation and side projects

### **Incremental Runs**
- `incremental: true` - Remembers previous results
- Only re-tests changed code on subsequent runs
- Use `--force` to rebuild incremental cache when needed

### **Timeout Optimization**
- `timeoutMS: 10000` - Generous timeout for complex operations
- `timeoutFactor: 2.0` - Allows 2x normal execution time
- `dryRunTimeoutMinutes: 10` - Sufficient for large codebases

## 🎮 Usage Commands

### **Full Mutation Test Run**
```bash
# First run (builds incremental cache)
dotnet stryker

# Subsequent runs (uses incremental data)
dotnet stryker
```

### **Force Complete Re-run**
```bash
# Rebuilds everything from scratch
dotnet stryker --force
```

### **Target Specific Projects**
```bash
# Test only Domain layer
dotnet stryker --mutate "src/ExxerAI.Domain/**/*.cs"

# Test Application + Domain
dotnet stryker --mutate "src/ExxerAI.Application/**/*.cs,src/ExxerAI.Domain/**/*.cs"
```

### **Development Testing**
```bash
# Dry run only (validates setup)
dotnet stryker --dry-run-only

# Higher concurrency for powerful machines
dotnet stryker --concurrency 8

# Lower concurrency for resource-constrained environments
dotnet stryker --concurrency 2
```

## 📊 Mutation Score Thresholds

| Score Range | Status | Action Required |
|-------------|--------|-----------------|
| 85%+ | 🟢 **Excellent** | Maintain quality |
| 70-84% | 🟡 **Warning** | Review untested paths |
| 60-69% | 🔴 **Danger** | Improve test coverage |
| <60% | 💥 **Build Fails** | Critical coverage gaps |

## 🎯 Excluded Mutations (Performance Focused)

To maximize performance, these mutation types are excluded:
- `StringLiteral` - Low-value string mutations
- `InterpolatedString` - String interpolation changes
- `RegexLiteral` - Regex pattern mutations
- `NullCoalescing` - Null coalescing operator changes

## 📈 Expected Performance

### **Without Optimization**
- Runtime: ~2-4 hours for 762 tests
- Memory: High memory usage
- All tests run for all mutants

### **With This Configuration**
- Runtime: ~30-60 minutes for 762 tests
- Memory: Optimized usage
- Only relevant tests run per mutant
- Incremental runs: ~5-15 minutes

## 🔧 Troubleshooting

### **Slow Performance**
```bash
# Check what's being mutated
dotnet stryker --dry-run-only --verbosity trace

# Reduce concurrency if memory-bound
dotnet stryker --concurrency 2

# Use more aggressive ignoring
# Edit stryker-config.json -> ignorePatterns
```

### **High Memory Usage**
```bash
# Restart test runners more frequently
# Edit stryker-config.json -> "maxTestRunnerReuse": 5

# Or disable runner reuse entirely
# Edit stryker-config.json -> "maxTestRunnerReuse": 1
```

### **Timeout Issues**
```bash
# Increase timeouts
dotnet stryker --timeout-ms 20000 --timeout-factor 3

# Or increase dry run timeout
dotnet stryker --dry-run-timeout-minutes 15
```

## 📁 Generated Reports

All reports are saved to `StrykerOutput/reports/`:
- **HTML Report**: `mutation.html` - Interactive browser report
- **JSON Report**: `mutation.json` - Machine-readable results
- **Incremental Cache**: `stryker-incremental.json` - Speed optimization data

## 🚀 CI/CD Integration

### **GitHub Actions Example**
```yaml
- name: Run Mutation Tests
  run: |
    dotnet tool install -g dotnet-stryker
    dotnet stryker --verbosity info
  env:
    STRYKER_DASHBOARD_API_KEY: ${{ secrets.STRYKER_API_KEY }}
```

### **Azure DevOps Example**
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Install Stryker'
  inputs:
    command: 'custom'
    custom: 'tool'
    arguments: 'install -g dotnet-stryker'

- task: DotNetCoreCLI@2
  displayName: 'Run Mutation Tests'
  inputs:
    command: 'custom'
    custom: 'stryker'
    arguments: '--verbosity info'
```

## 💡 Pro Tips for Your Life Project

1. **Start with Domain Layer**: Highest ROI for mutation testing
2. **Use Incremental Mode**: Essential for large codebases like yours
3. **Monitor Slow Warnings**: Stryker will suggest optimizations
4. **Regular Runs**: Run nightly to catch regression in mutation scores
5. **Domain Behavior Focus**: Perfect timing as you move from anemic to rich domain

## 🎯 Next Steps for Domain Evolution

As you transition from anemic domain to rich domain behavior:

1. **Baseline Current Score**: Run full mutation test to establish baseline
2. **Focus on Domain Tests**: Ensure domain logic has high mutation coverage
3. **Iterative Improvement**: Use mutation results to guide where to add behavior
4. **Regression Prevention**: Keep mutation score high as you refactor

---

**Remember**: This configuration is optimized for your specific project structure. Adjust concurrency and timeouts based on your machine's capabilities and CI environment.

## 📞 Stryker Dashboard

Configure your project on [Stryker Dashboard](https://dashboard.stryker-mutator.io) to track mutation scores over time and compare across branches.

---

*Built for a project that's evolved from VBA to .NET 10 - optimized for a lifetime of continuous improvement!* 🎯 