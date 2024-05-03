using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SensitiveWords
{
    /// <summary>
    /// 只读字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class InternalReadOnlyDictionary<TKey, TValue> : ReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// 通过 <paramref name="key"/> 获取值，不存在返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new TValue this[TKey key] => Dictionary.TryGetValue(key, out var value) ? value : default;

        /// <summary>
        /// 只读字典
        /// </summary>
        /// <param name="comparer"></param>
        protected internal InternalReadOnlyDictionary(IEqualityComparer<TKey> comparer = null)
            : base(new Dictionary<TKey, TValue>(comparer))
        {
        }

        /// <summary>
        /// <inheritdoc cref="IDictionary{TKey, TValue}.Add(TKey, TValue)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected internal virtual void Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
        }

        /// <summary>
        /// <inheritdoc cref="IDictionary{TKey, TValue}.Remove(TKey)"/>
        /// </summary>
        /// <param name="key"></param>
        protected internal virtual bool Remove(TKey key)
        {
            return Dictionary.Remove(key);
        }

        /// <summary>
        /// Clear
        /// </summary>
        protected internal virtual void Clear()
        {
            Dictionary.Clear();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{nameof(Count)} = {Count}";
    }
}