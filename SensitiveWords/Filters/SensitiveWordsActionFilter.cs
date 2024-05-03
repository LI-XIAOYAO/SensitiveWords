using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SensitiveWords.Filters
{
    /// <summary>
    /// 脱敏过滤器
    /// </summary>
    internal class SensitiveWordsActionFilter : IAsyncActionFilter
    {
        public static ParallelOptions ParallelOptions { get; set; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Input
            SensitiveWordsHandler(context);

            // Output
            SensitiveWordsHandler(await next());
        }

        /// <summary>
        /// 入参处理
        /// </summary>
        /// <param name="context"></param>
        private static void SensitiveWordsHandler(ActionExecutingContext context)
        {
            if (context.ActionArguments.Count > 0 && SensitiveWordsResolver.Options[HandleOptions.Input].Any())
            {
                Parallel.ForEach(context.ActionArguments, ParallelOptions, (arg, _, i) =>
                {
                    if (!(context.ActionDescriptor.Parameters[(int)i] is ControllerParameterDescriptor cpd && cpd.ParameterInfo.IsDefined(typeof(IgnoreSensitiveWordsAttribute))))
                    {
                        context.ActionArguments[arg.Key] = arg.Value.DesensitizeInput();
                    }
                });
            }
        }

        /// <summary>
        /// 返回值处理
        /// </summary>
        /// <param name="context"></param>
        private static void SensitiveWordsHandler(ActionExecutedContext context)
        {
            if (SensitiveWordsResolver.Options[HandleOptions.Output].Any())
            {
                if (context.Result is ContentResult contentResult)
                {
                    contentResult.Content = contentResult.Content.DesensitizeOutput();
                }
                else if (context.Result is JsonResult jsonResult)
                {
                    jsonResult.Value = jsonResult.Value.DesensitizeOutput();
                }
                else if (context.Result is ObjectResult objectResult)
                {
                    objectResult.Value = objectResult.Value.DesensitizeOutput();
                }
            }
        }
    }
}