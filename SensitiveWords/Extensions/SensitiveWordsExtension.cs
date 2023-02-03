using Microsoft.AspNetCore.Mvc;
using SensitiveWords;
using SensitiveWords.Filters;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// DI扩展
    /// </summary>
    public static class SensitiveWordsExtension
    {
        /// <summary>
        /// 添加脱敏配置
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="isAddFilter"></param>
        /// <returns></returns>
        public static IServiceCollection AddSensitiveWords(this IServiceCollection serviceDescriptors, Action<SensitiveWordsOptionsCollection> sensitiveWordsOptions, bool isAddFilter = true)
        {
            sensitiveWordsOptions?.Invoke(SensitiveWordsResolver.Options);

            if (isAddFilter)
            {
                serviceDescriptors.Configure<MvcOptions>(mvcOptions => mvcOptions.Filters.Add<SensitiveWordsActionFilter>());
            }

            return serviceDescriptors;
        }
    }
}