using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenRace.ServicesConfigs
{
    public static class ConfigureInvalidStateCustomResponseExtension
    {
        public static void ConfigureInvalidStateCustomResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = HandleInvalidModelStateResponse;
            });
        }

        private static IActionResult HandleInvalidModelStateResponse(ActionContext context)
        {
            var ms = new MemoryStream();
            context.HttpContext.Request.Body.CopyTo(ms);
            var requestString = Encoding.UTF8.GetString(ms.ToArray());
            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var errors = context.ModelState.Where(it => it.Value.Errors.Any()).ToArray();
            
            var logger = loggerFactory.CreateLogger("Invalid Model");
            logger.LogWarning("Model state errors: {@Errors}. Request: {Request}", 
                errors, requestString);
            var errorsJson = JsonSerializer.Serialize(errors, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            var result = new BadRequestObjectResult(
                $"Invalid Model State Response:\n{errorsJson}\n\nRequest:\n{requestString}");
            return result;
        }
    }
}