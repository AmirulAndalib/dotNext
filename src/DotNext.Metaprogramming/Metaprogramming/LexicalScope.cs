using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace DotNext.Metaprogramming
{
    /// <summary>
    /// Represents lexical scope in the form of instructions inside it and set of declared local variables.
    /// </summary>
    internal class LexicalScope : ILexicalScope, IDisposable, IEnumerable<Expression>
    {
        private class StatementNode : IDisposable
        {
            internal StatementNode(Expression statement) => Statement = statement;

            internal Expression Statement { get; }

            internal StatementNode Next { get; private protected set; }

            internal StatementNode CreateNext(Expression statement) => Next = new StatementNode(statement);

            public virtual void Dispose() => Next = null;
        }

        private sealed class Enumerator : StatementNode, IEnumerator<Expression>
        {
            private StatementNode current;

            internal Enumerator(StatementNode first)
                : base(Expression.Empty())
            {
                Next = first;
                current = this;
            }

            public bool MoveNext()
            {
                current = Next;
                if(current is null)
                    return false;
                else
                {
                    Next = current.Next;
                    return true;
                }
            }

            public Expression Current => current?.Statement;

            object IEnumerator.Current => Current;

            void IEnumerator.Reset() => throw new NotSupportedException();

            public override void Dispose()
            {
                current = null;
                base.Dispose();
            }
        }

        [ThreadStatic]
        private static LexicalScope current;

        internal static S FindScope<S>()
            where S : class, ILexicalScope
        {
            for(var current = LexicalScope.current; !(current is null); current = current.Parent)
                if(current is S scope)
                    return scope;
            return null;
        }

        internal static bool IsInScope<S>() where S : class, ILexicalScope => !(FindScope<S>() is null);

        internal static ILexicalScope Current => current ?? throw new InvalidOperationException(ExceptionMessages.OutOfLexicalScope);

        private readonly Dictionary<string, ParameterExpression> variables = new Dictionary<string, ParameterExpression>();

        private StatementNode first, last;
        private protected readonly LexicalScope Parent;
        private SymbolDocumentInfo sourceCode;

        private protected LexicalScope(bool isStatement)
        {
            if(isStatement && current is null)
                throw new InvalidOperationException(ExceptionMessages.OutOfLexicalScope);
            Parent = current;
            current = this;
        }

        internal void EnableDebugging() => sourceCode = Expression.SymbolDocument(Path.GetTempFileName());

        private protected SymbolDocumentInfo SymbolDocument => Parent is null ? sourceCode : Parent.SymbolDocument;

        ParameterExpression ILexicalScope.this[string variableName]
        {
            get
            {
                for (var current = this; !(current is null); current = current.Parent)
                    if (current.variables.TryGetValue(variableName, out var variable))
                        return variable;
                return null;
            }
        }
        
        private protected IReadOnlyCollection<ParameterExpression> Variables => variables.Values;

        private void AddStatementCore(Expression statement)
            => last = first is null || last is null ? first = new StatementNode(statement) : last.CreateNext(statement);

        public void AddStatement(Expression statement)
        {
            var document = SymbolDocument;
            if(!(document is null))
                AddStatementCore(Expression.DebugInfo(document, 0, 0, 0, 0));
            AddStatementCore(statement);
        }

        public void DeclareVariable(ParameterExpression variable) => variables.Add(variable.Name, variable);

        private protected Expression Build()
        {
            if(first is null)
                return Expression.Empty();
            else if(ReferenceEquals(first, last) && Variables.Count == 0)
                return first.Statement;
            else
                return Expression.Block(Variables, this);
        }

        private Enumerator GetEnumerator() => new Enumerator(first);

        IEnumerator<Expression> IEnumerable<Expression>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Dispose()
        {
            for(var current = first; !(current is null); current = current.Next)
                current.Dispose();
            first = last = null;
            sourceCode = null;
            variables.Clear();
            current = Parent;
        }
    }
}