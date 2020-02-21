
namespace Hardware.Extension.PaymentProcessor
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using GridServe.Hardware.EPS.Extension.PaymentProcessor;

    public class EPSProcessor : IPaymentProcessor
    {
        #region Contants

        private const string Platform = "Portable";

        #endregion
        public string Copyright
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                return "EPS_PaymentProcessor";
            }
        }

        public ArrayList SupportedCountries
        {
            get
            {
                return new ArrayList();
            }
        }

        public Response ActivateGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response Authorize(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response BalanceOnGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response Capture(Request request)
        {
            throw new NotImplementedException();
        }

        public Response GenerateCardToken(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response GetMerchantAccountPropertyMetadata(Request request)
        {
            string methodName = "GetMerchantAccountPropertyMetadata";

            // Check null request
            List<PaymentError> errors = new List<PaymentError>();
            if (request == null)
            {
                errors.Add(new PaymentError(ErrorCode.InvalidRequest, "Request is null."));
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: null, properties: null, errors: errors);
            }

            // Prepare response
            List<PaymentProperty> properties = new List<PaymentProperty>();
            PaymentProperty property;
            property = new PaymentProperty(
                GenericNamespace.MerchantAccount,
                MerchantAccountProperties.AssemblyName,
                this.GetAssemblyName());
            property.SetMetadata("Assembly Name:", "The assembly name of the test provider", false, true, 0);
            properties.Add(property);

            Response response = new Response();
            response.Locale = request.Locale;
            response.Properties = properties.ToArray();
            if (errors.Count > 0)
            {
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        public Response GetPaymentAcceptPoint(Request request)
        {
            throw new NotImplementedException();
        }

        public Response ImmediateCapture(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response LoadGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response Reauthorize(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response Refund(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            throw new NotImplementedException();
        }

        public Response RetrievePaymentAcceptResult(Request request)
        {
            throw new NotImplementedException();
        }

        public Response Reversal(Request request)
        {
            throw new NotImplementedException();
        }

        public Response ValidateMerchantAccount(Request request)
        {
            string methodName = "ValidateMerchantAccount";

            // Convert request
            ValidateMerchantAccountRequest validateRequest = null;
            try
            {
                validateRequest = ValidateMerchantAccountRequest.ConvertFrom(request);
            }
            catch (SampleException ex)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            ValidateMerchantProperties(validateRequest, errors);
            if (errors.Count > 0)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, validateRequest.Locale, errors);
            }

            // Create response
            var validateResponse = new ValidateMerchantAccountResponse(validateRequest.Locale, validateRequest.ServiceAccountId, this.Name);

            // Convert response and return
            Response response = ValidateMerchantAccountResponse.ConvertTo(validateResponse);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        public Response Void(Request request)
        {
            throw new NotImplementedException();
        }

        #region PrivateMethods
        private string GetAssemblyName()
        {
            string asemblyQualifiedName = this.GetType().AssemblyQualifiedName;
            int commaIndex = asemblyQualifiedName.IndexOf(',');
            return asemblyQualifiedName.Substring(commaIndex + 1).Trim();
        }


        /// <summary>
        /// Validates merchant account properties.
        /// </summary>
        /// <param name="requestBase">The request.</param>
        /// <param name="errors">The errors.</param>
        private static void ValidateMerchantProperties(RequestBase requestBase, List<PaymentError> errors)
        {
            /*
             IMPORTANT!!!
             THIS IS SAMPLE CODE ONLY!
             THE CODE SHOULD BE UPDATED TO VALIDATE MERCHANT FROM THE APPROPRIATE PAYMENT PROVIDERS.
            */
            if (!TestData.MerchantId.ToString("D").Equals(requestBase.MerchantId, StringComparison.InvariantCulture))
            {
                errors.Add(new PaymentError(ErrorCode.InvalidMerchantProperty, string.Format("Invalid property value for {0}", MerchantAccountProperties.MerchantId)));
            }

            if (!TestData.ProviderId.ToString("D").Equals(requestBase.ProviderId, StringComparison.InvariantCulture))
            {
                errors.Add(new PaymentError(ErrorCode.InvalidMerchantProperty, string.Format("Invalid property value for {0}", SampleMerchantAccountProperty.ProviderId)));
            }

            if (!TestData.TestString.Equals(requestBase.TestString, StringComparison.InvariantCulture))
            {
                errors.Add(new PaymentError(ErrorCode.InvalidMerchantProperty, string.Format("Invalid property value for {0}", SampleMerchantAccountProperty.TestString)));
            }

            if (requestBase.TestDecimal != TestData.TestDecimal)
            {
                errors.Add(new PaymentError(ErrorCode.InvalidMerchantProperty, string.Format("Invalid property value for {0}", SampleMerchantAccountProperty.TestDecimal)));
            }

            if (requestBase.TestDate != TestData.TestDate)
            {
                errors.Add(new PaymentError(ErrorCode.InvalidMerchantProperty, string.Format("Invalid property value for {0}", SampleMerchantAccountProperty.TestDate)));
            }
        }
        #endregion
    }
}