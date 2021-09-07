namespace Kritikos.SourceGenerator.RecordBehavior
{
  using System.Collections.Generic;
  using System.Linq;

  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  internal class AttributeSyntaxContextReceiver<TAttribute> : AttributeSyntaxContextReceiver, ISyntaxContextReceiver
    where TAttribute : System.Attribute
  {
    public AttributeSyntaxContextReceiver()
      : base(typeof(TAttribute)?.FullName ?? string.Empty)
    {
    }
  }
}
