namespace Kritikos.SourceGenerator.RecordBehavior
{
  using System;
  using System.CodeDom.Compiler;
#if DEBUG
  using System.Diagnostics;
#endif // DEBUG
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Text;

  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.Text;

  [Generator]
  internal class RecordBehaviorSourceGenerator : ISourceGenerator
  {
    internal static readonly string Name = nameof(RecordBehaviorSourceGenerator);

    internal static readonly string
      Version = typeof(RecordBehaviorSourceGenerator).Assembly.GetName().Version.ToString();

    private static readonly string[] Modifiers = { "public", "protected", "internal", "private" };

    public void Initialize(GeneratorInitializationContext context)
      => context.RegisterForSyntaxNotifications(() => new AttributeSyntaxContextReceiver("RecordBehavior"));

    public void Execute(GeneratorExecutionContext context)
    {
      if (context.SyntaxContextReceiver is not AttributeSyntaxContextReceiver receiver)
      {
        return;
      }

#if DEBUG
      var debug = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                    "build_property.Kritikos_SourceGenerator_Debug",
                    out var debugValue)
                  && bool.TryParse(debugValue, out var debugResult) && debugResult;

      if (debug && !Debugger.IsAttached)
      {
        Debugger.Launch();
      }
#endif // DEBUG

      context.AddSource(
        $"RecordBehaviorAttribute.{nameof(RecordBehaviorSourceGenerator)}.cs",
        SourceText.From(CodeWriter.WriteAttribute(), Encoding.UTF8));

      foreach (var (node, symbol) in receiver.ClassTypes)
      {
        if (symbol.IsStatic)
        {
          continue;
        }

        if (node.Modifiers.All(x => x.ValueText != "partial"))
        {
          continue;
        }

        var name = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        if (symbol.Interfaces.Any(x => x.Name == $"IEquatable<{name}>"))
        {
          continue;
        }

        var properties = symbol.GetMembers()
          .OfType<IPropertySymbol>()
          .Where(x => x.SetMethod == null || x.SetMethod.IsInitOnly)
          .Select(x => (PropertyName: x.Name,
            PropertyType: x.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)))
          .ToArray();
        if (!properties.Any())
        {
          continue;
        }

        using var stringwriter = new StringWriter(CultureInfo.InvariantCulture);
        using var indented =
          new IndentedTextWriter(stringwriter, node.SyntaxTree.GetIndention(context.AnalyzerConfigOptions));

        var constructors = symbol.Constructors.ToArray();
        var constructorName =
          name.Contains("<")
            ? name.Substring(0, name.IndexOf("<", StringComparison.InvariantCulture))
            : name;

        var members = symbol.GetMembers()
          .OfType<IMethodSymbol>()
          .ToArray();

        var visibilityModifier = node.Modifiers.FirstOrDefault(x => Modifiers.Contains(x.ValueText)).ValueText;
        var virtualText = symbol.IsAbstract || !symbol.IsSealed
          ? "virtual "
          : string.Empty;

        indented.WriteLine("// <auto-generated /> ");
        indented.WriteLine($"namespace {symbol.ContainingNamespace}");
        indented.WriteLine("{");
        indented.Indent = 1;
        indented.WriteLine("#pragma warning disable");
        indented.WriteLine("using System;");
        indented.WriteLine("using System.CodeDom.Compiler;");
        indented.WriteLine("using System.Runtime.CompilerServices;");
        indented.WriteLine("using System.Text;");
        indented.Indent = 0;
        indented.WriteLine();
        indented.Indent = 1;
        indented.WriteLine($@"[GeneratedCode(""{Name}"", ""{Version}"")]");
        indented.WriteLine($"{visibilityModifier} partial class {name} : IEquatable<{name}>");
        indented.WriteLine("{");
        indented.Indent = 2;

        #region DefaultConstructor

        if (!constructors.Any(x => x.Parameters.Length == 0 && !x.IsImplicitlyDeclared))
        {
          indented.WriteLine($"public {constructorName}()");
          indented.WriteLine("{");
          indented.WriteLine("}");

          indented.SkipLine();
        }

        #endregion DefaultConstructor

        #region CloningConstructor

        if (!constructors.Any(x => x.Parameters.Length == 1 && x.Parameters.First().Type.Name == name))
        {
          indented.WriteLine($"public {constructorName}({name} original)");
          indented.WriteLine(" : this()");
          indented.WriteLine("{");
          indented.Indent++;
          foreach (var (prop, _) in properties)
          {
            indented.WriteLine($"{prop} = original.{prop};");
          }

          indented.Indent--;
          indented.WriteLine("}");
        }

        #endregion CloningConstructor

        indented.SkipLine();

        #region PropertyConstructor

