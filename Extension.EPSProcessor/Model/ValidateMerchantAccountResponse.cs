namespace GridServe.Hardware.EPS.Extension.PaymentProcessor
{
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;

    /// <summary>
    /// Response for <see cref="ValidateMerchantAccountRequest"/>.
    /// </summary>
    /// <seealso cref="Microsoft.Dynamics.Retail.SampleConnector.Portable.ResponseBase" />
    internal class ValidateMerchantAccountResponse : ResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateMerchantAccountResponse"/> class.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="serviceAccountId">The service account identifier.</param>
        /// <param name="connectorName">Name of the connector.</param>
        internal ValidateMerchantAccountResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }

        /// <summary>
        /// Converts <see cref="ValidateMerchantAccountResponse"/> to <see cref="Response"/>.
        /// </summary>
        /// <param name="validateResponse">The validate response.</param>
        /// <returns>An instance of <see cref="Response"/>.</returns>
        internal static Response ConvertTo(ValidateMerchantAccountResponse validateResponse)
        {
            var response = new Response();
            validateResponse.WriteBaseProperties(response);
            return response;
        }
    }
}
