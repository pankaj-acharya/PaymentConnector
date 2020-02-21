namespace Contoso
{
    namespace Commerce.HardwareStation.PDFPrinterSample
    {
        using System;
        using System.Composition;
        using System.Web.Http;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// PDF printer web API controller class.
        /// </summary>
        [Export("PDFPRINTER", typeof(IHardwareStationController))]
        public class PDFPrinterController : ApiController, IHardwareStationController
        {

            /// <summary>
            /// Prints the content.
            /// </summary>
            /// <param name="printRequest">The print request.</param>
            /// <exception cref="System.Web.Http.HttpResponseException">Exception thrown when an error occurs.</exception>
            public void Print(PrintPDFRequest printRequest)
            {
                ThrowIf.Null(printRequest, "printRequests");

                try
                {
                    byte[] receivedFile = Convert.FromBase64String(printRequest.EncodedBinary);

                    // Add here the code to send the PDF file to the printer.
                }
                catch (PeripheralException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when trying to open a printer and print.", ex);
                    throw new PeripheralException(PeripheralException.PrinterError, ex.Message, ex);
                }
            }
        }
    }
}
