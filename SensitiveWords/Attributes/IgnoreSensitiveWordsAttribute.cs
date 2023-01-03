using System;

namespace SensitiveWords
{
    /// <summary>
    /// 敏感词忽略特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class IgnoreSensitiveWordsAttribute : Attribute
    {
    }
}