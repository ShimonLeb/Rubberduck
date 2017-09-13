﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Grammar;
using static Rubberduck.Parsing.Grammar.VBAParser;
using Rubberduck.VBEditor;
using RubberduckTests.Mocks;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;

namespace RubberduckTests.Grammar
{
    [TestClass]
    public class SelectionExtensionsTests
    {
        public class CollectorVBAParserBaseVisitor<Result> : VBAParserBaseVisitor<IEnumerable<Result>>
        {
            protected override IEnumerable<Result> DefaultResult => new List<Result>();

            protected override IEnumerable<Result> AggregateResult(IEnumerable<Result> firstResult, IEnumerable<Result> secondResult)
            {
                return firstResult.Concat(secondResult);
            }
        }

        public class SubStmtContextElementCollectorVisitor : CollectorVBAParserBaseVisitor<SubStmtContext>
        {
            public override IEnumerable<SubStmtContext> VisitSubStmt([NotNull] SubStmtContext context)
            {
                return new List<SubStmtContext> { context };
            }
        }

        public class IfStmtContextElementCollectorVisitor : CollectorVBAParserBaseVisitor<IfStmtContext>
        {
            public override IEnumerable<IfStmtContext> VisitIfStmt([NotNull] IfStmtContext context)
            {
                return base.VisitIfStmt(context).Concat(new List<IfStmtContext> { context });
            }
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_Not_In_Selection_ZeroBased_EvilCode()
        {
            const string inputCode = @"
Option Explicit

Public _
    Sub _
foo()

Debug.Print ""foo""

    End _
  Sub : 'Lame comment!
";
            
            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            var selection = new Selection(3, 0, 10, 5);

            Assert.IsFalse(context.Contains(selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_In_Selection_OneBased_EvilCode()
        {
            const string inputCode = @"
Option Explicit

Public _
    Sub _
foo()

Debug.Print ""foo""

    End _
  Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            var selection = new Selection(4, 1, 11, 8);
            
            Assert.IsTrue(context.Contains(selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_Not_In_Selection_Start_OneBased_EvilCode()
        {
            const string inputCode = @"
Option Explicit

Public _
    Sub _
foo()

Debug.Print ""foo""

    End _
  Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            var selection = new Selection(5, 1, 11, 8);

            Assert.IsFalse(context.Contains(selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_Not_In_Selection_End_OneBased_EvilCode()
        {
            const string inputCode = @"
Option Explicit

Public _
    Sub _
foo()

Debug.Print ""foo""

    End _
  Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            var selection = new Selection(4, 1, 10, 8);

            Assert.IsFalse(context.Contains(selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_In_GetSelection_OneBased_EvilCode()
        {
            const string inputCode = @"
Option Explicit

Public _
    Sub _
foo()

Debug.Print ""foo""

    End _
  Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            pane.Selection = new Selection(4, 1, 11, 8);
            
            Assert.IsTrue(context.Contains(pane.Selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_Not_In_GetSelection_ZeroBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo()

Debug.Print ""foo""

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            pane.Selection = new Selection(3, 0, 7, 7);

            Assert.IsFalse(context.Contains(pane.Selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_In_GetSelection_OneBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo()

Debug.Print ""foo""

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            pane.Selection = new Selection(4, 1, 8, 8);

            Assert.IsTrue(context.Contains(pane.Selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_In_Selection_OneBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo()

Debug.Print ""foo""

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            var selection = new Selection(4, 1, 8, 8);

            Assert.IsTrue(context.Contains(selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_NotIn_Selection_StartTooSoon_OneBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo()

Debug.Print ""foo""

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            var selection = new Selection(4, 2, 8, 8);

            Assert.IsFalse(context.Contains(selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_NotIn_Selection_EndsTooSoon_OneBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo()

Debug.Print ""foo""

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new SubStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).First();
            var selection = new Selection(4, 1, 8, 7);

            Assert.IsFalse(context.Contains(selection));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        public void Context_In_Selection_FirstBlock_OneBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var contexts = visitor.Visit(tree);
            var selection = new Selection(6, 1, 10, 7);

            Assert.IsTrue(contexts.ElementAt(0).Contains(selection));   // first If block
            Assert.IsFalse(contexts.ElementAt(1).Contains(selection));  // second If block
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_Not_In_Selection_SecondBlock_OneBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var contexts = visitor.Visit(tree);
            var selection = new Selection(6, 1, 10, 7);

            Assert.IsTrue(contexts.ElementAt(0).Contains(selection));   // first If block
            Assert.IsFalse(contexts.ElementAt(1).Contains(selection));  // second If block
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Context_In_Selection_SecondBlock_OneBased()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var contexts = visitor.Visit(tree);
            var selection = new Selection(12, 1, 16, 7);

            Assert.IsFalse(contexts.ElementAt(0).Contains(selection));  // first If block
            Assert.IsTrue(contexts.ElementAt(1).Contains(selection));   // second If block
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Selection_Contains_LastToken()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var contexts = visitor.Visit(tree);
            var token = contexts.ElementAt(1).Stop;
            var selection = new Selection(12, 1, 16, 7);

            Assert.IsTrue(selection.Contains(token));                   // last token in second If block
            Assert.IsFalse(contexts.ElementAt(0).Contains(selection));  // first If block
            Assert.IsTrue(contexts.ElementAt(1).Contains(selection));   // second If block
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Selection_Not_Contains_LastToken()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var context = visitor.Visit(tree).Last();
            var token = context.Stop;
            var selection = new Selection(12, 1, 14, 1);

            Assert.IsFalse(selection.Contains(token));
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Selection_Contains_Only_Innermost_Nested_Context()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long, FooBar As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
  If FooBar Then
     Debug.Print ""Foo bar!""
  End If
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var contexts = visitor.Visit(tree); 
            var token = contexts.ElementAt(0).Stop; 
            var selection = new Selection(8, 1, 10, 9);

            Assert.IsTrue(selection.Contains(token));                   // last token in innermost If block
            Assert.IsTrue(contexts.ElementAt(0).Contains(selection));   // innermost If block
            Assert.IsFalse(contexts.ElementAt(1).Contains(selection));  // first outer If block
            Assert.IsFalse(contexts.ElementAt(2).Contains(selection));  // second outer If block
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Selection_Contains_Both_Nested_Context()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long, FooBar As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
  If FooBar Then
     Debug.Print ""Foo bar!""
  End If
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var contexts = visitor.Visit(tree); //returns innermost statement first then topmost consecutively
            var token = contexts.ElementAt(0).Stop;
            var selection = new Selection(6, 1, 13, 7);

            Assert.IsTrue(selection.Contains(token));                   // last token in innermost If block
            Assert.IsTrue(contexts.ElementAt(0).Contains(selection));   // innermost If block
            Assert.IsTrue(contexts.ElementAt(1).Contains(selection));   // first outer If block
            Assert.IsFalse(contexts.ElementAt(2).Contains(selection));  // second outer If block
        }

        [TestMethod]
        [TestCategory("Grammar")]
        [TestCategory("Selection")]
        public void Selection_Not_Contained_In_Neither_Nested_Context()
        {
            const string inputCode = @"
Option Explicit

Public Sub foo(Bar As Long, Baz As Long, FooBar As Long)

If Bar > Baz Then
  Debug.Print ""Yeah!""
  If FooBar Then
     Debug.Print ""Foo bar!""
  End If
Else
  Debug.Print ""Boo!""
End If

If Baz > Bar Then
  Debug.Print ""Boo!""
Else
  Debug.Print ""Yeah!""
End If

End Sub : 'Lame comment!
";

            var vbe = MockVbeBuilder.BuildFromSingleStandardModule(inputCode, out var component);
            var pane = component.CodeModule.CodePane;
            var state = MockParser.CreateAndParse(vbe.Object);
            var tree = state.GetParseTree(new QualifiedModuleName(component));
            var visitor = new IfStmtContextElementCollectorVisitor();
            var contexts = visitor.Visit(tree); //returns innermost statement first then topmost consecutively
            var token = contexts.ElementAt(0).Stop;
            var selection = new Selection(15, 1, 19, 7);

            Assert.IsFalse(selection.Contains(token));                  // last token in innermost If block
            Assert.IsFalse(contexts.ElementAt(0).Contains(selection));  // innermost If block
            Assert.IsFalse(contexts.ElementAt(1).Contains(selection));  // first outer if block
            Assert.IsTrue(contexts.ElementAt(2).Contains(selection));   // second outer If block
        }
    }
}