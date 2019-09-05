using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XSSDefenses.XSSDefenses.Builder;
using XSSDefenses.XSSDefenses.MiddlerWare;

namespace XSSDefenses.XSSDefenses.Extensions
{
    public static class CspMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsp(
             this IApplicationBuilder app, Action<CspOptionsBuilder> builder)
        {
            var newBuilder = new CspOptionsBuilder();
            builder(newBuilder);

            var options = newBuilder.Build();
            return app.UseMiddleware<CspOptionMiddlerWare>(options);
        }
    }
}
