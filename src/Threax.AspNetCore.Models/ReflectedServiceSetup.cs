﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Threax.AspNetCore.Models
{
    public static class ReflectedServiceSetup
    {
        public static void ConfigureReflectedServices(this IServiceCollection services, Assembly assembly)
        {
            var setupType = typeof(IServiceSetup).GetTypeInfo();
            var types = assembly.GetTypes().Where(i =>
            {
                var typeInfo = i.GetTypeInfo();
                return setupType.IsAssignableFrom(i) && !typeInfo.IsAbstract && !typeInfo.IsInterface;
            });
            foreach (var type in types)
            {
                IServiceSetup instance = null;

                try
                {
                    instance = (IServiceSetup)Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                    //this handles any errors creating a type, not really a big deal, could be an abstract class or something else, just ignore it
                    //Probably should make this better later
                }

                instance?.ConfigureServices(services);
            }
        }
    }
}
