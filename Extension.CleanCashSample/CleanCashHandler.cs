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
        using Microsoft.Dynamics.Commerce.HardwareStation.PeripheralRequests;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Newtonsoft.Json;
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;

        /// <summary>
        /// Fiscal peripheral device handler for CleanCash.
        /// </summary>
        public class CleanCashHandler : INamedRequestHandler
        {
            /// <summary>
            /// Gets name of the handler.
            /// </summary>
            public string HandlerName => "CleanCashSample";

            /// <summary>
            /// Gets supported types.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes => new[]
            {
                typeof(InitializeFiscalDeviceRequest),
                typeof(SubmitDocumentFiscalDeviceRequest),
                typeof(IsReadyFiscalDeviceRequest)
            };

            /// <summary>
            /// Represents an enty point for the request handler.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, nameof(request));

                if (request is InitializeFiscalDeviceRequest)
                {
                    return this.Initialize(request as InitializeFiscalDeviceRequest);
                }
                else if (request is SubmitDocumentFiscalDeviceRequest)
                {
                    return this.SubmitDocument(request as SubmitDocumentFiscalDeviceRequest);
                }
                else if (request is IsReadyFiscalDeviceRequest)
                {
                    return this.IsReady(request as IsReadyFiscalDeviceRequest);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
            }

            /// <summary>
            /// Submits the document.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private Response SubmitDocument(SubmitDocumentFiscalDeviceRequest request)
            {
                ThrowIf.NullOrWhiteSpace(request.Document, nameof(request.Document));
                ThrowIf.Null(request.PeripheralInfo, nameof(request.PeripheralInfo));
                ThrowIf.Null(request.PeripheralInfo.DeviceName, nameof(request.PeripheralInfo.DeviceName));
                ThrowIf.Null(request.PeripheralInfo.DeviceProperties, nameof(request.PeripheralInfo.DeviceName));

                SubmitDocumentFiscalDeviceResponse response;

                try
                {
                    var fiscalTransactionData = JsonConvert.DeserializeObject<CleanCashFiscalTransactionData>(request.Document, new JsonSerializerSettings() { Formatting = Formatting.Indented });
                    var connectionString = ConfigurationController.GetConnectionStringValue(request.PeripheralInfo.DeviceProperties);
                    var timeout = ConfigurationController.GetTimeoutValue(request.PeripheralInfo.DeviceProperties);
                    CleanCashResponse registerResponse = null;
                    CleanCashLockContainer.Execute(() =>
                    {
                        using (var register = new CleanCashFiscalRegister(connectionString))
                        {
                            registerResponse = register.RegisterFiscalTransaction(fiscalTransactionData);
                        }
                    }, timeout);
                    
                    response = new SubmitDocumentFiscalDeviceResponse(registerResponse.ControlCode, FiscalPeripheralCommunicationResultType.Succeeded, new FiscalPeripheralFailureDetails(), registerResponse.DeviceId);
                }
                catch (TimeoutException)
                {
                    FiscalPeripheralFailureDetails failureDetails = new FiscalPeripheralFailureDetails
                    {
                        IsRetryAllowed = true,
                        FailureType = FiscalPeripheralFailureType.Timeout
                    };
                    response = new SubmitDocumentFiscalDeviceResponse(string.Empty, FiscalPeripheralCommunicationResultType.Failed, failureDetails, string.Empty);
                }
                catch (CleanCashDeviceException ex)
                {
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when opening a fiscal register.", ex);
                    FiscalPeripheralFailureDetails failureDetails = new FiscalPeripheralFailureDetails
                    {
                        ErrorCode = ex.ResultCode.ToString(),
                        IsRetryAllowed = true,
                        FailureType = ErrorCodesMapper.MapToFiscalPeripheralFailureType(ex.ResultCode)
                    };
                    response = new SubmitDocumentFiscalDeviceResponse(string.Empty, FiscalPeripheralCommunicationResultType.Failed, failureDetails, string.Empty);
                }
                return response;
            }

            /// <summary>
            /// Initializes service.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private Response Initialize(InitializeFiscalDeviceRequest request)
            {
                ThrowIf.Null(request, nameof(request));

                InitializeFiscalDeviceResponse response;

                response = new InitializeFiscalDeviceResponse(string.Empty, FiscalPeripheralCommunicationResultType.None, new FiscalPeripheralFailureDetails(), string.Empty);

                return response;
            }

            /// <summary>
            /// Checks if printer is available.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private Response IsReady(IsReadyFiscalDeviceRequest request)
            {
                ThrowIf.Null(request.PeripheralInfo, $"{nameof(request)}.{nameof(request.PeripheralInfo)}");
                ThrowIf.NullOrWhiteSpace(request.PeripheralInfo.DeviceName, $"{nameof(request)}.{nameof(request.PeripheralInfo)}.{nameof(request.PeripheralInfo.DeviceName)}");
                ThrowIf.NullOrWhiteSpace(request.PeripheralInfo.DeviceProperties, $"{nameof(request)}.{nameof(request.PeripheralInfo)}.{nameof(request.PeripheralInfo.DeviceProperties)}");

                var response = new IsReadyFiscalDeviceResponse(false);

                try
                {
                    var connectionString = ConfigurationController.GetConnectionStringValue(request.PeripheralInfo.DeviceProperties);
                    var timeout = ConfigurationController.GetTimeoutValue(request.PeripheralInfo.DeviceProperties);
                    bool isReady = false;
                    CleanCashLockContainer.Execute(() =>
                    {
                        using (var register = new CleanCashFiscalRegister(connectionString))
                        {
                            isReady = register.IsReady();
                        }
                    }, timeout);

                    response = new IsReadyFiscalDeviceResponse(isReady);
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when opening a fiscal register.", ex);
                }

                return response;
            }
        }
    }
}