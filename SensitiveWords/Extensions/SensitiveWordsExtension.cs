using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
        /// <param name="isAddFilter">是否添加输入输出脱敏<br/><br/> <see cref="HandleOptions.Input"/> <see cref="ActionExecutingContext"/><br/><see cref="HandleOptions.Output"/> <see cref="ActionExecutedContext"/> </param>
        /// <param name="filterOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddSensitiveWords(this IServiceCollection serviceDescriptors, Action<SensitiveWordsOptionsCollection> sensitiveWordsOptions, bool isAddFilter = true, DesensitizeOptions filterOptions = null)
        {
            sensitiveWordsOptions?.Invoke(SensitiveWordsResolver.Options);

            if (isAddFilter)
            {
                SensitiveWordsActionFilter.ParallelOptions = (filterOptions ?? DesensitizeOptions.Default).GetParallelOptions();
                serviceDescriptors.Configure<MvcOptions>(mvcOptions => mvcOptions.Filters.Add<SensitiveWordsActionFilter>());
            }

            return serviceDescriptors;
        }
    }
}