namespace Contoso
{
    namespace Commerce.HardwareStation.PDFPrinterSample
    {
        using System.Runtime.Serialization;

        /// <summary>
        /// Print PDF request class.
        /// </summary>
        [DataContract]
        public class PrintPDFRequest
        {
            /// <summary>
            /// Gets or sets the encoded binary file.
            /// </summary>
            [DataMember]
            public string EncodedBinary { get; private set; }

            /// <summary>
            /// Gets or sets the device name.
            /// </summary>
            [DataMember]
            public string DeviceName { get; set; }
        }
    }
}