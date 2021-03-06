﻿namespace Nettle.Compiler.Rendering
{
    using Nettle.Functions;
    using System.Diagnostics;

    /// <summary>
    /// Represents the default implementation of a template renderer
    /// </summary>
    internal class TemplateRenderer : NettleRendererBase, ITemplateRenderer
    {
        private BlockCollectionRenderer _collectionRenderer;

        /// <summary>
        /// Constructs the renderer with required dependencies
        /// </summary>
        /// <param name="functionRepository">The function repository</param>
        /// <param name="templateRepository">The template repository</param>
        public TemplateRenderer
            (
                IFunctionRepository functionRepository,
                IRegisteredTemplateRepository templateRepository
            )

            : base(functionRepository)
        {
            Validate.IsNotNull(templateRepository);

            _collectionRenderer = new BlockCollectionRenderer
            (
                functionRepository,
                templateRepository
            );
        }

        /// <summary>
        /// Renders a Nettle template with the model data specified
        /// </summary>
        /// <param name="template">The template</param>
        /// <param name="model">The model data</param>
        /// <returns>The rendered template</returns>
        public string Render
            (
                Template template,
                object model
            )
        {
            Validate.IsNotNull(template);

            var watch = Stopwatch.StartNew();
            
            var debugMode = template.IsFlagSet
            (
                TemplateFlag.DebugMode
            );
            
            var context = new TemplateContext
            (
                model,
                template.Flags
            );

            var blocks = template.Blocks;

            var output = _collectionRenderer.Render
            (
                ref context,
                blocks,
                template.Flags
            );

            if (debugMode)
            {
                watch.Stop();

                var debugInfo = context.GenerateDebugInfo
                (
                    watch.Elapsed
                );

                output += "\r\n\r\n{0}".With
                (
                    debugInfo
                );
            }

            return output;
        }
    }
}
