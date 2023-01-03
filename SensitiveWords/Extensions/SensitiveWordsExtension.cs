using Microsoft.AspNetCore.Mvc;
using SensitiveWords;
using SensitiveWords.Filters;
using System;
using System.Collections.Generic;

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
        /// <returns></returns>
        public static IServiceCollection AddSensitiveWords(this IServiceCollection serviceDescriptors, Action<IList<SensitiveWordsOptions>> sensitiveWordsOptions)
        {
            sensitiveWordsOptions?.Invoke(SensitiveWordsResolver.Options);

            serviceDescriptors.Configure<MvcOptions>(mvcOptions => mvcOptions.Filters.Add<SensitiveWordsActionFilter>());

            return serviceDescriptors;
        }
    }
}