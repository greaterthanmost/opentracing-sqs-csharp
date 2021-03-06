﻿using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTracing.Util;

namespace EventBus.Sqs.Tracing.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventBusBuilder"></param>
        /// <returns></returns>
        public static IEventBusBuilder AddOpenTracing(this IEventBusBuilder eventBusBuilder)
        {
            var services = eventBusBuilder.Services;

            /**
            if (!GlobalTracer.IsRegistered())
                throw new NullReferenceException("The GlobalTracer is not registered. <null>");*/

            var eventBusDescriptor = services.First(s => s.ServiceType == typeof(IEventBus));

            var serviceProvider = services.BuildServiceProvider();

            services.Replace(ServiceDescriptor.Scoped<IEventBus>(locator =>
            {
                var eventBus = (IEventBus)(eventBusDescriptor?.ImplementationInstance ??
                                ActivatorUtilities.GetServiceOrCreateInstance(locator, eventBusDescriptor.ImplementationType));

                return new EventBusTracing(eventBus, GlobalTracer.Instance);

            }));

            return eventBusBuilder;
        }
    }
}
