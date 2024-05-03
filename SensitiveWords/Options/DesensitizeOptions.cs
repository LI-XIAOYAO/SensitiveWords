using System;
using System.Threading;
using System.Threading.Tasks;

namespace SensitiveWords
{
    /// <summary>
    /// 脱敏选项
    /// </summary>
    public class DesensitizeOptions
    {
        /// <summary>
        /// 默认并行配置选项
        /// </summary>
        private static ParallelOptions DefaultParallelOptions { get; } = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };

        /// <summary>
        /// Default
        /// </summary>
        internal static DesensitizeOptions Default => new DesensitizeOptions
        {
            TaskScheduler = DefaultParallelOptions.TaskScheduler,
            MaxDegreeOfParallelism = DefaultParallelOptions.MaxDegreeOfParallelism
        };

        /// <summary>
        /// <inheritdoc cref="ParallelOptions.MaxDegreeOfParallelism"/> Default '<see cref="Environment.ProcessorCount"/> - 1'
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = DefaultParallelOptions.MaxDegreeOfParallelism;

        /// <summary>
        /// <inheritdoc cref="ParallelOptions.TaskScheduler"/>
        /// </summary>
        public TaskScheduler TaskScheduler { get; set; } = DefaultParallelOptions.TaskScheduler;

        /// <summary>
        /// GetParallelOptions
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ParallelOptions GetParallelOptions(CancellationToken cancellationToken = default)
        {
            return new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                TaskScheduler = TaskScheduler
            };
        }
    }
}