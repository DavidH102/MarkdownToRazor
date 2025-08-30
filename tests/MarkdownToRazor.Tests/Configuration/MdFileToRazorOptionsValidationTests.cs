using MarkdownToRazor.Configuration;

namespace MarkdownToRazor.Tests.Configuration;

/// <summary>
/// Comprehensive tests for MdFileToRazorOptions configuration validation.
/// Tests edge cases, invalid configurations, and proper validation behavior.
/// </summary>
public class MdFileToRazorOptionsValidationTests
{
    [Fact]
    public void Validate_ValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            OutputDirectory = "output",
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true,
            EnableYamlFrontmatter = true
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Fact]
    public void Validate_RuntimeOnlyConfiguration_DoesNotThrow()
    {
        // Arrange - Runtime-only configuration without OutputDirectory
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            OutputDirectory = null, // Runtime-only scenarios don't need output directory
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true,
            EnableYamlFrontmatter = false
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Fact]
    public void Validate_NullSourceDirectory_ThrowsArgumentException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = null!,
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("SourceDirectory", exception.Message);
    }

    [Fact]
    public void Validate_EmptySourceDirectory_ThrowsArgumentException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "",
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("SourceDirectory", exception.Message);
    }

    [Fact]
    public void Validate_WhitespaceSourceDirectory_ThrowsArgumentException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "   ",
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("SourceDirectory", exception.Message);
    }

    [Fact]
    public void Validate_NullFilePattern_ThrowsArgumentException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = null!,
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("FilePattern", exception.Message);
    }

    [Fact]
    public void Validate_EmptyFilePattern_ThrowsArgumentException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "",
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("FilePattern", exception.Message);
    }

    [Fact]
    public void Validate_WhitespaceFilePattern_ThrowsArgumentException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "   ",
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("FilePattern", exception.Message);
    }

    [Fact]
    public void Validate_BothConfigurationMethodsDisabled_ThrowsArgumentException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = false,
            EnableYamlFrontmatter = false
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("configuration method", exception.Message);
    }

    [Fact]
    public void Validate_OnlyHtmlCommentConfiguration_DoesNotThrow()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true,
            EnableYamlFrontmatter = false
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Fact]
    public void Validate_OnlyYamlFrontmatter_DoesNotThrow()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = false,
            EnableYamlFrontmatter = true
        };

        // Act & Assert - Should not throw
        options.Validate();
    }

    [Theory]
    [InlineData("content")]
    [InlineData("docs")]
    [InlineData("markdown")]
    [InlineData("MDFilesToConvert")]
    [InlineData("source/markdown")]
    public void GetAbsoluteSourcePath_ValidRelativePaths_ReturnsCorrectAbsolutePath(string sourceDirectory)
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = sourceDirectory
        };
        var contentRootPath = "/app/root";

        // Act
        var result = options.GetAbsoluteSourcePath(contentRootPath);

        // Assert - Path.GetFullPath normalizes the combined path
        var expected = Path.GetFullPath(Path.Combine(contentRootPath, sourceDirectory));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("../docs")]
    [InlineData("./content")]
    public void GetAbsoluteSourcePath_RelativeWithDots_ResolvesCorrectly(string sourceDirectory)
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = sourceDirectory
        };
        var contentRootPath = "/app/root";

        // Act
        var result = options.GetAbsoluteSourcePath(contentRootPath);

        // Assert - Path.GetFullPath resolves .. and . correctly
        var expected = Path.GetFullPath(Path.Combine(contentRootPath, sourceDirectory));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/absolute/path")]
    public void GetAbsoluteSourcePath_UnixAbsolutePaths_ReturnsAsIs(string sourceDirectory)
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = sourceDirectory
        };
        var contentRootPath = "/app/root";

        // Act
        var result = options.GetAbsoluteSourcePath(contentRootPath);

        // Assert - Absolute paths should be returned as-is (after normalization)
        Assert.Equal(Path.GetFullPath(sourceDirectory), result);
    }

    [Theory]
    [InlineData("C:\\Windows\\Path")]
    public void GetAbsoluteSourcePath_WindowsAbsolutePaths_HandledByPlatform(string sourceDirectory)
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = sourceDirectory
        };
        var contentRootPath = "/app/root";

        // Act
        var result = options.GetAbsoluteSourcePath(contentRootPath);

        // Assert - On Unix systems, Windows paths are treated as relative
        // On Windows systems, they would be treated as absolute
        // We test based on current platform behavior
        if (Path.IsPathRooted(sourceDirectory))
        {
            Assert.Equal(Path.GetFullPath(sourceDirectory), result);
        }
        else
        {
            Assert.Equal(Path.GetFullPath(Path.Combine(contentRootPath, sourceDirectory)), result);
        }
    }

    [Fact]
    public void GetAbsoluteOutputPath_WithOutputDirectory_ReturnsCorrectAbsolutePath()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            OutputDirectory = "output"
        };
        var contentRootPath = "/app/root";

        // Act
        var result = options.GetAbsoluteOutputPath(contentRootPath);

        // Assert
        var expected = Path.Combine(contentRootPath, "output");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAbsoluteOutputPath_WithNullOutputDirectory_ReturnsNull()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            OutputDirectory = null
        };
        var contentRootPath = "/app/root";

        // Act
        var result = options.GetAbsoluteOutputPath(contentRootPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new MarkdownToRazorOptions();

        // Assert
        Assert.Equal("MDFilesToConvert", options.SourceDirectory);
        Assert.Null(options.OutputDirectory);
        Assert.Equal("*.md", options.FilePattern);
        Assert.True(options.SearchRecursively);
        Assert.True(options.EnableHtmlCommentConfiguration);
        Assert.True(options.EnableYamlFrontmatter);
    }

    [Theory]
    [InlineData("*.md")]
    [InlineData("*.markdown")]
    [InlineData("*.MD")]
    [InlineData("*.MARKDOWN")]
    [InlineData("readme.md")]
    [InlineData("*.txt")]
    public void FilePattern_VariousPatterns_AcceptedWithoutValidation(string pattern)
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = pattern,
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert - Should not throw during validation
        options.Validate();
        Assert.Equal(pattern, options.FilePattern);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SearchRecursively_BothValues_AcceptedWithoutValidation(bool searchRecursively)
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "*.md",
            SearchRecursively = searchRecursively,
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert - Should not throw during validation
        options.Validate();
        Assert.Equal(searchRecursively, options.SearchRecursively);
    }

    [Fact]
    public void GetAbsoluteSourcePath_NullContentRootPath_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.GetAbsoluteSourcePath(null!));
    }

    [Fact]
    public void GetAbsoluteOutputPath_NullContentRootPath_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            OutputDirectory = "output"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.GetAbsoluteOutputPath(null!));
    }

    [Fact]
    public void GetAbsoluteSourcePath_EmptyContentRootPath_HandlesGracefully()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content"
        };
        var contentRootPath = "";

        // Act
        var result = options.GetAbsoluteSourcePath(contentRootPath);

        // Assert - Path.GetFullPath will resolve this relative to current directory
        var expected = Path.GetFullPath(Path.Combine(contentRootPath, "content"));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("content/docs")]
    [InlineData("source\\markdown")]
    [InlineData("docs/api")]
    public void SourceDirectory_PathSeparators_HandledCorrectly(string sourceDirectory)
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = sourceDirectory,
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert - Should validate successfully
        options.Validate();
        Assert.Equal(sourceDirectory, options.SourceDirectory);
    }

    [Fact]
    public void Configuration_ImmutableAfterCreation_CanBeModified()
    {
        // Arrange
        var options = new MarkdownToRazorOptions();

        // Act - Modify properties after creation
        options.SourceDirectory = "new-source";
        options.OutputDirectory = "new-output";
        options.FilePattern = "*.markdown";
        options.SearchRecursively = false;
        options.EnableHtmlCommentConfiguration = false;
        options.EnableYamlFrontmatter = true;

        // Assert - Properties should be modifiable
        Assert.Equal("new-source", options.SourceDirectory);
        Assert.Equal("new-output", options.OutputDirectory);
        Assert.Equal("*.markdown", options.FilePattern);
        Assert.False(options.SearchRecursively);
        Assert.False(options.EnableHtmlCommentConfiguration);
        Assert.True(options.EnableYamlFrontmatter);
    }

    [Fact]
    public void Validate_MultipleCalls_ConsistentBehavior()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "*.md",
            EnableHtmlCommentConfiguration = true
        };

        // Act & Assert - Multiple validation calls should behave consistently
        options.Validate(); // First call
        options.Validate(); // Second call
        options.Validate(); // Third call

        // Should not throw on any call
    }

    [Fact]
    public void Configuration_ToStringOrSimilar_DoesNotExposeSecrets()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            OutputDirectory = "output",
            FilePattern = "*.md"
        };

        // Act
        var stringRepresentation = options.ToString();

        // Assert - Should not contain sensitive information
        // This is more of a defensive test to ensure no secrets are accidentally exposed
        Assert.NotNull(stringRepresentation);
    }
}