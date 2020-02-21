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
    namespace Commerce.HardwareStation.EFRSample.DocumentSerializers
    {
        using System.IO;
        using System.Xml;
        using System.Xml.Serialization;
        using Contoso.Commerce.Runtime.DocumentProvider.DataModelEFR.Documents;
        using Microsoft.Dynamics.Commerce.HardwareStation;

        /// <summary>
        /// Serializes the sales transaction response document.
        /// </summary>
        public static class SalesTransactionResponseSerializer
        {
            /// <summary>
            /// Deserializes response string to sales transaction response document.
            /// </summary>
            /// <param name="responseString">The response string.</param>
            /// <returns>The sales transaction response document.</returns>
            public static SalesTransactionResponse Deserialize(string responseString)
            {
                ThrowIf.NullOrWhiteSpace(responseString, nameof(responseString));

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SalesTransactionResponse));

                using (StringReader stringReader = new StringReader(responseString))
                {
                    using (XmlReader xmlReader = XmlReader.Create(stringReader))
                    {
                        return (SalesTransactionResponse)xmlSerializer.Deserialize(xmlReader);
                    }
                }
            }
        }
    }
}
