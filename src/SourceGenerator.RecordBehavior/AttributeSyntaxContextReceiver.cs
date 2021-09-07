namespace Kritikos.SourceGenerator.RecordBehavior
{
  using System.Collections.Generic;
  using System.Linq;

  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  internal class AttributeSyntaxContextReceiver : ISyntaxContextReceiver
  {
    public AttributeSyntaxContextReceiver(string attributeName)
    {
      AttributeName = attributeName;
    }

    public string AttributeName { get; }

    public List<(ClassDeclarationSyntax Node, INamedTypeSymbol Symbol)> ClassTypes { get; }
      = new();

    public List<(RecordDeclarationSyntax Node, INamedTypeSymbol Symbol)> RecordTypes { get; }
      = new();

    public List<(StructDeclarationSyntax Node, INamedTypeSymbol Symbol)> StructTypes { get; }
      = new();

    public List<(SyntaxNode Node, INamedTypeSymbol Symbol)> OtherTypes { get; }
      = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
      var model = context.SemanticModel;
      var attribute = model.Compilation.GetTypeByMetadataName(AttributeName);

      var node = context.Node;
      if (model.GetDeclaredSymbol(node) is not INamedTypeSymbol symbol)
      {
        return;
      }

      var attributeList = symbol.GetAttributes()
        .Where(x => x.AttributeClass?.ToString() == AttributeName)
        .ToList();

      if (!attributeList.Any())
      {
        return;
      }

      if (node is ClassDeclarationSyntax cds)
      {
        ClassTypes.Add((cds, symbol));
      }
      else if (node is StructDeclarationSyntax sds)
      {
        StructTypes.Add((sds, symbol));
      }
      else if (node is RecordDeclarationSyntax rds)
      {
        RecordTypes.Add((rds, symbol));
      }
      else
      {
        OtherTypes.Add((node, symbol));
      }
    }
  }
}
