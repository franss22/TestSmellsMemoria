﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TestSmells.DuplicateAssert
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DuplicateAssertAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DuplicateAssert";


        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Test Smells";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private static readonly string[] RelevantAssertionsNames = {
            //Assert
            "AreEqual",
            "AreNotEqual",
            "AreNotSame",
            "AreSame",
            "IsFalse",
            "IsInstanceOfType",
            "IsNotInstanceOfType",
            "IsNotNull",
            "IsNull",
            "IsTrue",
            "ThrowsException",
            "ThrowsExceptionAsync",
            "Fail",
            "Inconclusive",
            //CollectionAssert
            "AllItemsAreInstancesOfType",
            "AllItemsAreNotNull",
            "AllItemsAreUnique",
            "AreEqual",
            "AreEquivalent",
            "AreNotEqual",
            "AreNotEquivalent",
            "Contains",
            "DoesNotContain",
            "IsNotSubsetOf",
            "IsSubsetOf",
            //StringAssert
            "Contains",//edge (String, String)
            "DoesNotMatch",
            "EndsWith",//edge (String, String)
            "Matches",
            "StartsWith",//edge (String, String)

        };


        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(GetClassesFromCompilation);
        }

        private void GetClassesFromCompilation(CompilationStartAnalysisContext context)
        {
            // Get the attribute object from the compilation
            var testClassAttr = context.Compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
            if (testClassAttr is null) { return; }
            var testMethodAttr = context.Compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute");
            if (testMethodAttr is null) { return; }

            var relevantAssertions = TestUtils.GetAssertionMethodSymbols(context.Compilation);
            if (relevantAssertions.Length == 0) return;


            var analyzeMethod = AnalyzeMethodSymbol(testClassAttr, testMethodAttr, relevantAssertions);

            context.RegisterSymbolStartAction(analyzeMethod, SymbolKind.Method);

        }

        private static Action<SymbolStartAnalysisContext> AnalyzeMethodSymbol(INamedTypeSymbol testClassAttr, INamedTypeSymbol testMethodAttr, IMethodSymbol[] relevantAssertions)
        {
            return (SymbolStartAnalysisContext context) =>
            {
                if (!TestUtils.TestMethodInTestClass(context, testClassAttr, testMethodAttr)) { return; }

                var operationBlockAnalisis = AnalyzeMethodOperations(relevantAssertions);
                context.RegisterOperationBlockAction(operationBlockAnalisis);

            };


        }

        private static Action<OperationBlockAnalysisContext> AnalyzeMethodOperations(IMethodSymbol[] relevantAssertions)

        {
            return (OperationBlockAnalysisContext context) =>
            {
                var assertions = new List<IInvocationOperation>();
                var blockOperation = TestUtils.GetBlockOperation(context);

                var descendants = blockOperation.Descendants();
                foreach (var operation in descendants)
                {
                    if (operation.Kind != OperationKind.Invocation) { continue; }
                    var invocationOperation = (IInvocationOperation)operation;
                    if (TestUtils.MethodIsInList(invocationOperation.TargetMethod, relevantAssertions))
                    {
                        assertions.Add(invocationOperation);
                    }
                }

                if (assertions.Count <= 1) { return; }


                var duplications = new List<List<IInvocationOperation>>
                {
                    new List<IInvocationOperation> { assertions.First() }
                };

                foreach (var assert in assertions.Skip(1))
                {
                    bool unique = true;
                    foreach (var similarOperations in duplications)
                    {
                        if (AreSimilarInvocations(assert, similarOperations.First()))
                        {
                            similarOperations.Add(assert);
                            unique = false;
                            break;
                        }
                    }
                    if (unique)
                    {
                        duplications.Add(new List<IInvocationOperation> { assert });
                    }
                }

                if (duplications.Count == assertions.Count) { return; }
                foreach (var assertionGroup in duplications)
                {
                    var testLocation = context.OwningSymbol.Locations.First();

                    var locations = new List<Location>(from o in assertionGroup select o.Syntax.GetLocation());
                    var diagnosticLocation = locations.First();
                    locations.Insert(0, testLocation);
                    var diagnostic = Diagnostic.Create(Rule, diagnosticLocation, locations, context.OwningSymbol.Name);
                    context.ReportDiagnostic(diagnostic);

                }


            };
        }

        private static bool AreSimilarInvocations(IInvocationOperation invocation1, IInvocationOperation invocation2)
        {
            return (invocation1.Syntax.IsEquivalentTo(invocation2.Syntax, true));
        }

    }
}