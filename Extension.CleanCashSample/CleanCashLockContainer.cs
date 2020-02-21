﻿/**
* SAMPLE CODE NOTICE
* 
* THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
* OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
* THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
* NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

namespace Contoso
{
    namespace Commerce.HardwareStation.CleanCashSample
    {
        using System;
        using System.Threading;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Contains locks used to access a device.
        /// </summary>
        internal static class CleanCashLockContainer
        {
            /// <summary>
            /// Lock to use while accessing a device.
            /// </summary>
            private static readonly object AsyncLock = new object();

            /// <summary>
            ///  Executes an Action, limiting the number of threads that can execute function at the same time.
            /// </summary>
            /// <param name="action">Action to run.</param>
            public static void Execute(Action action, int millisecondsTimeout)
            {
                ThrowIf.Null(action, nameof(action));

                Guid correlationId = Guid.NewGuid();
                bool lockTaken = false;
                try
                {
                    RetailLogger.Log.HardwareStationAcquiringDeviceLock(correlationId);
                    Monitor.TryEnter(AsyncLock, millisecondsTimeout, ref lockTaken);
                    if (lockTaken)
                    {
                        action();
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(AsyncLock);
                    }

                    RetailLogger.Log.HardwareStationExitingDeviceLockExecute(correlationId);
                }
            }
        }
    }
}
