using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SensitiveWords
{
    /// <summary>
    /// 敏感词选项
    /// </summary>
    public sealed class SensitiveWordsOptions
    {
        /// <summary>
        /// FullPaths
        /// </summary>
        private readonly HashSet<string> _fullPaths = new HashSet<string>();

        /// <summary>
        /// SensitiveWords
        /// </summary>
        private readonly HashSet<string> _sensitiveWords = new HashSet<string>();

        /// <summary>
        /// SimpleSensitiveWords
        /// </summary>
        private readonly HashSet<string> _simpleSensitiveWords = new HashSet<string>();

        /// <summary>
        /// RegexSensitiveWords
        /// </summary>
        private readonly HashSet<string> _regexSensitiveWords = new HashSet<string>();

        /// <summary>
        /// _regexKeys
        /// </summary>
        private RegexKey[] _regexKeys = Array.Empty<RegexKey>();

        /// <summary>
        /// _lock
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// WordsNodes
        /// </summary>
        private WordsNodes _wordsNodes;

        /// <summary>
        /// _isIgnoreCase
        /// </summary>
        private bool _isIgnoreCase;

        /// <summary>
        /// _regexMatchTimeout
        /// </summary>
        private TimeSpan _regexMatchTimeout = Regex.InfiniteMatchTimeout;

        /// <summary>
        /// _isRegexCompiled
        /// </summary>
        private bool _isRegexCompiled;

        static SensitiveWordsOptions()
        {
            Regex.CacheSize = 128;
        }

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
            if (0 != (handleOptions & ~(HandleOptions.Default | HandleOptions.Input | HandleOptions.Output)) || handleOptions < HandleOptions.Default)
            {
                throw new ArgumentOutOfRangeException(nameof(handleOptions));
            }

            if (0 != (replaceOptions & ~(ReplaceOptions.Character | ReplaceOptions.PinYin | ReplaceOptions.JianPin | ReplaceOptions.Homophone)) || replaceOptions < ReplaceOptions.Character)
            {
                throw new ArgumentOutOfRangeException(nameof(replaceOptions));
            }

            HandleOptions = handleOptions;
            ReplaceOptions = replaceOptions;
        }

        /// <summary>
        /// 敏感词选项
        /// </summary>
        /// <param name="handleOptions">处理选项</param>
        /// <param name="replaceOptions">替换选项</param>
        /// <param name="character">替换字符串</param>
        /// <param name="isIgnoreCase">忽略大小写</param>
        /// <param name="isReplaceSingle">替换为单个不补位</param>
        /// <param name="groupReplaceOptions">正则组替换选项</param>
        /// <param name="isMaxMatch">优先匹配最大长度（正则下不生效）</param>
        /// <param name="whiteSpaceOptions">空字符选项</param>
        public SensitiveWordsOptions(HandleOptions handleOptions, ReplaceOptions replaceOptions, string character, bool isIgnoreCase = false, bool isReplaceSingle = false, GroupReplaceOptions groupReplaceOptions = GroupReplaceOptions.Default, bool isMaxMatch = true, WhiteSpaceOptions whiteSpaceOptions = WhiteSpaceOptions.Default)
            : this(handleOptions, replaceOptions)
        {
            if (0 != (groupReplaceOptions & ~(GroupReplaceOptions.Default | GroupReplaceOptions.GroupOnly | GroupReplaceOptions.GroupPriority)) || groupReplaceOptions < GroupReplaceOptions.Default)
            {
                throw new ArgumentOutOfRangeException(nameof(groupReplaceOptions));
            }

            if (0 != (whiteSpaceOptions & ~(WhiteSpaceOptions.Default | WhiteSpaceOptions.IgnoreWhiteSpace | WhiteSpaceOptions.IgnoreNewLine)) || whiteSpaceOptions < WhiteSpaceOptions.Default)
            {
                throw new ArgumentOutOfRangeException(nameof(whiteSpaceOptions));
            }

            Character = character;
            IsIgnoreCase = isIgnoreCase;
            IsReplaceSingle = isReplaceSingle;
            GroupReplaceOptions = groupReplaceOptions;
            IsMaxMatch = isMaxMatch;
            WhiteSpaceOptions = whiteSpaceOptions;
        }

        /// <summary>
        /// 处理选项
        /// </summary>
        public HandleOptions HandleOptions { get; set; } = HandleOptions.Default;

        /// <summary>
        /// 替换选项
        /// </summary>
        public ReplaceOptions ReplaceOptions { get; set; } = ReplaceOptions.Character;

        /// <summary>
        /// 正则组替换选项
        /// </summary>
        public GroupReplaceOptions GroupReplaceOptions { get; set; } = GroupReplaceOptions.Default;

        /// <summary>
        /// 空字符选项
        /// </summary>
        public WhiteSpaceOptions WhiteSpaceOptions { get; set; } = WhiteSpaceOptions.Default;

        /// <summary>
        /// 替换字符串
        /// </summary>
        public string Character { get; set; } = "*";

        /// <summary>
        /// 是否忽略大小写
        /// </summary>
        public bool IsIgnoreCase
        {
            get => _isIgnoreCase;
            set
            {
                lock (_regexSensitiveWords)
                {
                    ValidRegexAdd(nameof(IsIgnoreCase));

                    _isIgnoreCase = value;
                }
            }
        }

        /// <summary>
        /// 是否替换为单个不补位
        /// </summary>
        public bool IsReplaceSingle { get; set; }

        /// <summary>
        /// 优先匹配最大长度（正则下不生效）
        /// </summary>
        public bool IsMaxMatch { get; set; } = true;

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 正则超时时间
        /// </summary>
        public TimeSpan RegexMatchTimeout
        {
            get => _regexMatchTimeout;
            set
            {
                lock (_regexSensitiveWords)
                {
                    ValidRegexAdd(nameof(RegexMatchTimeout));

                    _regexMatchTimeout = value;
                }
            }
        }

        /// <summary>
        /// 使用 <see cref="RegexOptions.Compiled"/> 编译正则提升性能，在调用 <see cref="Build"/> 时耗时
        /// </summary>
        public bool IsRegexCompiled
        {
            get => _isRegexCompiled;
            set
            {
                lock (_regexSensitiveWords)
                {
                    ValidRegexAdd(nameof(IsRegexCompiled));

                    _isRegexCompiled = value;
                }
            }
        }

        /// <summary>
        /// 添加敏感词，多个英文竖线|分隔，包含竖线需要转义
        /// </summary>
        /// <param name="words"></param>
        /// <param name="isRegex"></param>
        /// <returns></returns>
        private SensitiveWordsOptions Add(string words, bool isRegex)
        {
            if (string.IsNullOrWhiteSpace(words))
            {
                return this;
            }

            ClassificationWords(words, word =>
            {
                if (isRegex ? _regexSensitiveWords.Add(word) : _simpleSensitiveWords.Add(word))
                {
                    _sensitiveWords.Add(word);
                }
            });

            return this;
        }

        /// <summary>
        /// 添加敏感词，多个英文竖线|分隔，包含竖线需要转义
        /// </summary>
        /// <param name="words"></param>
        public SensitiveWordsOptions Add(string words)
        {
            return Add(words, false);
        }

        /// <summary>
        /// 添加正则敏感词，多个英文竖线|分隔，包含竖线需要转义
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public SensitiveWordsOptions AddRegex(string words)
        {
            return Add(words, true);
        }

        /// <summary>
        /// 添加文件敏感词
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isRegex"></param>
        private SensitiveWordsOptions AddFile(string path, bool isRegex)
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

            return Add(File.ReadAllText(path), isRegex);
        }

        /// <summary>
        /// 添加文件敏感词
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public SensitiveWordsOptions AddFile(string path)
        {
            return AddFile(path, false);
        }

        /// <summary>
        /// 添加正则文件敏感词
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public SensitiveWordsOptions AddRegexFile(string path)
        {
            return AddFile(path, true);
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public SensitiveWordsOptions SetTag(string tag)
        {
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));

            return this;
        }

        /// <summary>
        /// 设置处理选项
        /// </summary>
        /// <param name="handleOptions"></param>
        /// <returns></returns>
        public SensitiveWordsOptions SetHandleOptions(HandleOptions handleOptions)
        {
            HandleOptions = handleOptions;

            return this;
        }

        /// <summary>
        /// 设置替换选项
        /// </summary>
        /// <param name="replaceOptions"></param>
        /// <returns></returns>
        public SensitiveWordsOptions SetReplaceOptions(ReplaceOptions replaceOptions)
        {
            ReplaceOptions = replaceOptions;

            return this;
        }

        /// <summary>
        /// 设置正则组替换选项
        /// </summary>
        /// <param name="groupReplaceOptions"></param>
        /// <returns></returns>
        public SensitiveWordsOptions SetGroupReplaceOptions(GroupReplaceOptions groupReplaceOptions)
        {
            GroupReplaceOptions = groupReplaceOptions;

            return this;
        }

        /// <summary>
        /// 设置空字符选项
        /// </summary>
        /// <param name="whiteSpaceOptions"></param>
        /// <returns></returns>
        public SensitiveWordsOptions SetWhiteSpaceOptions(WhiteSpaceOptions whiteSpaceOptions)
        {
            WhiteSpaceOptions = whiteSpaceOptions;

            return this;
        }

        /// <summary>
        /// 设置替换字符串
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public SensitiveWordsOptions SetCharacter(string character)
        {
            Character = character;

            return this;
        }

        /// <summary>
        /// 忽略大小写
        /// </summary>
        /// <returns></returns>
        public SensitiveWordsOptions IgnoreCase()
        {
            IsIgnoreCase = true;

            return this;
        }

        /// <summary>
        /// 替换为单个不补位
        /// </summary>
        /// <returns></returns>
        public SensitiveWordsOptions ReplaceSingle()
        {
            IsReplaceSingle = true;

            return this;
        }

        /// <summary>
        /// 设置是否优先匹配最大长度（正则下不生效）
        /// </summary>
        /// <param name="IsMaxMatch"></param>
        /// <returns></returns>
        public SensitiveWordsOptions SetIsMaxMatch(bool IsMaxMatch)
        {
            this.IsMaxMatch = IsMaxMatch;

            return this;
        }

        /// <summary>
        /// 设置<inheritdoc cref="RegexMatchTimeout"/>
        /// </summary>
        /// <param name="regexMatchTimeout"></param>
        /// <returns></returns>
        public SensitiveWordsOptions SetRegexMatchTimeout(TimeSpan regexMatchTimeout)
        {
            RegexMatchTimeout = regexMatchTimeout;

            return this;
        }

        /// <summary>
        /// 添加正则敏感词前调用,<inheritdoc cref="IsRegexCompiled"/>
        /// </summary>
        /// <returns></returns>
        public SensitiveWordsOptions RegexCompiled()
        {
            IsRegexCompiled = true;

            return this;
        }

        /// <summary>
        /// 构建敏感词，添加删除敏感词后调用生效
        /// </summary>
        /// <returns></returns>
        public SensitiveWordsOptions Build()
        {
            lock (_lock)
            {
                _wordsNodes = WordsNodes.Build(_simpleSensitiveWords, IsIgnoreCase);

                var regexKeys = new ConcurrentBag<RegexKey>();
                if (_regexSensitiveWords.Count > 0)
                {
                    var regexOptions = GetRegexOptions();

                    Parallel.ForEach(_regexSensitiveWords, DesensitizeOptions.Default.GetParallelOptions(), words =>
                    {
                        var regexKey = new RegexKey(this, words, regexOptions, RegexMatchTimeout);

                        regexKey.AddRegex();
                        regexKeys.Add(regexKey);
                    });
                }

                foreach (var regexKey in _regexKeys.Except(regexKeys))
                {
                    regexKey.RemoveRegex();
                }

                _regexKeys = regexKeys.ToArray();
            }

            return this;
        }

        /// <summary>
        /// 删除敏感词
        /// </summary>
        /// <param name="words"></param>
        /// <param name="isRegex"></param>
        /// <returns></returns>
        private bool Remove(string words, bool isRegex)
        {
            bool result = false;
            ClassificationWords(words, word =>
            {
                if (result = isRegex ? _regexSensitiveWords.Remove(word) : _simpleSensitiveWords.Remove(word))
                {
                    _sensitiveWords.Remove(word);
                }
            });

            return result;
        }

        /// <summary>
        /// 删除敏感词，多个英文竖线|分隔，包含竖线需要转义
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public bool Remove(string words)
        {
            return Remove(words, false);
        }

        /// <summary>
        /// 删除正则敏感词，多个英文竖线|分隔，包含竖线需要转义
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public bool RemoveRegex(string words)
        {
            return Remove(words, true);
        }

        /// <summary>
        /// 获取敏感词
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<string> GetSensitiveWords() => _sensitiveWords;

        /// <summary>
        /// 是否存在指定敏感词
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public bool Contains(string words) => _sensitiveWords.Contains(words);

        /// <summary>
        /// Replace
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public string Replace(string value, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (null != _wordsNodes)
                {
                    value = _wordsNodes.Replace(value, MatchEvaluator, IsMaxMatch, WhiteSpaceOptions, cancellationToken);
                }

                if (_regexKeys.Length > 0)
                {
                    foreach (var words in _regexKeys)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Task.Factory.StartNew(() => value = words.Replace(value, MatchEvaluator), cancellationToken).Wait(cancellationToken);
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// 是否匹配上敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public bool IsMatch(string value, DesensitizeOptions options = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (null != _wordsNodes && _wordsNodes.Matches(value, IsMaxMatch, WhiteSpaceOptions, cancellationToken).Count > 0)
                {
                    return true;
                }

                if (_regexKeys.Length > 0)
                {
                    var matchCount = 0;

                    Parallel.ForEach(_regexKeys, (options ?? DesensitizeOptions.Default).GetParallelOptions(cancellationToken), (words, state) =>
                    {
                        if (words.IsMatch(value))
                        {
                            state.Stop();

                            Interlocked.Increment(ref matchCount);
                        }
                    });

                    return matchCount > 0;
                }
            }

            return false;
        }

        /// <summary>
        /// 匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public MatchResult Matches(string value, CancellationToken cancellationToken = default)
        {
            var matchResult = new MatchResult(this);

            if (!string.IsNullOrEmpty(value))
            {
                if (null != _wordsNodes)
                {
                    matchResult.NodeCaptures = _wordsNodes.Matches(value, IsMaxMatch, WhiteSpaceOptions, cancellationToken);
                }

                if (_regexKeys.Length > 0)
                {
                    var regexMatches = new List<Match>();
                    foreach (var words in _regexKeys)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Task.Factory.StartNew(() =>
                        {
                            var matches = words.Matches(value);
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                foreach (Match match in matches)
                                {
                                    regexMatches.Add(match);
                                }
                            }
                        }, cancellationToken).Wait(cancellationToken);
                    }

                    matchResult.RegexMatches = new InternalReadOnlyCollection<Match>(regexMatches);
                }
            }

            return matchResult;
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
        private string GetCharacter(int length) => string.IsNullOrEmpty(Character) ? string.Empty : IsReplaceSingle || Character.Length > 1 ? Character : Character.PadLeft(length, Character[0]);

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
                                    var value = words.AsSpan(position, i - position);
                                    if (!value.IsEmpty)
                                    {
                                        action(value.ToString());
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
                    var value = words.AsSpan(position);
                    if (!value.IsEmpty)
                    {
                        action(value.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// GetRegexOptions
        /// </summary>
        /// <returns></returns>
        private RegexOptions GetRegexOptions()
        {
            var regexOptions = RegexOptions.None;
            if (IsIgnoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            if (IsRegexCompiled)
            {
                regexOptions |= RegexOptions.Compiled;
            }

            return regexOptions;
        }

        /// <summary>
        /// ValidRegexAdd
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ValidRegexAdd(string type)
        {
            if (_regexSensitiveWords.Count > 0)
            {
                throw new InvalidOperationException($"Added regex words cannot be modified property {type}");
            }
        }
    }
}