        indented.WriteLine(
          $"public {constructorName}({string.Join(", ", properties.Select(x => $"{x.PropertyType} {char.ToLowerInvariant(x.PropertyName[0]) + x.PropertyName.Substring(1)}"))})");
        indented.WriteLine(" : this()");
        indented.WriteLine("{");
        indented.Indent++;
        foreach (var (prop, _) in properties)
        {
          indented.WriteLine($"{prop} = {char.ToLowerInvariant(prop[0]) + prop.Substring(1)};");
        }

        indented.Indent--;
        indented.WriteLine("}");

        #endregion PropertyConstructor

        indented.SkipLine();

        #region IEquatable.Equals

        indented.WriteLine($"public {virtualText}bool Equals({name}? other)");
        indented.Indent++;
        indented.Write("=> ((other != null) || ReferenceEquals(this, other))");

        if (properties.Length > 0)
        {
          indented.Indent++;
          indented.Write(
            $@" && {string.Join(" && ", properties.Select(x => $"{x.PropertyName}.Equals(other.{x.PropertyName})"))}");
          indented.Indent--;
        }

        indented.Indent--;

        indented.WriteLine(";");

        #endregion IEquatable.Equals

        indented.SkipLine();

        #region Equals

        indented.WriteLine($"public override bool Equals(object? obj)");
        indented.Indent++;
        indented.WriteLine(
          $"=> obj is not null && (ReferenceEquals(this, obj) || (obj is {name} item && Equals(item)));");
        indented.Indent--;

        #endregion Equals

        indented.SkipLine();

        #region GetHashCode

        var genericPartitions = properties
          .Select((data, index) => new { data, index })
          .GroupBy(group => (int)(group.index / 8))
          .Select(group => group.Select(tuple => tuple.data))
          .ToArray();

        indented.WriteLine("public override int GetHashCode()");
        indented.Indent++;
        indented.Write("=> ");
        var hashes = genericPartitions
          .Select(x => $"HashCode.Combine({string.Join(", ", x.Select(y => y.PropertyName))})")
          .ToArray();
        var combined = hashes.Length > 1
          ? $"HashCode.Combine({string.Join(", ", hashes)})"
          : hashes[0];
        indented.Write(combined);

        indented.WriteLine(";");

        indented.Indent--;

        #endregion GetHashCode

        indented.SkipLine();

        #region ToString

        indented.WriteLine("public override string ToString()");
        indented.WriteLine("{");
        indented.Indent++;

        indented.WriteLine("StringBuilder stringBuilder = new StringBuilder();");
        indented.WriteLine(@$"stringBuilder.Append(""{constructorName}"");");
        indented.WriteLine(@"stringBuilder.Append("" { "");");

        indented.WriteLine("if (PrintMembers(stringBuilder))");
        indented.WriteLine(@"{");
        indented.Indent++;
        indented.WriteLine(@"stringBuilder.Append(' ');");
        indented.Indent--;
        indented.WriteLine("}\n");
        indented.WriteLine(@"stringBuilder.Append("" } "");");
        indented.WriteLine("return stringBuilder.ToString();");

        indented.Indent--;
        indented.WriteLine("}");

        #endregion ToString

        indented.SkipLine();

        #region Deconstruct

        var parameters = string.Join(", ", properties.Select(x => $"out {x.PropertyType} {x.PropertyName}"));

        // indented.WriteLine("#pragma warning disable SA1313 // Parameter names should begin with lower-case letter");
        indented.WriteLine($"public void Deconstruct({parameters})");
        indented.WriteLine("{");
        indented.Indent++;
        foreach (var (prop, _) in properties)
        {
          indented.WriteLine($"{prop} = this.{prop};");
        }

        indented.Indent--;
        indented.WriteLine("}");

        // indented.WriteLine("#pragma warning restore SA1313 // Parameter names should begin with lower-case letter");

        #endregion Deconstruct

        indented.SkipLine();

        #region PrintMembers

        indented.WriteLine($"protected {virtualText}bool PrintMembers(StringBuilder builder)");
        indented.WriteLine("{");
        indented.Indent++;

        indented.WriteLine("RuntimeHelpers.EnsureSufficientExecutionStack();");
        for (var i = 0; i < properties.Length; i++)
        {
          indented.WriteLine(i == 0
            ? $@"builder.Append(""{properties[i].PropertyName} = "");"
            : $@"builder.Append("", {properties[i].PropertyName} = "");");

          indented.WriteLine($@"builder.AppendLine({properties[i].PropertyName}.ToString());");
        }

        indented.WriteLine("return true;");
        indented.Indent--;
        indented.WriteLine("}");

        #endregion PrintMembers

        indented.Indent = 1;
        indented.WriteLine("}");

        indented.WriteLine("#pragma warning enable");
        indented.Indent = 0;
        indented.WriteLine("}");

        context.AddSource(
          $"{symbol.Name}.{nameof(RecordBehaviorSourceGenerator)}.cs",
          SourceText.From(stringwriter.ToString(), Encoding.UTF8));
      }
    }
  }
}
