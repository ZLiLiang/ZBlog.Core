using Microsoft.Extensions.DependencyInjection;
using ZBlog.Core.Common;
using ZBlog.Core.Common.Option.Core;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    public static class AllOptionRegister
    {
        public static void AddAllOptionRegister(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            foreach (var optionType in App.EffectiveTypes.Where(s => !s.IsInterface && typeof(IConfigurableOptions).IsAssignableFrom(s)))
            {
                services.AddConfigurableOptions(optionType);
            }
        }
    }
}
