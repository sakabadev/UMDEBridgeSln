using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace UMDEBridge.Demo.SourceGenerator
{
	[Generator]
	public class MyCodeGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
		}

		public void Execute(GeneratorExecutionContext context)
		{
			var code = SourceText.From(@"
using System;

namespace MyCodeGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MyCustomAttribute : Attribute
    {
        public MyCustomAttribute() {}
    }
}", Encoding.UTF8);
			context.AddSource("MyCustomAttribute.g.cs", code);
		}
	}
}