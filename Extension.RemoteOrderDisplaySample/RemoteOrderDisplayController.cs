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
    namespace Commerce.HardwareStation.RemoteOrderDisplaySample
    {
        using System;
        using System.Composition;
        using System.Web.Http;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Remote Order Display web API controller class.
        /// </summary>
        [Export("REMOTEORDERDISPLAY", typeof(IHardwareStationController))]
        public class RemoteOrderDisplayController : HardwareStationController, IHardwareStationController
        {
            private const string RemoteOrderDisplayDeviceName = "MockOPOSRemoteOrderDisplay";

            /// <summary>
            /// Display a message in the remote order display device.
            /// </summary>
            /// <param name="request">The remote order display request.</param>
            /// <returns>Returns true if the change operation succeeds.</returns>
            public bool DisplayMessage(RemoteOrderDisplayRequest request)
            {
                ThrowIf.Null(request, "request");

                string deviceName = request.DeviceName;

                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    deviceName = RemoteOrderDisplayController.RemoteOrderDisplayDeviceName;
                }

                try
                {
                    var openRemoteOrderDisplayDeviceRequest = new OpenRemoteOrderDisplayDeviceRequest(deviceName, null);
                    this.CommerceRuntime.Execute<NullResponse>(openRemoteOrderDisplayDeviceRequest, null);

                    var displayMessageRemoteOrderDisplayDeviceRequest = new DisplayMessageRemoteOrderDisplayDeviceRequest(request.MessageListAsStringArray);
                    this.CommerceRuntime.Execute<NullResponse>(displayMessageRemoteOrderDisplayDeviceRequest, null);

                    return true;
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when operating on remote order display device.", ex);

                    throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_RemoteOrderDisplay_Error", ex.Message, ex);
                }
                finally
                {
                    var closeRemoteOrderDisplayDeviceRequest = new CloseRemoteOrderDisplayDeviceRequest();
                    this.CommerceRuntime.Execute<NullResponse>(closeRemoteOrderDisplayDeviceRequest, null);
                }
            }
        }
    }
}