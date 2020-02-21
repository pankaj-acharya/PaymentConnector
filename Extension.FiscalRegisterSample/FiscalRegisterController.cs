namespace Contoso
{
    namespace Commerce.HardwareStation.FiscalRegisterSample
    {
        using System;
        using System.Composition;
        using System.Threading.Tasks;
        using System.Web.Http;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Fiscal register peripheral web API controller class.
        /// </summary>
        [Export("FISCALREGISTER", typeof(IHardwareStationController))]
        [Authorize]
        public class FiscalRegisterController : HardwareStationController, IHardwareStationController
        {
            /// <summary>
            /// Checks if the fiscal register is ready for registration operation or not.
            /// </summary>
            /// <returns><c>True</c> if fiscal register is active, <c>false</c> otherwise.</returns>
            public Task<bool> IsReady()
            {
                try
                {
                    IsReadyFiscalRegisterDeviceRequest fiscalRegisterIsReadyRequest = new IsReadyFiscalRegisterDeviceRequest();
                    IsReadyFiscalRegisterDeviceResponse isReadyResponse = this.CommerceRuntime.Execute<IsReadyFiscalRegisterDeviceResponse>(fiscalRegisterIsReadyRequest, null);

                    return Task.FromResult(isReadyResponse.IsReady);
                }
                catch (FiscalRegisterException ex)
                {
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when opening a fiscal register.", ex);
                    throw;
                }
                catch (Exception ex)
                {
                    // Rethrow exception setting of errorResourceId to raise localized general error and logging unlocalized details.
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when opening a fiscal register.", ex);
                    throw new FiscalRegisterException(FiscalRegisterException.FiscalRegisterErrorResourceId, ex.Message, ex);
                }
            }

            /// <summary>
            /// Registers the specified fiscal transaction.
            /// </summary>
            /// <param name="request">The fiscal registration request.</param>
            /// <returns>Result of fiscal registration.</returns>
            /// <exception cref="FiscalRegisterException">A device exception.</exception>
            [HttpPost]
            public Task<FiscalRegistrationResults> RegisterFiscalTransaction(FiscalRegistrationRequest request)
            {
                ThrowIf.Null(request, "request");

                try
                {
                    RegisterFiscalTransactionFiscalRegisterDeviceRequest registerFiscalTransactionRequest = new RegisterFiscalTransactionFiscalRegisterDeviceRequest(request.FiscalTransaction, request.Configuration);
                    RegisterFiscalTransactionFiscalRegisterDeviceResponse registerResponse = this.CommerceRuntime.Execute<RegisterFiscalTransactionFiscalRegisterDeviceResponse>(registerFiscalTransactionRequest, null);

                    return Task.FromResult(registerResponse.FiscalRegistrationResults);
                }
                catch (FiscalRegisterException ex)
                {
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when opening a fiscal register.", ex);
                    throw;
                }
                catch (Exception ex)
                {
                    // Rethrow exception setting of errorResourceId to raise localized general error and logging unlocalized details.
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when opening a fiscal register.", ex);
                    throw new FiscalRegisterException(FiscalRegisterException.FiscalRegisterErrorResourceId, ex.Message, ex);
                }
            }
        }
    }
}
