using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ZBlog.Core.WebAPI.Filter
{
    /// <summary>
    /// 全局路由前缀公约
    /// </summary>
    public class GlobalRoutePrefixFilter : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _centralPrefix;

        public GlobalRoutePrefixFilter(IRouteTemplateProvider routeTemplateProvider)
        {
            this._centralPrefix = new AttributeRouteModel(routeTemplateProvider);
        }

        /// <summary>
        /// 接口的Apply方法
        /// </summary>
        /// <param name="application"></param>
        public void Apply(ApplicationModel application)
        {
            //遍历所有的 Controller
            foreach (var controller in application.Controllers)
            {
                // 已经标记了 RouteAttribute 的 Controller
                var matchedSelectors = controller.Selectors.Where(it => it.AttributeRouteModel != null);
                if (matchedSelectors.Any())
                {
                    foreach (var selectorModel in matchedSelectors)
                    {
                        // 在当前路由上再添加一个路由前缀
                        selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(this._centralPrefix, selectorModel.AttributeRouteModel);
                    }
                }

                // 没有标记 RouteAttribute 的 Controller
                var unmatchedSelectors = controller.Selectors.Where(it => it.AttributeRouteModel == null);
                if (unmatchedSelectors.Any())
                {
                    foreach (var selectorModel in unmatchedSelectors)
                    {
                        // 添加一个 路由前缀
                        selectorModel.AttributeRouteModel = this._centralPrefix
;
                    }
                }
            }
        }
    }
}
