﻿using Autofac;
using Microsoft.AspNetCore.Mvc;

namespace ZBlog.Core.WebAPI.Filter
{
    public class AutofacPropertityModuleReg : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // 记得要启动服务注册
            // builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            var controllerBaseType = typeof(ControllerBase);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(it => controllerBaseType.IsAssignableFrom(it) && it != controllerBaseType)
                .PropertiesAutowired();
        }
    }
}
