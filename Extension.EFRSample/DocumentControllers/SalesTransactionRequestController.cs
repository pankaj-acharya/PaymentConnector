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
    namespace Commerce.HardwareStation.EFRSample
    {
        using System;
        using System.Net.Http;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using Contoso.Commerce.HardwareStation.EFRSample.Constants;
        using Contoso.Commerce.HardwareStation.EFRSample.DocumentSerializers;
        using Contoso.Commerce.Runtime.DocumentProvider.DataModelEFR.Constants;
        using Contoso.Commerce.Runtime.DocumentProvider.DataModelEFR.Documents;
        using Microsoft.Dynamics.Commerce.HardwareStation.PeripheralRequests;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
        using ThrowIf = Microsoft.Dynamics.Commerce.HardwareStation.ThrowIf;

        /// <summary>
        /// Implements sales transaction request methods of EFR.
        /// </summary>
        public static class SalesTransactionRequestController
        {
            /// <summary>
            /// Http client.
            /// </summary>
            private static readonly HttpClient httpClient = new HttpClient();

            /// <summary>
            /// Registers sales transaction in the service.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction to register.</param>
            /// <param name="endPointAddress">The service endpoint address.</param>
            /// <param name="token">The cancellation token.</param>
            /// <returns>The response from printer.</returns>
            public static async Task<string> RegisterAsync(string salesTransaction, string endPointAddress, CancellationToken token)
            {
                string response = await RunPostRequestAsync(
                        endPointAddress + "/" + RequestConstants.Register,
                        salesTransaction, token)
                    .ConfigureAwait(false);
                return response;
            }

            /// <summary>
            /// Parses response from service.
            /// </summary>
            /// <typeparam name="TResponse">The type of the service response.</typeparam>
            /// <param name="responseFromService">The response from service.</param>
            /// <returns>The fiscal device response.</returns>
            public static TResponse ParseResponse<TResponse>(string responseFromService) where TResponse : FiscalDeviceResponseBase
            {
                FiscalPeripheralCommunicationResultType resultType = FiscalPeripheralCommunicationResultType.None;
                FiscalPeripheralFailureDetails failureDetails = new FiscalPeripheralFailureDetails();
                SalesTransactionResponse salesTransactionResponse = null;
                try
                {
                    salesTransactionResponse = SalesTransactionResponseSerializer.Deserialize(responseFromService);
                }
                catch
                {
                    resultType = FiscalPeripheralCommunicationResultType.Failed;
                    failureDetails.IsRetryAllowed = true;
                    failureDetails.FailureType = FiscalPeripheralFailureType.BadResponse;
                }

                if (salesTransactionResponse != null)
                {
                    switch (salesTransactionResponse.RegistrationResult.ResultCode)
                    {
                        case SalesTransactionResponseResultCodeConstants.TransactionProcessedSuccessfully:
                            resultType = FiscalPeripheralCommunicationResultType.Succeeded;
                            failureDetails.IsRetryAllowed = false;
                            failureDetails.FailureType = FiscalPeripheralFailureType.None;
                            break;

                        case SalesTransactionResponseResultCodeConstants.CouldNotProcessTransaction:
                            resultType = FiscalPeripheralCommunicationResultType.Failed;
                            failureDetails.IsRetryAllowed = true;
                            failureDetails.FailureType = FiscalPeripheralFailureType.Other;
                            break;

                        case SalesTransactionResponseResultCodeConstants.InvalidRequestData:
                            resultType = FiscalPeripheralCommunicationResultType.Failed;
                            failureDetails.IsRetryAllowed = false;
                            failureDetails.FailureType = FiscalPeripheralFailureType.Other;
                            break;

                        default:
                            throw new NotSupportedException(string.Format("Result code '{0}' is not supported.", salesTransactionResponse.RegistrationResult.ResultCode));
                    }
                }

                TResponse response = (TResponse)Activator.CreateInstance(
                    typeof(TResponse),
                    new object[] { responseFromService, resultType, failureDetails, string.Empty });

                return response;
            }

            /// <summary>
            /// Runs POST request.
            /// </summary>
            /// <param name="requestUri">The request Uri.</param>
            /// <param name="requestBody">The request body.</param>
            /// <param name="token">The cancellation token.</param>
            /// <returns>The response from the service.</returns>
            private static async Task<string> RunPostRequestAsync(string requestUri, string requestBody, CancellationToken token)
            {
                ThrowIf.NullOrWhiteSpace(requestUri, nameof(requestUri));

                using (StringContent requestContent = new StringContent(requestBody, Encoding.UTF8, "application/xml"))
                {
                    using (HttpResponseMessage httpResponse = await httpClient.PostAsync(requestUri, requestContent, token).ConfigureAwait(false))
                    {
                        string response = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                        return response;
                    }
                }
            }
        }
    }
}