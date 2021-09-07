namespace Kritikos.SourceGenerator.RecordBehavior
{
  using System;
  using System.CodeDom.Compiler;

  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.Diagnostics;

  internal static class RecordBehaviorExtensions
  {
    internal static string GetIndention(this SyntaxTree tree, AnalyzerConfigOptionsProvider optionsProvider)
    {
      var options = optionsProvider.GetOptions(tree);
      if (!options.TryGetValue("indent_style", out var indentStyle))
      {
        return "\t";
      }

      if (string.Equals(indentStyle, "tab", StringComparison.OrdinalIgnoreCase))
      {
        return "\t";
      }

      if (string.Equals(indentStyle, "space", StringComparison.OrdinalIgnoreCase))
      {
        var size = options.TryGetValue("indent_size", out var indentSize)
          ? (uint.TryParse(indentSize, out var indentSizeValue)
            ? indentSizeValue
            : 3u)
          : 3u;
        return new string(' ', (int)size);
      }

      return "\t";
    }

    internal static void SkipLine(this IndentedTextWriter writer)
    {
      var indent = writer.Indent;
      writer.Indent = 0;
      writer.WriteLine();
      writer.Indent = indent;
    }
  }
}
