// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace MemoryUsage.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestEmptyProgram()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestDetectionAndFix()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            static void Main(string[] args)
            {
                var mem = args.AsMemory();
                var memSlice = mem.Slice(1);

                var memSliceAndSpan = mem.Slice(1).Span;
                memSliceAndSpan = mem.Slice(1, 1).Span;

                var memSliceAndSpanPrefix = mem.Slice(1).Span.ToArray();
                memSliceAndSpanPrefix = mem.Slice(1, 1).Span.ToArray();

                var memSliceAndSpanInMiddle = args.AsMemory().Slice(1).Span.ToArray();
                memSliceAndSpanInMiddle = args.AsMemory().Slice(1, 1).Span.ToArray();

                var multipleSliceAndSpan = args.AsMemory().Slice(1).Span.ToArray().AsMemory().Slice(1, 1).Span;
            }
        }
    }";
            var defaultDiagnosticResult = new DiagnosticResult
            {
                Id = "MemoryUsage",
                Message = "Can be re-ordered and written as Span.Slice()",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 0, 0) }
            };

            var expected = new DiagnosticResult[8]
            {
                defaultDiagnosticResult,
                defaultDiagnosticResult,
                defaultDiagnosticResult,
                defaultDiagnosticResult,
                defaultDiagnosticResult,
                defaultDiagnosticResult,
                defaultDiagnosticResult,
                defaultDiagnosticResult
            };
            expected[0].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 13, 39) };
            expected[1].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 35) };
            expected[2].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 45) };
            expected[3].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 17, 41) };
            expected[4].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 19, 47) };
            expected[5].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 20, 43) };
            expected[6].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 44) };
            expected[7].Locations = new[] { new DiagnosticResultLocation("Test0.cs", 22, 44) };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            static void Main(string[] args)
            {
                var mem = args.AsMemory();
                var memSlice = mem.Slice(1);

                var memSliceAndSpan = mem.Span.Slice(1);
                memSliceAndSpan = mem.Span.Slice(1, 1);

                var memSliceAndSpanPrefix = mem.Span.Slice(1).ToArray();
                memSliceAndSpanPrefix = mem.Span.Slice(1, 1).ToArray();

                var memSliceAndSpanInMiddle = args.AsMemory().Span.Slice(1).ToArray();
                memSliceAndSpanInMiddle = args.AsMemory().Span.Slice(1, 1).ToArray();

                var multipleSliceAndSpan = args.AsMemory().Span.Slice(1).ToArray().AsMemory().Span.Slice(1, 1);
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestDetectionAndFixNothingChanges()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            static void Main(string[] args)
            {
                var mem = args.AsMemory();
                var memSlice = mem.Slice(1);

                var memSliceAndSpan = mem.Span.Slice(1);
                memSliceAndSpan = mem.Span.Slice(1, 1);

                var memSliceAndSpanPrefix = mem.Span.Slice(1).ToArray();
                memSliceAndSpanPrefix = mem.Span.Slice(1, 1).ToArray();

                var memSliceAndSpanInMiddle = args.AsMemory().Span.Slice(1).ToArray();
                memSliceAndSpanInMiddle = args.AsMemory().Span.Slice(1, 1).ToArray();

                var multipleSliceAndSpan = args.AsMemory().Span.Slice(1).ToArray().AsMemory().Span.Slice(1, 1);
            }
        }
    }";
            var expected = new DiagnosticResult[0] { };
            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            static void Main(string[] args)
            {
                var mem = args.AsMemory();
                var memSlice = mem.Slice(1);

                var memSliceAndSpan = mem.Span.Slice(1);
                memSliceAndSpan = mem.Span.Slice(1, 1);

                var memSliceAndSpanPrefix = mem.Span.Slice(1).ToArray();
                memSliceAndSpanPrefix = mem.Span.Slice(1, 1).ToArray();

                var memSliceAndSpanInMiddle = args.AsMemory().Span.Slice(1).ToArray();
                memSliceAndSpanInMiddle = args.AsMemory().Span.Slice(1, 1).ToArray();

                var multipleSliceAndSpan = args.AsMemory().Span.Slice(1).ToArray().AsMemory().Span.Slice(1, 1);
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MemoryUsageCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MemoryUsageAnalyzer();
        }
    }
}
