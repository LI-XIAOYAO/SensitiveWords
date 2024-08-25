using System;
using System.Collections.Generic;
using System.Threading;

namespace SensitiveWords
{
    /// <summary>
    /// 节点捕获集
    /// </summary>
    public class NodeCaptures : InternalReadOnlyCollection<NodeCapture>
    {
        private readonly WordsNodes _wordsNodes;

        internal NodeCaptures(string value, WordsNodes wordsNodes, bool isMaxMatch, WhiteSpaceOptions whiteSpaceOptions, CancellationToken cancellationToken)
        {
            Value = value;
            _wordsNodes = wordsNodes;

            if (string.IsNullOrEmpty(value) || 0 == wordsNodes.Count)
            {
                return;
            }

            var isIgnoreWhiteSpace = (whiteSpaceOptions & WhiteSpaceOptions.IgnoreWhiteSpace) > 0;
            var isIgnoreNewLine = (whiteSpaceOptions & WhiteSpaceOptions.IgnoreNewLine) > 0;

            for (int i = 0, position = 0; i < value.Length; position = ++i)
            {
                cancellationToken.ThrowIfCancellationRequested();

                i = Match(wordsNodes, i, position);
            }

            int Match(WordsNodes nodes, int i, int position)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (i < value.Length)
                {
                    var node = nodes[value[i]];
                    if (null == node && i > position && (isIgnoreWhiteSpace || isIgnoreNewLine))
                    {
                        node = nodes[value[i = MoveIndex(value, i, isIgnoreWhiteSpace, isIgnoreNewLine, cancellationToken)]];
                    }

                    if (null != node)
                    {
                        if (node.IsEnd && !(isMaxMatch && node.HasNodes()))
                        {
                            Add(new NodeCapture(position, i - position + 1));
                            position = i;
                        }
                        else
                        {
                            var p = position;
                            if (node.HasNodes())
                            {
                                position = Match(node.Nodes, ++i, position);
                            }

                            if (node.IsEnd && p == position)
                            {
                                Add(new NodeCapture(position, i - position));
                                position = i - 1;
                            }
                        }
                    }
                }

                return position;
            }
        }

        /// <summary>
        /// 节点捕获集
        /// </summary>
        public IReadOnlyList<NodeCapture> Captures => this;

        /// <summary>
        /// 单词节点集
        /// </summary>
        public WordsNodes WordsNodes => _wordsNodes;

        /// <summary>
        /// 捕获值
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="nodeCapture"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected internal override void Add(NodeCapture nodeCapture)
        {
            if (null == nodeCapture)
            {
                throw new ArgumentNullException(nameof(nodeCapture));
            }

            nodeCapture.Captures = this;

            base.Add(nodeCapture);
        }

        /// <summary>
        /// 获取忽略空白后的索引
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <param name="isIgnoreWhiteSpace"></param>
        /// <param name="isIgnoreNewLine"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static int MoveIndex(string text, int index, bool isIgnoreWhiteSpace, bool isIgnoreNewLine, CancellationToken cancellationToken)
        {
            while (((isIgnoreWhiteSpace && text[index] is ' ') || (isIgnoreNewLine && (text[index] is '\r' || text[index] is '\n'))) && index < text.Length - 1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                index++;
            }

            return index;
        }

        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="matchEvaluator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public string Replace(Func<NodeCapture, string> matchEvaluator, CancellationToken cancellationToken = default)
        {
            if (null == matchEvaluator || 0 == Count)
            {
                return Value;
            }

            var offset = 0;
            var text = Value;
            foreach (var nodeCapture in this)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var replacement = matchEvaluator(nodeCapture) ?? string.Empty;
                if (replacement != nodeCapture.Value)
                {
                    text = text.Replace(nodeCapture.Index + offset, nodeCapture.Length, replacement);

                    offset += replacement.Length - nodeCapture.Length;
                }
            }

            return text;
        }

        /// <summary>
        /// 替换为指定字符串
        /// </summary>
        /// <param name="replacement"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public string Replace(string replacement, CancellationToken cancellationToken = default) => Replace(c => replacement, cancellationToken);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{nameof(Count)} = {Count}, {nameof(WordsNodes)} = {WordsNodes.Count}";
    }
}