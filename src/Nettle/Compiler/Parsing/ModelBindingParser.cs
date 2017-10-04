﻿namespace Nettle.Compiler.Parsing
{
    using Nettle.Compiler.Parsing.Blocks;
    using System.Linq;

    /// <summary>
    /// Represents a model binding code block parser
    /// </summary>
    internal sealed class ModelBindingParser : NettleParser, IBlockParser
    {
        /// <summary>
        /// Determines if a signature matches the block type of the parser
        /// </summary>
        /// <param name="signatureBody">The signature body</param>
        /// <returns>True, if it matches; otherwise false</returns>
        public bool Matches
            (
                string signatureBody
            )
        {
            var excludedValues = new string[]
            {
                "!",
                "@",
                ">",
                "var ",
                "reassign ",
                "each ",
                "if ",
            };

            return false == excludedValues.Any
            (
                value => signatureBody.StartsWith
                (
                    value
                )
            );
        }

        /// <summary>
        /// Parses the code block signature into a code block object
        /// </summary>
        /// <param name="templateContent">The template content</param>
        /// <param name="positionOffSet">The position offset index</param>
        /// <param name="signature">The block signature</param>
        /// <returns>The parsed code block</returns>
        public CodeBlock Parse
            (
                ref string templateContent,
                ref int positionOffSet,
                string signature
            )
        {
            var bindingValue = NettleValueType.ModelBinding.ParseValue
            (
                signature
            );

            var startPosition = positionOffSet;
            var endPosition = startPosition + (signature.Length - 1);

            TrimTemplate
            (
                ref templateContent,
                ref positionOffSet,
                signature
            );

            return new ModelBinding()
            {
                Signature = signature,
                StartPosition = startPosition,
                EndPosition = endPosition,
                BindingPath = bindingValue.ToString()
            };
        }
    }
}
