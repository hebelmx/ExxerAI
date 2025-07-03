using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace IndTrace.Generators;

[Generator]
public class EnumLookupIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.BaseList != null,
                transform: static (ctx, _) =>
                {
                    var classSyntax = (ClassDeclarationSyntax)ctx.Node;
                    var model = ctx.SemanticModel;
                    var symbol = model.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
                    return symbol;
                })
            .Where(symbol => symbol?.BaseType?.ToDisplayString() == "IndTrace.Domain.Enum.EnumModel");

        context.RegisterSourceOutput(classDeclarations, (ctx, symbol) =>
        {
            var source = GenerateSource(symbol);
            ctx.AddSource($"{symbol.Name}_Lookup.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private static string GenerateSource(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace.ToDisplayString();
        var className = symbol.Name;
        var lookupClass = $"{className}_LookupProvider";

        var sb = new StringBuilder($@"
using System.Collections.Generic;
using IndTrace.Domain.Enum;

namespace {ns}
{{
    public static class {lookupClass}
    {{
        public static List<EnumLookUpTable> ToLookup() => new()
        {{
");

        foreach (var member in symbol.GetMembers().OfType<IFieldSymbol>().Where(f => f.IsStatic && SymbolEqualityComparer.Default.Equals(f.Type, symbol)))
        {
            if (member.Name == "Invalid") continue;

            sb.AppendLine($"            new EnumLookUpTable({className}.{member.Name}.Value, \"{member.Name}\", {className}.{member.Name}.DisplayName),");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}