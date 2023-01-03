using System;
using System.Collections.Generic;
using System.IO;
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
        /// GroupSensitiveWords
        /// </summary>
        private ISet<string> GroupSensitiveWords { get; } = new HashSet<string>();

        /// <summary>
        /// FullPaths
        /// </summary>
        private ISet<string> _fullPaths = new HashSet<string>();

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
        public SensitiveWordsOptions(HandleOptions handleOptions, ReplaceOptions replaceOptions, string character, bool ignoreCase = false, bool replaceSingle = false, GroupReplaceOptions groupReplaceOptions = GroupReplaceOptions.Default)
            : this(handleOptions, replaceOptions)
        {
            Character = character;
            IgnoreCase = ignoreCase;
            ReplaceSingle = replaceSingle;
            GroupReplaceOptions = groupReplaceOptions;
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
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 添加敏感词，多个英文竖线|分隔，包含竖线需要转义
        /// </summary>
        /// <param name="words"></param>
        public SensitiveWordsOptions Add(string words)
        {
            if (string.IsNullOrWhiteSpace(words))
            {
                return this;
            }

            foreach (var word in Regex.Split(words, @"(?<!(?<!\\)\([^\)]*)(?<!\\)\|(?![^\(]*(?<!\\)\))", RegexOptions.Multiline))
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    if (1 == new Regex(word).GetGroupNumbers().Length)
                    {
                        SimpleSensitiveWords.Add(word);
                    }
                    else
                    {
                        GroupSensitiveWords.Add(word);
                    }
                }
            }

            SensitiveWords.Clear();
            SensitiveWords.UnionWith(GroupSensitiveWords);
            if (SimpleSensitiveWords.Count > 0)
            {
                SensitiveWords.Add(string.Join("|", SimpleSensitiveWords));
            }

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
        /// Replace
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal string Replace(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                foreach (var word in SensitiveWords)
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
                foreach (var word in SensitiveWords)
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
    }
}