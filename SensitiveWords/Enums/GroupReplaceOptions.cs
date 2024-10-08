﻿namespace SensitiveWords
{
    /// <summary>
    /// 正则组替换选项
    /// </summary>
    public enum GroupReplaceOptions
    {
        /// <summary>
        /// 默认，替换匹配
        /// </summary>
        Default = 1,

        /// <summary>
        /// 仅替换组
        /// </summary>
        GroupOnly = 2,

        /// <summary>
        /// 组优先
        /// </summary>
        GroupPriority = 4
    }
}