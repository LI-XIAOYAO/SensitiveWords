using System;
using System.Collections.Generic;
using System.Linq;

namespace SensitiveWords
{
    /// <summary>
    /// 单词节点集
    /// </summary>
    public class WordsNodes : InternalReadOnlyCollection<WordsNode>
    {
        /// <summary>
        /// 有效词组数
        /// </summary>
        public int WordsCount => Items.Sum(c => c.WordsCount);

        /// <summary>
        /// 是否存在节点
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsNode(char value) => Items.Any(c => c.ValueEquals(value));

        /// <summary>
        /// 构建词组节点集
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static WordsNodes Build(IEnumerable<string> list, bool ignoreCase = true)
        {
            if (null == list)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var root = new WordsNodes();
            foreach (var words in list)
            {
                var current = root;
                for (int i = 0; i < words.Length; i++)
                {
                    var wordsNode = 0 == i ? root.Find(words[i]) : current.Find(words[i]);
                    if (null == wordsNode)
                    {
                        current.Add(wordsNode = WordsNode.Create(words[i], i == words.Length - 1, ignoreCase));
                    }

                    wordsNode.IsEnd |= i == words.Length - 1;

                    current = wordsNode.Nodes;
                }
            }

            return root;
        }

        /// <summary>
        /// 查找节点
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public WordsNode Find(char value) => Items.FirstOrDefault(c => c.ValueEquals(value));

        /// <summary>
        /// 匹配字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isMaxMatch"></param>
        /// <returns></returns>
        public NodeCaptures Matches(string text, bool isMaxMatch = true)
        {
            var nodeCaptures = new NodeCaptures(text, this);

            if (string.IsNullOrEmpty(text))
            {
                return nodeCaptures;
            }

            for (int i = 0, position = 0; i < text.Length; position = ++i)
            {
                i = Match(this, i, position);
            }

            int Match(WordsNodes wordsNodes, int i, int position)
            {
                if (i < text.Length)
                {
                    var node = wordsNodes.Find(text[i]);
                    if (null != node)
                    {
                        if (node.IsEnd && !(isMaxMatch && node.HasNodes()))
                        {
                            nodeCaptures.Add(new NodeCapture(position, i - position + 1));
                            position = i - 1;
                        }
                        else
                        {
                            var p = position;
                            position = Match(node.Nodes, ++i, position);

                            if (node.IsEnd && p == position)
                            {
                                nodeCaptures.Add(new NodeCapture(position, i - position));
                                position = i - 1;
                            }
                        }
                    }
                }

                return position;
            }

            return nodeCaptures;
        }

        /// <summary>
        /// 替换为指定字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacement"></param>
        /// <param name="isMaxMatch"></param>
        /// <returns></returns>
        public string Replace(string text, string replacement, bool isMaxMatch = true) => Replace(text, c => replacement, isMaxMatch);

        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="text"></param>
        /// <param name="matchEvaluator"></param>
        /// <param name="isMaxMatch"></param>
        /// <returns></returns>
        public string Replace(string text, Func<NodeCapture, string> matchEvaluator, bool isMaxMatch = true)
        {
            if (null == matchEvaluator)
            {
                return text;
            }

            var nodeCaptures = Matches(text, isMaxMatch);
            if (0 == nodeCaptures.Count)
            {
                return text;
            }

            return nodeCaptures.Replace(matchEvaluator, isMaxMatch);
        }

        /// <summary>
        /// 获取有效词组
        /// </summary>
        /// <returns></returns>
        public List<string> GetWords()
        {
            var list = new List<string>();
            GetWords(this, string.Empty);

            void GetWords(WordsNodes wordsNode, string value)
            {
                foreach (var item in wordsNode)
                {
                    var val = $"{value}{item.Value}";
                    if (item.IsEnd)
                    {
                        list.Add(val);
                    }

                    if (item.Nodes.Count > 0)
                    {
                        GetWords(item.Nodes, val);
                    }
                }
            }

            return list;
        }
    }
}