using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SensitiveWords
{
    /// <summary>
    /// 多音字词组
    /// </summary>
    public class HomophoneRegexGroupOptions
    {
        /// <summary>
        /// FullPaths
        /// </summary>
        private static readonly HashSet<string> _fullPaths = new HashSet<string>();

        /// <summary>
        /// HomophoneRegexMaps
        /// </summary>
        internal static Dictionary<string, string> HomophoneRegexMaps { get; } = new Dictionary<string, string>();

        /// <summary>
        /// 多音字词组
        /// </summary>
        public static HomophoneRegexGroupOptions Options => new HomophoneRegexGroupOptions();

        /// <summary>
        /// 添加单个多音字词组
        /// </summary>
        /// <param name="key">拼音</param>
        /// <param name="value">词组，多个英文竖线分隔</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public HomophoneRegexGroupOptions Add(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(nameof(value));
            }

            new Regex(value);

            HomophoneRegexMaps[key.ToUpper()] = value;

            return this;
        }

        /// <summary>
        /// 添加多音字词组
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public HomophoneRegexGroupOptions Add(IDictionary<string, string> keyValuePairs)
        {
            foreach (var item in keyValuePairs)
            {
                Add(item.Key, item.Value);
            }

            return this;
        }

        /// <summary>
        /// 添加多音字文件词组，拼音与词组空格隔开一个一行<br /><br />
        /// eg:<br />
        /// hang 一行|行业|排行<br />
        /// xing 前行|运行|行书<br />
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public HomophoneRegexGroupOptions AddFile(string path)
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

            var text = File.ReadAllLines(path);
            foreach (var line in text)
            {
                var value = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (2 != value.Length)
                {
                    throw new ArgumentException(line);
                }

                Add(value[0].Trim(), value[1].Trim());
            }

            return this;
        }
    }
}