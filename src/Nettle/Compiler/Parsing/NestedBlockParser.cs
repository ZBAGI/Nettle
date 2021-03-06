﻿namespace Nettle.Compiler.Parsing
{
    using Nettle.Compiler.Parsing.Blocks;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a function code block parser
    /// </summary>
    internal abstract class NestedBlockParser : NettleParser, IBlockParser
    {
        /// <summary>
        /// Constructs the parser with a blockifier
        /// </summary>
        /// <param name="blockifier">The blockifier</param>
        protected NestedBlockParser
            (
                IBlockifier blockifier
            )
        {
            Validate.IsNotNull(blockifier);

            this.Blockifier = blockifier;
        }

        /// <summary>
        /// Gets the blockifier
        /// </summary>
        protected IBlockifier Blockifier { get; private set; }

        /// <summary>
        /// Gets the open tag name
        /// </summary>
        protected abstract string TagName { get; }

        /// <summary>
        /// Gets an array of body partition tag names
        /// </summary>
        protected virtual string[] BodyPartitionTagNames
        {
            get
            {
                return new string[] { };
            }
        }

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
            return signatureBody.StartsWith
            (
                this.TagName
            );
        }

        /// <summary>
        /// Parses the signature into a code block object
        /// </summary>
        /// <param name="templateContent">The template content</param>
        /// <param name="positionOffSet">The position offset index</param>
        /// <param name="signature">The block signature</param>
        /// <returns>The parsed code block</returns>
        public abstract CodeBlock Parse
        (
            ref string templateContent,
            ref int positionOffSet,
            string signature
        );

        /// <summary>
        /// Gets the code blocks open tag syntax based on the tag name
        /// </summary>
        /// <returns>The open tag syntax</returns>
        protected string GetOpenTagSyntax()
        {
            return @"{{" + this.TagName + " ";
        }

        /// <summary>
        /// Gets the code blocks close tag syntax based on the tag name
        /// </summary>
        /// <returns>The close tag syntax</returns>
        protected string GetCloseTagSyntax()
        {
            return @"{{/" + this.TagName + @"}}";
        }

        /// <summary>
        /// Extracts the body of a nested code block
        /// </summary>
        /// <param name="templateContent">The template content</param>
        /// <param name="positionOffSet">The position offset index</param>
        /// <param name="signature">The variable signature</param>
        /// <returns>The extracted block</returns>
        protected NestableCodeBlock ExtractNestedBody
            (
                ref string templateContent,
                ref int positionOffSet,
                string signature
            )
        {
            var startIndex = signature.Length;
            var templateLength = templateContent.Length;
            var body = String.Empty;

            var openTagSyntax = GetOpenTagSyntax();
            var closeTagSyntax = GetCloseTagSyntax();
            var partitionTags = new List<string>();

            var openTagCount = 1;
            var closeTagCount = 0;
            var partitionTagCount = 0;
            var endedOnPartition = false;
            var partitionSignature = String.Empty;
            var endFound = false;

            foreach (var tagName in this.BodyPartitionTagNames)
            {
                partitionTags.Add
                (
                    @"{{" + tagName
                );
            }

            for (int currentIndex = startIndex; currentIndex < templateLength; currentIndex++)
            {
                body += templateContent[currentIndex];

                if (body.Length > 1)
                {
                    if (body.EndsWith(openTagSyntax))
                    {
                        openTagCount++;
                    }
                    else if (body.EndsWith(closeTagSyntax))
                    {
                        closeTagCount++;
                        partitionTagCount = 0;
                    }
                    else
                    {
                        var matchedTag = partitionTags.FirstOrDefault
                        (
                            tag => body.EndsWith(tag)
                        );

                        if (matchedTag != null)
                        {
                            partitionTagCount++;
                            partitionSignature = matchedTag;
                        }
                    }
                }

                if (openTagCount == closeTagCount)
                {
                    // The final closing tag was found
                    endFound = true;
                    break;
                }
                else if (partitionTagCount > 0 && openTagCount == (closeTagCount + 1))
                {
                    // A partition tag was found
                    endFound = true;
                    endedOnPartition = true;
                    break;
                }
            }

            if (false == endFound)
            {
                throw new NettleParseException
                (
                    "No '{0}' tag was found.".With
                    (
                        closeTagSyntax
                    ),
                    templateLength
                );
            }

            if (endedOnPartition)
            {
                signature += body.Substring
                (
                    0,
                    body.Length - partitionSignature.Length
                );
            }
            else
            {
                signature += body;
            }

            body = body.Substring
            (
                0,
                body.Length - closeTagSyntax.Length
            );

            var blocks = this.Blockifier.Blockify(body);
            var startPosition = positionOffSet;
            var endPosition = (startPosition + signature.Length);

            TrimTemplate
            (
                ref templateContent,
                ref positionOffSet,
                signature
            );

            return new NestableCodeBlock()
            {
                Signature = signature,
                StartPosition = startPosition,
                EndPosition = endPosition,
                Body = body,
                Blocks = blocks
            };
        }
    }
}
