#pragma warning disable SA1402 // File may only contain a single type
namespace Kritikos.LdapDslExpression
{
  using Kritikos.LdapDslExpression.Expression;
  using Kritikos.SourceGenerator.RecordBehavior;

  [RecordBehavior]
  public partial class Person<T>
  {
    public T Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public int Age { get; init; }

    public string LastName { get; set; } = string.Empty;
  }

  [RecordBehavior]
  public partial class Pet
  {
    public string Name { get; init; }
  }

  [RecordBehavior]
  public partial class LdapExpressionBool : ILdapExpression
  {
  }

  [RecordBehavior]
  public abstract partial class LdapExpressionEqual<TExpression> : LdapExpressionBool, ILdapExpression
  {
    private TExpression Left { get; init; }

    private TExpression Right { get; init; }
  }

  [RecordBehavior]
  public abstract partial class LdapExpression<TExpressionArgument, TExpressionResult> : ILdapExpression
    where TExpressionArgument : ILdapExpression
    where TExpressionResult : ILdapExpression
  {
    public TExpressionArgument Left { get; init; }

    public TExpressionArgument Right { get; init; }
  }

  public record Customer<T>(T Id, string Name);
}
