using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SensitiveWords
{
    /// <summary>
    /// 敏感词解析器
    /// </summary>
    public static class SensitiveWordsResolver
    {
        /// <summary>
        /// 敏感词处理选项
        /// </summary>
        public static SensitiveWordsOptionsCollection Options { get; } = new SensitiveWordsOptionsCollection();

        /// <summary>
        /// 配置敏感词处理选项
        /// </summary>
        /// <param name="sensitiveWordsOptions"></param>
        public static void Config(Action<SensitiveWordsOptionsCollection> sensitiveWordsOptions)
        {
            sensitiveWordsOptions?.Invoke(Options);
        }

        /// <summary>
        /// 注册多音字词组
        /// </summary>
        /// <param name="groupOptions"></param>
        public static void RegisterHomophoneRegexWordGroup(Action<HomophoneRegexGroupOptions> groupOptions)
        {
            groupOptions?.Invoke(HomophoneRegexGroupOptions.Options);
        }

        /// <summary>
        /// 指定类型标签脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static object Desensitize(this string value, HandleOptions handleOptions, string tag, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            foreach (var sensitiveWordsOptions in Options)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((handleOptions & sensitiveWordsOptions.HandleOptions) > 0 && (null == tag || sensitiveWordsOptions.Tag == tag))
                {
                    value = sensitiveWordsOptions.Replace(value, cancellationToken);
                }
            }

            return value;
        }

        /// <summary>
        /// 指定选项配置脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static object Desensitize(this string value, SensitiveWordsOptions sensitiveWordsOptions, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return sensitiveWordsOptions.Replace(value, cancellationToken);
        }

        /// <summary>
        /// 默认脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, CancellationToken cancellationToken = default) => value.Desensitize(options: null, cancellationToken);

        /// <summary>
        /// 默认脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.Desensitize(HandleOptions.Default, options, cancellationToken);

        /// <summary>
        /// 输入脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T DesensitizeInput<T>(this T value, CancellationToken cancellationToken = default) => value.DesensitizeInput(options: null, cancellationToken);

        /// <summary>
        /// 输入脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T DesensitizeInput<T>(this T value, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.Desensitize(HandleOptions.Input, options, cancellationToken);

        /// <summary>
        /// 输出脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T DesensitizeOutput<T>(this T value, CancellationToken cancellationToken = default) => value.DesensitizeOutput(null, cancellationToken);

        /// <summary>
        /// 输出脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T DesensitizeOutput<T>(this T value, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.Desensitize(HandleOptions.Output, options, cancellationToken);

        /// <summary>
        /// 脱敏所有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T DesensitizeAll<T>(this T value, CancellationToken cancellationToken = default) => value.DesensitizeAll(null, cancellationToken);

        /// <summary>
        /// 脱敏所有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static T DesensitizeAll<T>(this T value, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.Desensitize(HandleOptions.Default | HandleOptions.Input | HandleOptions.Output, options, cancellationToken);

        /// <summary>
        /// 指定类型脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, HandleOptions handleOptions, CancellationToken cancellationToken = default) => value.Desensitize(handleOptions, options: null, cancellationToken);

        /// <summary>
        /// 指定类型脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, HandleOptions handleOptions, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.Desensitize(handleOptions, null, options, cancellationToken);

        /// <summary>
        /// 指定类型标签脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, HandleOptions handleOptions, string tag, CancellationToken cancellationToken = default) => value.Desensitize(handleOptions, tag, null, cancellationToken);

        /// <summary>
        /// 指定类型标签脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, HandleOptions handleOptions, string tag, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (null == value || 0 == Options.Count)
            {
                return value;
            }

            handleOptions.Valid();

            if (value is string str)
            {
                return (T)str.Desensitize(handleOptions, tag, cancellationToken);
            }

            var stack = Stack.Synchronized(new Stack());
            value = value.Desensitize(stack, (v, t) => v.Desensitize(handleOptions, t ?? tag, cancellationToken), (options ?? DesensitizeOptions.Default).GetParallelOptions(cancellationToken));

            stack.Clear();

            return value;
        }

        /// <summary>
        /// 指定选项脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions, CancellationToken cancellationToken = default) => value.Desensitize(sensitiveWordsOptions, null, cancellationToken);

        /// <summary>
        /// 指定选项脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static T Desensitize<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (sensitiveWordsOptions is null)
            {
                throw new ArgumentNullException(nameof(sensitiveWordsOptions));
            }

            if (null == value || 0 == Options.Count)
            {
                return value;
            }

            if (value is string str)
            {
                return (T)str.Desensitize(sensitiveWordsOptions, cancellationToken);
            }

            var stack = Stack.Synchronized(new Stack());
            value = value.Desensitize(stack, (v, t) => v.Desensitize(sensitiveWordsOptions, cancellationToken), (options ?? DesensitizeOptions.Default).GetParallelOptions(cancellationToken));

            stack.Clear();

            return value;
        }

        /// <summary>
        /// 脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="stack"></param>
        /// <param name="func"><![CDATA[Func<Value, Tag, Return>]]></param>
        /// <param name="parallelOptions"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static T Desensitize<T>(this T value, Stack stack, Func<string, string, object> func, ParallelOptions parallelOptions)
        {
            if (null == value || stack.Contains(value))
            {
                return value;
            }

            var type = value.GetType();
            if (type.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
            {
                return value;
            }

            if (value is string str)
            {
                return (T)func(str, null);
            }
            else if (type.IsClass)
            {
                if (!stack.TryAdd(value))
                {
                    return value;
                }

                if (value is IDictionary dic)
                {
                    Parallel.ForEach(dic.Keys.Cast<object>(), parallelOptions, key =>
                    {
                        var current = dic[key];
                        if (null != current)
                        {
                            if (current is string val)
                            {
                                if (!dic.IsReadOnly && val.Length > 0)
                                {
                                    dic[key] = func(val, null);
                                }
                            }
                            else
                            {
                                current.Desensitize(stack, func, parallelOptions);
                            }
                        }
                    });
                }
                else if (value is IEnumerable enumerable)
                {
                    var (IntIndex, LongIndex) = type.GetIndexProperty();

                    Parallel.ForEach(enumerable.Cast<object>(), parallelOptions, (item, _, i) =>
                    {
                        if (null != item)
                        {
                            if (item is string val)
                            {
                                if (val.Length > 0)
                                {
                                    (null != LongIndex || i > int.MaxValue ? LongIndex : IntIndex)?.SetValue(value, func(val, null), new object[] { i });
                                }
                            }
                            else
                            {
                                item.Desensitize(stack, func, parallelOptions);
                            }
                        }
                    });
                }
                else
                {
                    Parallel.ForEach(type.GetProperties(), parallelOptions, propertyInfo =>
                    {
                        if (!propertyInfo.CanRead || !(propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsStruct()) || propertyInfo.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                        {
                            return;
                        }

                        var propValue = propertyInfo.GetValue(value);
                        if (null != propValue)
                        {
                            if (propValue is string val)
                            {
                                if (propertyInfo.CanWrite && val.Length > 0)
                                {
                                    propertyInfo.SetValue(value, func(val, propertyInfo.IsDefined(typeof(SensitiveWordsAttribute)) ? propertyInfo.GetCustomAttribute<SensitiveWordsAttribute>().Tag : null));
                                }
                            }
                            else
                            {
                                propValue.Desensitize(stack, func, parallelOptions);
                            }
                        }
                    });
                }
            }
            else if (type.IsStruct())
            {
                Parallel.ForEach(type.GetProperties(), parallelOptions, prop =>
                {
                    if (!prop.CanRead || !prop.PropertyType.IsClass || typeof(string) == prop.PropertyType || prop.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                    {
                        return;
                    }

                    prop.GetValue(value)?.Desensitize(stack, func, parallelOptions);
                });
            }

            return value;
        }

        /// <summary>
        /// 指定类型标签是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static bool ContainsSensitiveWords(this string value, HandleOptions handleOptions, string tag, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (var sensitiveWordsOptions in Options)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((handleOptions & sensitiveWordsOptions.HandleOptions) > 0 && (null == tag || sensitiveWordsOptions.Tag == tag))
                {
                    if (sensitiveWordsOptions.IsMatch(value, options, cancellationToken))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 指定选项是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static bool ContainsSensitiveWords(this string value, SensitiveWordsOptions sensitiveWordsOptions, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return sensitiveWordsOptions.IsMatch(value, options, cancellationToken);
        }

        /// <summary>
        /// 是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, CancellationToken cancellationToken = default) => value.ContainsSensitiveWords(options: null, cancellationToken);

        /// <summary>
        /// 是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.ContainsSensitiveWords(HandleOptions.Default | HandleOptions.Input | HandleOptions.Output, options, cancellationToken);

        /// <summary>
        /// 指定类型是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, HandleOptions handleOptions, CancellationToken cancellationToken = default) => value.ContainsSensitiveWords(handleOptions, options: null, cancellationToken);

        /// <summary>
        /// 指定类型是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, HandleOptions handleOptions, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.ContainsSensitiveWords(handleOptions, null, options, cancellationToken);

        /// <summary>
        /// 指定类型标签是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, HandleOptions handleOptions, string tag, CancellationToken cancellationToken = default) => value.ContainsSensitiveWords(handleOptions, tag, null, cancellationToken);

        /// <summary>
        /// 指定类型标签是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, HandleOptions handleOptions, string tag, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (null == value || 0 == Options.Count)
            {
                return false;
            }

            handleOptions.Valid();

            if (value is string str)
            {
                return str.ContainsSensitiveWords(handleOptions, tag, options, cancellationToken);
            }

            var stack = Stack.Synchronized(new Stack());
            var contains = ContainsSensitiveWords(value, stack, (v, t) => v.ContainsSensitiveWords(handleOptions, t ?? tag, options, cancellationToken), (options ?? DesensitizeOptions.Default).GetParallelOptions(cancellationToken));

            stack.Clear();

            return contains;
        }

        /// <summary>
        /// 指定选项是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions, CancellationToken cancellationToken = default) => value.ContainsSensitiveWords(sensitiveWordsOptions, null, cancellationToken);

        /// <summary>
        /// 指定选项是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (sensitiveWordsOptions is null)
            {
                throw new ArgumentNullException(nameof(sensitiveWordsOptions));
            }

            if (null == value || 0 == Options.Count)
            {
                return false;
            }

            if (value is string str)
            {
                return str.ContainsSensitiveWords(sensitiveWordsOptions, options, cancellationToken);
            }

            var stack = Stack.Synchronized(new Stack());
            var contains = ContainsSensitiveWords(value, stack, (v, t) => v.ContainsSensitiveWords(sensitiveWordsOptions, options, cancellationToken), (options ?? DesensitizeOptions.Default).GetParallelOptions(cancellationToken));

            stack.Clear();

            return contains;
        }

        /// <summary>
        /// 是否包含敏感词
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="stack"></param>
        /// <param name="func"><![CDATA[Func<Value, Tag, Return>]]></param>
        /// <param name="parallelOptions"></param>
        /// <param name="matchCount"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static bool ContainsSensitiveWords<T>(this T value, Stack stack, Func<string, string, bool> func, ParallelOptions parallelOptions, int matchCount = 0)
        {
            if (matchCount > 0)
            {
                return true;
            }

            if (null == value || stack.Contains(value))
            {
                return false;
            }

            var type = value.GetType();
            if (type.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
            {
                return false;
            }

            void Matched(ParallelLoopState state)
            {
                state.Stop();

                Interlocked.Increment(ref matchCount);
            }

            if (value is string str)
            {
                return func(str, null);
            }
            else if (type.IsClass)
            {
                if (!stack.TryAdd(value))
                {
                    return false;
                }

                if (value is IDictionary dic)
                {
                    Parallel.ForEach(dic.Keys.Cast<object>(), parallelOptions, (key, state) =>
                    {
                        if (0 == matchCount)
                        {
                            var current = dic[key];
                            if (null != current)
                            {
                                if (current is string val)
                                {
                                    if (!dic.IsReadOnly && val.Length > 0 && func(val, null))
                                    {
                                        Matched(state);
                                    }
                                }
                                else
                                {
                                    if (current.ContainsSensitiveWords(stack, func, parallelOptions, matchCount))
                                    {
                                        Matched(state);
                                    }
                                }
                            }
                        }
                    });
                }
                else if (value is IEnumerable enumerable)
                {
                    var (IntIndex, LongIndex) = type.GetIndexProperty();

                    Parallel.ForEach(enumerable.Cast<object>(), parallelOptions, (item, state, i) =>
                    {
                        if (0 == matchCount && null != item)
                        {
                            if (item is string val)
                            {
                                if (val.Length > 0 && (null != (null != LongIndex || i > int.MaxValue ? LongIndex : IntIndex)) && func(val, null))
                                {
                                    Matched(state);
                                }
                            }
                            else if (item.ContainsSensitiveWords(stack, func, parallelOptions, matchCount))
                            {
                                Matched(state);
                            }
                        }
                    });
                }
                else
                {
                    Parallel.ForEach(type.GetProperties(), parallelOptions, (prop, state) =>
                    {
                        if (matchCount > 0 || !prop.CanRead || !(prop.PropertyType.IsClass || prop.PropertyType.IsStruct()) || prop.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                        {
                            return;
                        }

                        var propValue = prop.GetValue(value);
                        if (null != propValue)
                        {
                            if (propValue is string val)
                            {
                                if (prop.CanWrite && val.Length > 0 && func(val, prop.IsDefined(typeof(SensitiveWordsAttribute)) ? prop.GetCustomAttribute<SensitiveWordsAttribute>().Tag : null))
                                {
                                    Matched(state);
                                }
                            }
                            else if (propValue.ContainsSensitiveWords(stack, func, parallelOptions, matchCount))
                            {
                                Matched(state);
                            }
                        }
                    });
                }
            }
            else if (type.IsStruct())
            {
                Parallel.ForEach(type.GetProperties(), parallelOptions, (prop, state) =>
                {
                    if (matchCount > 0 || !prop.CanRead || !prop.PropertyType.IsClass || typeof(string) == prop.PropertyType || prop.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                    {
                        return;
                    }

                    var propValue = prop.GetValue(value);
                    if (null != propValue && propValue.ContainsSensitiveWords(stack, func, parallelOptions, matchCount))
                    {
                        Matched(state);
                    }
                });
            }

            return matchCount > 0;
        }

        /// <summary>
        /// 指定类型标签匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static MatchResultCollection MatcheSensitiveWords(this string value, HandleOptions handleOptions, string tag, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new MatchResultCollection();
            }

            var matches = new ConcurrentBag<MatchResult>();
            foreach (var sensitiveWordsOptions in Options)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((handleOptions & sensitiveWordsOptions.HandleOptions) > 0 && (null == tag || sensitiveWordsOptions.Tag == tag))
                {
                    var current = sensitiveWordsOptions.Matches(value, cancellationToken);
                    if (current.IsMatch)
                    {
                        matches.Add(current);
                    }
                }
            }

            return new MatchResultCollection(matches.ToList());
        }

        /// <summary>
        /// 指定选项匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static MatchResultCollection MatcheSensitiveWords(this string value, SensitiveWordsOptions sensitiveWordsOptions, CancellationToken cancellationToken = default)
        {
            var matches = new MatchResultCollection();
            if (string.IsNullOrEmpty(value))
            {
                return matches;
            }

            var current = sensitiveWordsOptions.Matches(value, cancellationToken);
            if (current.IsMatch)
            {
                matches.Add(current);
            }

            return matches;
        }

        /// <summary>
        /// 匹配含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, CancellationToken cancellationToken = default) => value.MatcheSensitiveWords(options: null, cancellationToken);

        /// <summary>
        /// 匹配含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.MatcheSensitiveWords(HandleOptions.Default | HandleOptions.Input | HandleOptions.Output, options, cancellationToken);

        /// <summary>
        /// 指定类型匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, HandleOptions handleOptions, CancellationToken cancellationToken = default) => value.MatcheSensitiveWords(handleOptions, options: null, cancellationToken);

        /// <summary>
        /// 指定类型匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, HandleOptions handleOptions, DesensitizeOptions options, CancellationToken cancellationToken = default) => value.MatcheSensitiveWords(handleOptions, null, options, cancellationToken);

        /// <summary>
        /// 指定类型标签匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, HandleOptions handleOptions, string tag, CancellationToken cancellationToken = default) => value.MatcheSensitiveWords(handleOptions, tag, null, cancellationToken);

        /// <summary>
        /// 指定类型标签匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, HandleOptions handleOptions, string tag, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (null == value || 0 == Options.Count)
            {
                return new MatchResultCollection();
            }

            handleOptions.Valid();

            if (value is string str)
            {
                return str.MatcheSensitiveWords(handleOptions, tag, cancellationToken);
            }

            var matches = new ConcurrentBag<MatchResult>();
            var stack = Stack.Synchronized(new Stack());
            MatcheSensitiveWords(value, stack, (v, t) => v.MatcheSensitiveWords(handleOptions, t ?? tag, cancellationToken), matches, (options ?? DesensitizeOptions.Default).GetParallelOptions(cancellationToken));

            stack.Clear();

            return new MatchResultCollection(matches.ToList());
        }

        /// <summary>
        /// 指定选项匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions, CancellationToken cancellationToken = default) => value.MatcheSensitiveWords(sensitiveWordsOptions, null, cancellationToken);

        /// <summary>
        /// 指定选项匹配敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public static MatchResultCollection MatcheSensitiveWords<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions, DesensitizeOptions options, CancellationToken cancellationToken = default)
        {
            if (sensitiveWordsOptions is null)
            {
                throw new ArgumentNullException(nameof(sensitiveWordsOptions));
            }

            if (null == value || 0 == Options.Count)
            {
                return new MatchResultCollection();
            }

            if (value is string str)
            {
                return str.MatcheSensitiveWords(sensitiveWordsOptions, cancellationToken);
            }

            var matches = new ConcurrentBag<MatchResult>();
            var stack = Stack.Synchronized(new Stack());
            MatcheSensitiveWords(value, stack, (v, t) => v.MatcheSensitiveWords(sensitiveWordsOptions, cancellationToken), matches, (options ?? DesensitizeOptions.Default).GetParallelOptions(cancellationToken));

            stack.Clear();

            return new MatchResultCollection(matches.ToList());
        }

        /// <summary>
        /// 匹配敏感词
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="stack"></param>
        /// <param name="func"><![CDATA[Func<Value, Tag, Return>]]></param>
        /// <param name="matches"></param>
        /// <param name="parallelOptions"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private static void MatcheSensitiveWords<T>(this T value, Stack stack, Func<string, string, MatchResultCollection> func, ConcurrentBag<MatchResult> matches, ParallelOptions parallelOptions)
        {
            if (null == value || stack.Contains(value))
            {
                return;
            }

            var type = value.GetType();
            if (type.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
            {
                return;
            }

            void Matched(MatchResultCollection matchResults)
            {
                foreach (var item in matchResults)
                {
                    matches.Add(item);
                }
            }

            if (value is string str)
            {
                Matched(func(str, null));

                return;
            }
            else if (type.IsClass)
            {
                if (!stack.TryAdd(value))
                {
                    return;
                }

                if (value is IDictionary dic)
                {
                    Parallel.ForEach(dic.Keys.Cast<object>(), parallelOptions, key =>
                    {
                        var current = dic[key];
                        if (null != current)
                        {
                            if (current is string val)
                            {
                                if (!dic.IsReadOnly && val.Length > 0)
                                {
                                    Matched(func(val, null));
                                }
                            }
                            else
                            {
                                current.MatcheSensitiveWords(stack, func, matches, parallelOptions);
                            }
                        }
                    });
                }
                else if (value is IEnumerable enumerable)
                {
                    var (IntIndex, LongIndex) = type.GetIndexProperty();

                    Parallel.ForEach(enumerable.Cast<object>(), parallelOptions, (item, _, i) =>
                    {
                        if (null != item)
                        {
                            if (item is string val)
                            {
                                if (val.Length > 0 && null != (null != LongIndex || i > int.MaxValue ? LongIndex : IntIndex))
                                {
                                    Matched(func(val, null));
                                }
                            }
                            else
                            {
                                item.MatcheSensitiveWords(stack, func, matches, parallelOptions);
                            }
                        }
                    });
                }
                else
                {
                    Parallel.ForEach(type.GetProperties(), parallelOptions, propertyInfo =>
                    {
                        if (!propertyInfo.CanRead || !(propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsStruct()) || propertyInfo.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                        {
                            return;
                        }

                        var propValue = propertyInfo.GetValue(value);
                        if (null != propValue)
                        {
                            if (propValue is string val)
                            {
                                if (propertyInfo.CanWrite && val.Length > 0)
                                {
                                    Matched(func(val, propertyInfo.IsDefined(typeof(SensitiveWordsAttribute)) ? propertyInfo.GetCustomAttribute<SensitiveWordsAttribute>().Tag : null));
                                }
                            }
                            else
                            {
                                propValue.MatcheSensitiveWords(stack, func, matches, parallelOptions);
                            }
                        }
                    });
                }
            }
            else if (type.IsStruct())
            {
                Parallel.ForEach(type.GetProperties(), parallelOptions, prop =>
                {
                    if (!prop.CanRead || !prop.PropertyType.IsClass || typeof(string) == prop.PropertyType || prop.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                    {
                        return;
                    }

                    prop.GetValue(value)?.MatcheSensitiveWords(stack, func, matches, parallelOptions);
                });
            }

            return;
        }

        /// <summary>
        /// Valid
        /// </summary>
        /// <param name="handleOptions"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void Valid(this HandleOptions handleOptions)
        {
            if (0 != (handleOptions & ~(HandleOptions.Default | HandleOptions.Input | HandleOptions.Output)) || handleOptions < HandleOptions.Default)
            {
                throw new ArgumentOutOfRangeException(nameof(handleOptions));
            }
        }
    }
}