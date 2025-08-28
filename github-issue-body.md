## Problem Statement

Our current test coverage is insufficient for the WASM service-only architecture that we've implemented. While we have basic functional tests for `MdFileDiscoveryService`, we need comprehensive testing across all components and scenarios to ensure reliability and maintainability.

## Current Test Coverage Gaps

### 1. **Component Integration Testing**
- No tests for `MarkdownSection` component lifecycle
- Missing tests for `MarkdownFileExplorer` component functionality  
- No validation of component disposal and memory management

### 2. **Service Layer Testing**
- Limited error handling scenarios for `StaticAssetService`
- Missing edge cases for file loading from different sources (filesystem vs URLs)
- No performance testing for large markdown file sets
- Insufficient WASM-specific service behavior validation

### 3. **Configuration Testing**
- Missing tests for `MdFileToRazorOptions` validation
- No tests for MSBuild target integration
- Configuration edge cases not covered

### 4. **Build-Time vs Runtime Testing**
- No tests comparing build-time generation vs runtime rendering
- Missing validation that both approaches produce equivalent results
- No tests for migration scenarios (build-time → runtime)

## Example of Missing Test Coverage

Here's an example of a critical test case we're missing:

```csharp
[Fact]
public async Task StaticAssetService_LoadFromUrl_HandlesNetworkFailures()
{
    // Arrange
    var service = new StaticAssetService();
    var invalidUrl = "https://invalid-domain-that-does-not-exist.com/file.md";
    
    // Act & Assert
    await Assert.ThrowsAsync<HttpRequestException>(
        () => service.LoadContentAsync(invalidUrl));
}

[Fact]
public async Task MarkdownSection_LargeContent_RendersWithoutMemoryLeaks()
{
    // This test is completely missing from our current suite
    // Should validate performance with large markdown files
}
```

## Suggested Test Categories to Add

### **High Priority Tests:**

1. **Error Handling & Edge Cases**
   - Network failures for URL-based content loading
   - Invalid markdown syntax handling
   - Missing file scenarios
   - Permission denied scenarios
   - Large file performance testing

2. **WASM-Specific Testing**
   - Service behavior without `IHostEnvironment`
   - Browser-specific file loading limitations
   - Client-side routing integration
   - Memory usage in WASM context

3. **Cross-Platform Testing**
   - Path separator handling (Windows vs Linux)
   - Case sensitivity scenarios
   - Different file encodings

### **Medium Priority Tests:**

4. **Performance & Scalability**
   - Large directory scanning performance
   - Memory usage with many markdown files
   - Concurrent file access scenarios
   - Caching effectiveness validation

5. **Integration Testing**
   - End-to-end service registration and usage
   - MSBuild integration testing
   - NuGet package consumption scenarios

6. **Security Testing**
   - Path traversal prevention
   - Input sanitization for markdown content
   - URL validation for external content

### **Lower Priority Tests:**

7. **UI/UX Testing**
   - Component accessibility testing
   - Responsive design validation
   - Browser compatibility testing

8. **Documentation Testing**
   - Code examples in documentation are valid
   - API documentation completeness
   - Sample project functionality

## Success Criteria

- [ ] Achieve >90% code coverage across all service classes
- [ ] All error scenarios have corresponding tests
- [ ] WASM-specific behavior is thoroughly validated
- [ ] Performance benchmarks established for key operations
- [ ] Integration tests cover real-world usage patterns

## Implementation Approach

1. **Phase 1**: Critical service layer tests (error handling, WASM compatibility)
2. **Phase 2**: Component integration and lifecycle tests  
3. **Phase 3**: Performance and scalability testing
4. **Phase 4**: Cross-platform and security validation

## Related Context

This issue stems from recent work on WASM optimization where we discovered gaps in our testing strategy. The service-only architecture for WASM requires different testing approaches than the traditional component-based patterns.

**Current Functional Tests**: 8 tests in `MdFileDiscoveryServiceFunctionalTests`
**Estimated Needed Tests**: 40-60 comprehensive tests across all categories

## Labels
- enhancement
- testing
- priority-high
- wasm
- service-architecture
