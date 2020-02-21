namespace GridServe.Hardware.EPS.Extension.PaymentProcessor
{
        using System;

        /// <summary>
        /// Represents a miscellaneous charge.
        /// </summary>
        internal class MiscellaneousCharge
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MiscellaneousCharge"/> class.
            /// </summary>
            internal MiscellaneousCharge()
            {
            }

            /// <summary>
            /// Gets or sets the type of the charge.
            /// </summary>
            /// <value>
            /// The type of the charge.
            /// </value>
            internal string ChargeType { get; set; }

            /// <summary>
            /// Gets or sets the charge amount.
            /// </summary>
            /// <value>
            /// The charge amount.
            /// </value>
            internal decimal? ChargeAmount { get; set; }
        }
    }
