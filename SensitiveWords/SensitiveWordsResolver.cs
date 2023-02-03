using System;
using System.Collections;
using System.Linq;
using System.Reflection;

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
            groupOptions?.Invoke(HomophoneRegexGroupOptions.Instance);
        }

        /// <summary>
        /// 默认脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Desensitize(this string value) => value.Desensitize(HandleOptions.Default);

        /// <summary>
        /// 输入脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DesensitizeInput(this string value) => value.Desensitize(HandleOptions.Input);

        /// <summary>
        /// 输出脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DesensitizeOutput(this string value) => value.Desensitize(HandleOptions.Output);

        /// <summary>
        /// 脱敏所有
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DesensitizeAll(this string value) => value.Desensitize(HandleOptions.Default | HandleOptions.Input | HandleOptions.Output);

        /// <summary>
        /// 指定类型脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string Desensitize(this string value, HandleOptions handleOptions) => value.Desensitize(handleOptions, null);

        /// <summary>
        /// 指定类型标签脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string Desensitize(this string value, HandleOptions handleOptions, string tag)
        {
            if (string.IsNullOrEmpty(value) || !Options.Any())
            {
                return value;
            }

            if (handleOptions < HandleOptions.Default || handleOptions > (HandleOptions.Default | HandleOptions.Input | HandleOptions.Output))
            {
                throw new ArgumentOutOfRangeException(nameof(handleOptions));
            }

            foreach (var sensitiveWordsOptions in Options)
            {
                if ((handleOptions & sensitiveWordsOptions.HandleOptions) > 0 && (null == tag || sensitiveWordsOptions.Tag == tag))
                {
                    value = sensitiveWordsOptions.Replace(value);
                }
            }

            return value;
        }

        /// <summary>
        /// 指定选项配置脱敏
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <returns></returns>
        public static string Desensitize(this string value, SensitiveWordsOptions sensitiveWordsOptions)
        {
            if (null == sensitiveWordsOptions)
            {
                throw new ArgumentNullException(nameof(sensitiveWordsOptions));
            }

            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return sensitiveWordsOptions.Replace(value);
        }

        /// <summary>
        /// 默认脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Desensitize<T>(this T value)
            where T : class => value.Desensitize(HandleOptions.Default);

        /// <summary>
        /// 输入脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DesensitizeInput<T>(this T value)
            where T : class => value.Desensitize(HandleOptions.Input);

        /// <summary>
        /// 输出脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DesensitizeOutput<T>(this T value)
            where T : class => value.Desensitize(HandleOptions.Output);

        /// <summary>
        /// 脱敏所有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DesensitizeAll<T>(this T value)
            where T : class => value.Desensitize(HandleOptions.Default | HandleOptions.Input | HandleOptions.Output);

        /// <summary>
        /// 指定类型脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <returns></returns>
        public static T Desensitize<T>(this T value, HandleOptions handleOptions)
            where T : class
        {
            if (null == value || !Options.Any())
            {
                return value;
            }

            if (handleOptions < HandleOptions.Default || handleOptions > (HandleOptions.Default | HandleOptions.Input | HandleOptions.Output))
            {
                throw new ArgumentOutOfRangeException(nameof(handleOptions));
            }

            return value.Desensitize(new Stack(), (v, t) => v.Desensitize(handleOptions, t));
        }

        /// <summary>
        /// 指定选项脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Desensitize<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions)
            where T : class
        {
            if (null == sensitiveWordsOptions)
            {
                throw new ArgumentNullException(nameof(sensitiveWordsOptions));
            }

            if (null == value || !Options.Any())
            {
                return value;
            }

            return value.Desensitize(new Stack(), (v, t) => v.Desensitize(sensitiveWordsOptions));
        }

        /// <summary>
        /// 脱敏
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="stack"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private static T Desensitize<T>(this T value, Stack stack, Func<string, string, string> func)
            where T : class
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

            if (typeof(string) == type)
            {
                return func(value.ToString(), type.IsDefined(typeof(SensitiveWordsAttribute)) ? type.GetCustomAttribute<SensitiveWordsAttribute>().Tag : null) as T;
            }
            else if (type.IsClass)
            {
                stack.Push(value);

                if (value is IDictionary dic)
                {
                    foreach (DictionaryEntry item in dic)
                    {
                        if (null != item.Value)
                        {
                            dic[item.Key] = item.Value.Desensitize(stack, func);
                        }
                    }
                }
                else if (value is IList list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (null != list[i])
                        {
                            list[i] = list[i].Desensitize(stack, func);
                        }
                    }
                }
                else
                {
                    foreach (var propertyInfo in type.GetProperties())
                    {
                        if (!propertyInfo.CanRead || (typeof(string) != propertyInfo.PropertyType && !propertyInfo.PropertyType.IsClass) || propertyInfo.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                        {
                            continue;
                        }

                        var propValue = propertyInfo.GetValue(value);
                        if (null != propValue)
                        {
                            if (typeof(string) == propertyInfo.PropertyType)
                            {
                                if (propertyInfo.CanWrite && !string.IsNullOrEmpty(propValue.ToString()))
                                {
                                    propertyInfo.SetValue(value, func(propValue.ToString(), propertyInfo.IsDefined(typeof(SensitiveWordsAttribute)) ? propertyInfo.GetCustomAttribute<SensitiveWordsAttribute>().Tag : null));
                                }
                            }
                            else if (propertyInfo.PropertyType.IsClass)
                            {
                                propValue.Desensitize(stack, func);
                            }
                        }
                    }
                }

                stack.Pop();
            }

            return value;
        }

        /// <summary>
        /// 是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsSensitiveWords(this string value)
        {
            return ContainsSensitiveWords(value, HandleOptions.Default | HandleOptions.Input | HandleOptions.Output);
        }

        /// <summary>
        /// 指定类型是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool ContainsSensitiveWords(this string value, HandleOptions handleOptions) => value.ContainsSensitiveWords(handleOptions, null);

        /// <summary>
        /// 指定类型标签是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool ContainsSensitiveWords(this string value, HandleOptions handleOptions, string tag)
        {
            if (handleOptions < HandleOptions.Default || handleOptions > (HandleOptions.Default | HandleOptions.Input | HandleOptions.Output))
            {
                throw new ArgumentOutOfRangeException(nameof(handleOptions));
            }

            if (string.IsNullOrEmpty(value) || !Options.Any())
            {
                return false;
            }

            foreach (var sensitiveWordsOptions in Options)
            {
                if ((handleOptions & sensitiveWordsOptions.HandleOptions) > 0 && (null == tag || sensitiveWordsOptions.Tag == tag))
                {
                    if (sensitiveWordsOptions.IsMatch(value))
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
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ContainsSensitiveWords(this string value, SensitiveWordsOptions sensitiveWordsOptions)
        {
            if (null == sensitiveWordsOptions)
            {
                throw new ArgumentNullException(nameof(sensitiveWordsOptions));
            }

            if (string.IsNullOrEmpty(value) || !Options.Any())
            {
                return false;
            }

            return sensitiveWordsOptions.IsMatch(value);
        }

        /// <summary>
        /// 是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsSensitiveWords<T>(this T value)
            where T : class
        {
            return ContainsSensitiveWords(value, HandleOptions.Default | HandleOptions.Input | HandleOptions.Output);
        }

        /// <summary>
        /// 指定类型是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="handleOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, HandleOptions handleOptions)
            where T : class
        {
            if (handleOptions < HandleOptions.Default || handleOptions > (HandleOptions.Default | HandleOptions.Input | HandleOptions.Output))
            {
                throw new ArgumentOutOfRangeException(nameof(handleOptions));
            }

            if (null == value || !Options.Any())
            {
                return false;
            }

            return ContainsSensitiveWords(value, new Stack(), (v, t) => v.ContainsSensitiveWords(handleOptions, t));
        }

        /// <summary>
        /// 指定选项是否包含敏感词
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ContainsSensitiveWords<T>(this T value, SensitiveWordsOptions sensitiveWordsOptions)
            where T : class
        {
            if (null == sensitiveWordsOptions)
            {
                throw new ArgumentNullException(nameof(sensitiveWordsOptions));
            }

            if (null == value || !Options.Any())
            {
                return false;
            }

            return ContainsSensitiveWords(value, new Stack(), (v, t) => v.ContainsSensitiveWords(sensitiveWordsOptions));
        }

        /// <summary>
        /// 是否包含敏感词
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="stack"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private static bool ContainsSensitiveWords<T>(this T value, Stack stack, Func<string, string, bool> func)
            where T : class
        {
            if (null == value || !Options.Any() || stack.Contains(value))
            {
                return false;
            }

            var type = value.GetType();
            if (type.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
            {
                return false;
            }

            if (typeof(string) == type)
            {
                return func(value.ToString(), type.IsDefined(typeof(SensitiveWordsAttribute)) ? type.GetCustomAttribute<SensitiveWordsAttribute>().Tag : null);
            }
            else if (type.IsClass)
            {
                stack.Push(value);

                if (value is IDictionary dic)
                {
                    foreach (DictionaryEntry item in dic)
                    {
                        if (null != item.Value)
                        {
                            if (item.Value.ContainsSensitiveWords(stack, func))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (value is IList list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (null != list[i])
                        {
                            if (list[i].ContainsSensitiveWords(stack, func))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var propertyInfo in type.GetProperties())
                    {
                        if (!propertyInfo.CanRead || (typeof(string) != propertyInfo.PropertyType && !propertyInfo.PropertyType.IsClass) || propertyInfo.IsDefined(typeof(IgnoreSensitiveWordsAttribute)))
                        {
                            continue;
                        }

                        var propValue = propertyInfo.GetValue(value);
                        if (null != propValue)
                        {
                            if (typeof(string) == propertyInfo.PropertyType)
                            {
                                if (propertyInfo.CanWrite && !string.IsNullOrEmpty(propValue.ToString()))
                                {
                                    if (func(propValue.ToString(), propertyInfo.IsDefined(typeof(SensitiveWordsAttribute)) ? propertyInfo.GetCustomAttribute<SensitiveWordsAttribute>().Tag : null))
                                    {
                                        return true;
                                    }
                                }
                            }
                            else if (propertyInfo.PropertyType.IsClass)
                            {
                                if (propValue.ContainsSensitiveWords(stack, func))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                stack.Pop();
            }

            return false;
        }
    }
}