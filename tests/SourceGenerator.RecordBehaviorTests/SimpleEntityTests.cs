namespace SourceGenerator.RecordBehaviorTests
{
  using Kritikos.LdapDslExpression;

  using Xunit;

  public class SimpleEntityTests
  {
    [Fact]
    public void Test1()
    {
      var a = new Pet("Luna");
      var b = new Pet("Luna");

      Assert.Equal(a, b);
    }
  }
}
