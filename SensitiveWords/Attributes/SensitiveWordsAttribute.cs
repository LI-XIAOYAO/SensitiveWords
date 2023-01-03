using System;

namespace SensitiveWords
{
    /// <summary>
    /// 指定敏感词标签特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SensitiveWordsAttribute : Attribute
    {
        /// <summary>
        /// 指定敏感词标签特性
        /// </summary>
        /// <param name="tag"></param>
        public SensitiveWordsAttribute(string tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; }
    }
}