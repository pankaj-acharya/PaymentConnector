namespace GridServe.Hardware.EPS.Extension.PaymentProcessor
{
        using System;

        /// <summary>
        /// Represents tax details.
        /// </summary>
        internal class TaxDetail
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaxDetail"/> class.
            /// </summary>
            internal TaxDetail()
            {
            }

            /// <summary>
            /// Gets or sets the tax type identifier.
            /// </summary>
            /// <value>
            /// The tax type identifier.
            /// </value>
            internal string TaxTypeIdentifier { get; set; }

            /// <summary>
            /// Gets or sets the tax rate.
            /// </summary>
            /// <value>
            /// The tax rate.
            /// </value>
            internal decimal? TaxRate { get; set; }

            /// <summary>
            /// Gets or sets the tax description.
            /// </summary>
            /// <value>
            /// The tax description.
            /// </value>
            internal string TaxDescription { get; set; }

            /// <summary>
            /// Gets or sets the tax amount.
            /// </summary>
            /// <value>
            /// The tax amount.
            /// </value>
            internal decimal? TaxAmount { get; set; }
        }
    }
