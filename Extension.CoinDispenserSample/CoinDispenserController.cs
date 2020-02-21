/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */
 
namespace Contoso
{
    namespace Commerce.HardwareStation.CoinDispenserSample
    {
        using System;
        using System.Composition;
        using System.Web.Http;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Coin dispenser web API controller class.
        /// </summary>
        [Export("COINDISPENSER", typeof(IHardwareStationController))]
        public class CoinDispenserController : HardwareStationController, IHardwareStationController
        {
            private const string CoinDispenserTestName = "MockOPOSCoinDispenser";

            /// <summary>
            /// Collect the change in the coin dispenser.
            /// </summary>
            /// <param name="request">The coin dispenser request value.</param>
            /// <returns>Returns true fi the change operation succeeds.</returns>
            public bool DispenseChange(CoinDispenserRequest request)
            {
                ThrowIf.Null(request, "request");

                string deviceName = request.DeviceName;

                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    deviceName = CoinDispenserController.CoinDispenserTestName;
                }

                try
                {
                    var openCoinDispenserDeviceRequest = new OpenCoinDispenserDeviceRequest(deviceName, null);
                    this.CommerceRuntime.Execute<NullResponse>(openCoinDispenserDeviceRequest, null);

                    var dispenseChangeCoinDispenserDeviceRequest = new DispenseChangeCoinDispenserDeviceRequest(request.Amount);
                    this.CommerceRuntime.Execute<NullResponse>(dispenseChangeCoinDispenserDeviceRequest, null);

                    return true;
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when operating on coin dispenser.", ex);

                    throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_CoinDispenser_Error", ex.Message, ex);
                }
                finally
                {
                    var closeCoinDispenserDeviceRequest = new CloseCoinDispenserDeviceRequest();
                    this.CommerceRuntime.Execute<NullResponse>(closeCoinDispenserDeviceRequest, null);
                }
            }
        }
    }
}
