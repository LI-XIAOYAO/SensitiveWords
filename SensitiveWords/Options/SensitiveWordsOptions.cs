using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SensitiveWords
{
    /// <summary>
    /// 敏感词选项
    /// </summary>
    public sealed class SensitiveWordsOptions
    {
        /// <summary>
        /// SensitiveWords
        /// </summary>
        private ISet<string> SensitiveWords { get; } = new HashSet<string>();

        /// <summary>
        /// SimpleSensitiveWords
        /// </summary>
        private ISet<string> SimpleSensitiveWords { get; } = new HashSet<string>();

        /// <summary>
        /// RegexSensitiveWords
        /// </summary>
        private ISet<string> RegexSensitiveWords { get; } = new HashSet<string>();

        /// <summary>
        /// FullPaths
        /// </summary>
        private readonly ISet<string> _fullPaths = new HashSet<string>();

        /// <summary>
        /// 正则元字符
        /// </summary>
        private static readonly char[] _regexMetacharacters = new[] { '\\', '*', '+', '?', '|', '{', '[', '(', ')', '^', '$', '.', '#' };

        private WordsNodes WordsNodes { get; set; }

        /// <summary>
        /// 敏感词选项
        /// </summary>
        public SensitiveWordsOptions()
        {
        }

        /// <summary>
        /// 敏感词选项
        /// </summary>
        /// <param name="handleOptions">处理选项</param>
        /// <param name="replaceOptions">替换选项</param>
        public SensitiveWordsOptions(HandleOptions handleOptions, ReplaceOptions replaceOptions)
        {
            HandleOptions = handleOptions;
            ReplaceOptions = replaceOptions;
        }

        /// <summary>
        /// 敏感词选项
        /// </summary>
        /// <param name="handleOptions">处理选项</param>
        /// <param name="replaceOptions">替换选项</param>
        /// <param name="character">替换字符串</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <param name="replaceSingle">替换为单个不补位</param>
        /// <param name="groupReplaceOptions">组替换选项</param>
        /// <param name="isMaxMatch">优先匹配最大长度（正则下不生效）</param>
        /// <param name="whiteSpaceOptions">空字符选项</param>
        public SensitiveWordsOptions(HandleOptions handleOptions, ReplaceOptions replaceOptions, string character, bool ignoreCase = false, bool replaceSingle = false, GroupReplaceOptions groupReplaceOptions = GroupReplaceOptions.Default, bool isMaxMatch = true, WhiteSpaceOptions whiteSpaceOptions = WhiteSpaceOptions.Default)
            : this(handleOptions, replaceOptions)
        {
            Character = character;
            IgnoreCase = ignoreCase;
            ReplaceSingle = replaceSingle;
            GroupReplaceOptions = groupReplaceOptions;
            IsMaxMatch = isMaxMatch;
            WhiteSpaceOptions = whiteSpaceOptions;
        }

        /// <summary>
        /// 处理选项
        /// </summary>
        public HandleOptions HandleOptions { get; set; }

        /// <summary>
        /// 替换选项
        /// </summary>
        public ReplaceOptions ReplaceOptions { get; set; }

        /// <summary>
        /// 替换字符串
        /// </summary>
        public string Character { get; set; }

        /// <summary>
        /// 忽略大小写
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// 替换为单个不补位
        /// </summary>
        public bool ReplaceSingle { get; set; }

        /// <summary>
        /// 组替换选项
        /// </summary>
        public GroupReplaceOptions GroupReplaceOptions { get; set; }

        /// <summary>
        /// 优先匹配最大长度（正则下不生效）
        /// </summary>
        public bool IsMaxMatch { get; set; }

        /// <summary>
        /// 空字符选项
        /// </summary>
        public WhiteSpaceOptions WhiteSpaceOptions { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 添加敏感词，多个英文竖线|分隔，包含竖线需要转义，支持正则
        /// </summary>
        /// <param name="words"></param>
        public SensitiveWordsOptions Add(string words)
        {
            if (string.IsNullOrWhiteSpace(words))
            {
                return this;
            }

            ClassificationWords(words, word =>
            {
                if (!IsRegex(word))
                {
                    SimpleSensitiveWords.Add(word);
                }
                else
                {
                    RegexSensitiveWords.Add(word);
                }

                SensitiveWords.Add(word);
            });

            return this;
        }

        /// <summary>
        /// 添加文件敏感词
        /// </summary>
        /// <param name="path"></param>
        public SensitiveWordsOptions AddFile(string path)
        {
            path = Path.GetFullPath(path);

            if (_fullPaths.Contains(path))
            {
                return this;
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.", path);
            }

            _fullPaths.Add(path);

            return Add(File.ReadAllText(path));
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public SensitiveWordsOptions SetTag(string tag)
        {
            if (null == tag)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            Tag = tag;

            return this;
        }

        /// <summary>
        /// 删除敏感词
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public bool Remove(string words)
        {
            bool result = false;
            ClassificationWords(words, word =>
            {
                if (result |= SimpleSensitiveWords.Remove(word) || RegexSensitiveWords.Remove(word))
                {
                    SensitiveWords.Remove(word);
                }
            });

            return result;
        }

        /// <summary>
        /// 构建敏感词选项（添加删除非正则敏感词后调用生效）
        /// </summary>
        /// <returns></returns>
        public SensitiveWordsOptions Build()
        {
            WordsNodes = WordsNodes.Build(SimpleSensitiveWords, IgnoreCase);

            return this;
        }

        /// <summary>
        /// 获取敏感词
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSensitiveWords() => SimpleSensitiveWords.AsEnumerable().Union(RegexSensitiveWords);

        /// <summary>
        /// 是否存在指定敏感词
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool Contains(string word) => SimpleSensitiveWords.Contains(word) || RegexSensitiveWords.Contains(word);

        /// <summary>
        /// Replace
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal string Replace(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (null != WordsNodes)
                {
                    value = WordsNodes.Replace(value, MatchEvaluator, IsMaxMatch, WhiteSpaceOptions);
                }

                foreach (var word in RegexSensitiveWords)
                {
                    value = Regex.Replace(value, word, MatchEvaluator, IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                }
            }

            return value;
        }

        /// <summary>
        /// IsMatch
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool IsMatch(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (null != WordsNodes && WordsNodes.Matches(value, IsMaxMatch, WhiteSpaceOptions).Any())
                {
                    return true;
                }

                foreach (var word in RegexSensitiveWords)
                {
                    if (Regex.IsMatch(value, word))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// MatchEvaluator
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string MatchEvaluator(Match match)
        {
            switch (ReplaceOptions)
            {
                case ReplaceOptions.Character:
                    return MatchGroup(match, c => GetCharacter(c.Length));

                case ReplaceOptions.PinYin:
                    return MatchGroup(match, c => c.ToPinYinInfo().PinYin);

                case ReplaceOptions.JianPin:
                    return MatchGroup(match, c => c.ToPinYinInfo().JianPin);

                case ReplaceOptions.Homophone:
                    return MatchGroup(match, c => c.ToPinYinInfo().Homophone);

                default:
                    return match.Value;
            }
        }

        /// <summary>
        /// MatchEvaluator
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string MatchEvaluator(NodeCapture match)
        {
            switch (ReplaceOptions)
            {
                case ReplaceOptions.Character:
                    return GetCharacter(match.Length);

                case ReplaceOptions.PinYin:
                    return match.Value.ToPinYinInfo().PinYin;

                case ReplaceOptions.JianPin:
                    return match.Value.ToPinYinInfo().JianPin;

                case ReplaceOptions.Homophone:
                    return match.Value.ToPinYinInfo().Homophone;

                default:
                    return match.Value;
            }
        }

        /// <summary>
        /// MatchGroup
        /// </summary>
        /// <param name="match"></param>
        /// <param name="replaceCharFunc"></param>
        /// <returns></returns>
        private string MatchGroup(Match match, Func<string, string> replaceCharFunc)
        {
            var value = match.Value;

            string GroupReplace()
            {
                for (int i = 1; i < match.Groups.Count; i++)
                {
                    if (match.Groups[i].Success)
                    {
                        value = value.Replace(match.Groups[i].Index - match.Index, match.Groups[i].Length, replaceCharFunc(match.Groups[i].Value));
                    }
                }

                return value;
            }

            switch (GroupReplaceOptions)
            {
                case GroupReplaceOptions.Default:
                    return replaceCharFunc(match.Value);

                case GroupReplaceOptions.GroupOnly:
                    return GroupReplace();

                case GroupReplaceOptions.GroupPriority:
                    return match.Groups.Count > 1 ? GroupReplace() : replaceCharFunc(match.Value);

                default:
                    return value;
            }
        }

        /// <summary>
        /// GetCharacter
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private string GetCharacter(int length) => string.IsNullOrEmpty(Character) ? string.Empty : ReplaceSingle || Character.Length > 1 ? Character : Character.PadLeft(length, Character[0]);

        /// <summary>
        /// 敏感词分类
        /// </summary>
        /// <param name="words"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private static void ClassificationWords(string words, Action<string> action)
        {
            if (null == words || null == action)
            {
                return;
            }

            for (int i = 0, position = 0, op = 0, cp = 0; i < words.Length; i++)
            {
                if (0 == i || '\\' != words[i - 1])
                {
                    switch (words[i])
                    {
                        case '|':
                            if (op == cp)
                            {
                                if (i > position)
                                {
                                    var value = words.Substring(position, i - position);
                                    if (!string.IsNullOrWhiteSpace(value))
                                    {
                                        action(value);
                                    }
                                }

                                position = i + 1;
                            }

                            break;

                        case '(':
                        case '{':
                        case '[':
                            op++;
                            break;

                        case ')':
                        case '}':
                        case ']':
                            cp++;
                            break;
                    }
                }

                if (i == words.Length - 1 && i >= position)
                {
                    var value = words.Substring(position);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        action(value);
                    }
                }
            }
        }

        /// <summary>
        /// IsRegex
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        private static bool IsRegex(string words)
        {
            var index = 0;
            while (index < words.Length)
            {
                if ('\\' != words[index] && _regexMetacharacters.Contains(words[index]))
                {
                    return true;
                }
                else if ('\\' == words[index])
                {
                    if (index != words.Length - 1 && !_regexMetacharacters.Contains(words[index + 1]))
                    {
                        return true;
                    }

                    index++;
                }

                index++;
            }

            return false;
        }
    }
}