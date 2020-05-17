﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;

namespace DeviceApiTester.Infrastructure
{
    public static class DriverFactory
    {
        public static InstanceType CreateFromEnum<InstanceType, EnumType>(EnumType driver, params object[] parameters)
            where InstanceType : class
        {
            try
            {
                ImplementationTypeAttribute creatorAttribute = typeof(EnumType)
                    .GetMember(driver.ToString())?[0]
                    .GetCustomAttributes(typeof(ImplementationTypeAttribute), false)
                    .OfType<ImplementationTypeAttribute>()
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException($"The {typeof(EnumType).Name}.{driver} enum value is not attributed with an {nameof(ImplementationTypeAttribute)}.");

                return creatorAttribute.ImplementationType == null
                    ? null
                    : (InstanceType)Activator.CreateInstance(creatorAttribute.ImplementationType, parameters);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }
    }
}
