using System;
using System.Collections;
using System.Collections.Generic;

namespace SensitiveWords
{
    /// <summary>
    /// 只读集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InternalReadOnlyCollection<T> : IReadOnlyCollection<T>, IReadOnlyList<T>
    {
        private readonly IList<T> _list = new List<T>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected internal InternalReadOnlyCollection()
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="list"></param>
        protected internal InternalReadOnlyCollection(IList<T> list)
        {
            _list = list;
        }

        /// <summary>
        /// 根据索引获取值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index] => _list[index];

        /// <summary>
        /// 获取集合元素数
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected internal virtual void Add(T item)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _list.Add(item);
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected internal virtual void Remove(T item)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _list.Remove(item);
        }

        /// <summary>
        /// 移除指定索引元素
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected internal virtual void RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _list.RemoveAt(index);
        }

        /// <summary>
        /// 移除所以元素
        /// </summary>
        protected internal virtual void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{nameof(Count)} = {Count}";
    }
}