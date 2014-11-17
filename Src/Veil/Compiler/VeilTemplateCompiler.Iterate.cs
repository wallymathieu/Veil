﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Veil.Parser.Nodes;

namespace Veil.Compiler
{
    internal partial class VeilTemplateCompiler<T>
    {
        private static MethodInfo moveNextMethod = typeof(System.Collections.IEnumerator).GetMethod("MoveNext");
        private static MethodInfo disposeMethod = typeof(IDisposable).GetMethod("Dispose");

        private Expression HandleIterate(IterateNode node)
        {
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(node.ItemType);
            var getEnumeratorMethod = enumerableType.GetMethod("GetEnumerator");
            var getCurrentMethod = getEnumeratorMethod.ReturnType.GetProperty("Current").GetGetMethod();

            var currentElement = Expression.Variable(node.ItemType, "current");
            var didMoveNext = Expression.Variable(typeof(bool), "didMoveNext");
            var hasElements = Expression.Variable(typeof(bool), "hasElements");
            var enumerator = Expression.Variable(getEnumeratorMethod.ReturnType, "enumerator");
            var exitLabel = Expression.Label();

            var collection = ParseExpression(node.Collection);
            if (collection.Type == typeof(object))
            {
                collection = Expression.Convert(collection, enumerableType);
            }

            this.PushScope(currentElement);
            var loopBody = HandleNode(node.Body);
            this.PopScope();

            var result = Expression.Block(
                new[] { enumerator, hasElements },
                Expression.Assign(hasElements, Expression.Constant(false)),
                Expression.Assign(enumerator, Expression.Call(collection, getEnumeratorMethod)),
                Expression.Loop(Expression.Block(
                    new[] { didMoveNext },
                    Expression.Assign(didMoveNext, Expression.Call(enumerator, moveNextMethod)),
                    Expression.IfThenElse(Expression.IsFalse(didMoveNext),
                        Expression.Break(exitLabel),
                        Expression.Block(
                            new[] { currentElement },
                            Expression.Assign(currentElement, Expression.Property(enumerator, getCurrentMethod)),
                            Expression.Assign(hasElements, Expression.Constant(true)),
                            loopBody
                        )
                    )
                ), exitLabel),
                DisposeIfNeeded(enumerator),
                Expression.IfThen(Expression.IsFalse(hasElements), HandleNode(node.EmptyBody))
            );
            return result;
        }

        private static Expression DisposeIfNeeded(Expression instance)
        {
            if (!typeof(IDisposable).IsAssignableFrom(instance.Type))
            {
                return Expression.Empty();
            }
            return Expression.Call(instance, disposeMethod);
        }
    }
}