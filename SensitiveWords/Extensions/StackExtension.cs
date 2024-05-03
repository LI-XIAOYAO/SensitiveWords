using System.Collections;

namespace SensitiveWords
{
    /// <summary>
    /// StackExtension
    /// </summary>
    internal static class StackExtension
    {
        /// <summary>
        /// <inheritdoc cref="Stack.Push(object)"/>
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool TryAdd(this Stack stack, object obj)
        {
            if (!stack.Contains(obj))
            {
                lock (stack.SyncRoot)
                {
                    if (stack.Contains(obj))
                    {
                        return false;
                    }

                    stack.Push(obj);

                    return true;
                }
            }

            return false;
        }
    }
}