﻿using System;
using System.IO;
using NUnit.Framework;

namespace Veil
{
    [TestFixture]
    public class VeilEngineTests
    {
        private readonly IVeilEngine engine = new VeilEngine();

        [TestCase("Hello {{ Name }}. You have visited us {{ ViewCount }} times!", "Hello Chris. You have visited us 10 times!")]
        [TestCase("{{#if Name}}Hello {{Name}}{{/if}}", "Hello Chris")]
        [TestCase("{{#if ViewCount}}Count: {{ViewCount}}{{/if}}", "Count: 10")]
        [TestCase("{{#if IsAdmin}}Yo Admin!{{/if}}", "")]
        [TestCase("{{#if IsAdmin}}Yo Admin!{{else}}Sorry{{/if}}", "Sorry")]
        public void Should_render_a_hail_template(string template, string expectedResult)
        {
            var view = Compile(template);
            var result = Execute(view, new ViewModel { Name = "Chris", ViewCount = 10, IsAdmin = false });

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private Action<TextWriter, ViewModel> Compile(string template)
        {
            using (var reader = new StringReader(template))
            {
                return this.engine.Compile<ViewModel>(reader);
            }
        }

        private string Execute(Action<TextWriter, ViewModel> view, ViewModel model)
        {
            using (var writer = new StringWriter())
            {
                view(writer, model);
                return writer.ToString();
            }
        }

        private class ViewModel
        {
            public string Name { get; set; }

            public int ViewCount { get; set; }

            public bool IsAdmin { get; set; }
        }
    }
}