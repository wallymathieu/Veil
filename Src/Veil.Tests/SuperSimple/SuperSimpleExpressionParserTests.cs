﻿using DeepEqual.Syntax;
using NUnit.Framework;

namespace Veil.SuperSimple
{
    [TestFixture]
    internal class SuperSimpleExpressionParserTests
    {
        [Test]
        public void Should_parse_model_keywords_as_self_expression()
        {
            var model = new { };
            var result = SuperSimpleExpressionParser.Parse(model.GetType(), "Model");
            result.ShouldDeepEqual(SyntaxTreeNode.ExpressionNode.Self(model.GetType()));
        }

        [Test]
        public void Should_parse_current_keyword_as_self_expression()
        {
            var model = new { };
            var result = SuperSimpleExpressionParser.Parse(model.GetType(), "Current");
            result.ShouldDeepEqual(SyntaxTreeNode.ExpressionNode.Self(model.GetType()));
        }

        [Test]
        public void Should_parse_model_dot_property_as_proeprty_expression()
        {
            var model = new { Name = "foo" };
            var result = SuperSimpleExpressionParser.Parse(model.GetType(), "Model.Name");
            result.ShouldDeepEqual(SyntaxTreeNode.ExpressionNode.Property(model.GetType(), "Name"));
        }

        [TestCase("Model.Wrong")]
        [TestCase("Model.name")]
        public void Should_throw_for_invalid_expressions(string expression)
        {
            var model = new { Name = "foo" };
            Assert.Throws<VeilParserException>(() =>
            {
                SuperSimpleExpressionParser.Parse(model.GetType(), expression);
            });
        }
    }
}