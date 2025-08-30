// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

namespace MarkdownToRazor.Components;

/// <summary>
/// Options for MarkdownSectionPreCodeRenderer
/// </summary>
internal class MarkdownSectionPreCodeRendererOptions
{
    /// <summary>
    /// html attributes for Tag element in markdig generic attributes format
    /// </summary>
    public string? PreTagAttributes;
    /// <summary>
    /// html attributes for Code element in markdig generic attributes format
    /// </summary>
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
    public string? CodeTagAttributes;
#pragma warning restore CS0649
}
