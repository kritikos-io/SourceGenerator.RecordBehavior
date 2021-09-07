namespace Kritikos.SourceGenerator.RecordBehavior
{
  using System;
  using System.CodeDom.Compiler;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Globalization;
  using System.IO;
  using System.Text;

  public static class CodeWriter
  {
    public static string WriteAttribute()
    {
      using var stringwriter = new StringWriter(CultureInfo.InvariantCulture);
      using var indented =
        new IndentedTextWriter(stringwriter);

      indented.WriteLine("namespace Kritikos.SourceGenerator.RecordBehavior");

      indented.WriteLine("{");
      indented.Indent++;

      indented.WriteLine("using System;");
      indented.WriteLine("using System.CodeDom.Compiler;");
      indented.WriteLine();

      indented.WriteLine();
      indented.WriteLine(
        $@"[GeneratedCode(""{RecordBehaviorSourceGenerator.Name}"", ""{RecordBehaviorSourceGenerator.Version}"")]");
      indented.WriteLine(
        "[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]");

      indented.WriteLine("public class RecordBehaviorAttribute : Attribute");
      indented.WriteLine("{");
      indented.Indent++;

      indented.Indent--;
      indented.WriteLine("}");

      indented.Indent--;
      indented.WriteLine("}");

      return stringwriter.ToString();
    }

    public static string WriteAttributeOptions()
    {
      using var stringwriter = new StringWriter(CultureInfo.InvariantCulture);
      using var indented =
        new IndentedTextWriter(stringwriter);

      indented.WriteLine("namespace Kritikos.SourceGenerator.RecordBehavior");
      indented.WriteLine("{");
      indented.Indent++;

      indented.WriteLine("using System;");

      indented.SkipLine();

      indented.WriteLine("[Flags]");
      indented.WriteLine("public enum RecordBehaviorOptions");
      indented.WriteLine("{");
      indented.Indent++;

      indented.WriteLine("None = 0,");
      indented.WriteLine("SkipDefaultConstructor = 1,");

      indented.Indent--;
      indented.WriteLine("}");

      indented.Indent--;
      indented.WriteLine("}");

      return stringwriter.ToString();
    }
  }
}
