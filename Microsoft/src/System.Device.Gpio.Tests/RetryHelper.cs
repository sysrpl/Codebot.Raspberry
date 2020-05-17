﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;

namespace System.Device.Gpio.Tests
{
    public static class RetryHelper
    {
        private static readonly Func<int, int> s_DefaultBackoffFunc = i => Math.Min(i * 100, 60_000);

        /// <summary>
        /// Executes the <paramref name="test"/> action up to a maximum of <paramref name="maxAttempts"/> times.
        /// </summary>
        /// <param name="test">The test to invoke.</param>
        /// <param name="maxAttempts">The maximum number of times to invoke <paramref name="test"/>.</param>
        /// <param name="backoffFunc">After a failure, invoked to determine how many milliseconds to wait before the next attempt. It's passed the number of iterations attempted.</param>
        public static void Execute(Action test, int maxAttempts = 5, Func<int, int> backoffFunc = null)
        {
            if (maxAttempts < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            }

            if (test == null)
            {
                throw new ArgumentNullException(nameof(test));
            }

            // Execute the test until it either passes or we run it maxAttempts times
            var exceptions = new List<Exception>();
            for (int i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    test();
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    if (i == maxAttempts)
                    {
                        throw new AggregateException(exceptions);
                    }
                }

                Thread.Sleep((backoffFunc ?? s_DefaultBackoffFunc)(i));
            }
        }
    }
}
