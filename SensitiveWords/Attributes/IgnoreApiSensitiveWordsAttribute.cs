using System;

namespace SensitiveWords
{
    /// <summary>
    /// 敏感词忽略特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class IgnoreApiSensitiveWordsAttribute : Attribute
    {
        /// <summary>
        /// 处理选项
        /// </summary>
        public HandleOptions Options { get; set; } = HandleOptions.Input | HandleOptions.Output;
    }
}