﻿namespace Contoso
{
    namespace Commerce.HardwareStation.LocalizationSample
    {
        using System.Composition;
        using System.Reflection;
        using Microsoft.Dynamics.Commerce.HardwareStation.Localization;

        /// <summary>
        /// The Hardware Station exception localization helper class.
        /// </summary>
        [Export("HARDWARESTATIONEXCEPTIONLOCALIZER", typeof(IHardwareStationLocalizer))]
        public class HardwareStationExceptionLocalizerSample : HardwareStationLocalizer
        {
            /// <summary>
            /// Exception localizable message resource file name.
            /// </summary>
            private static readonly string ExceptionResourceFileName = string.Format("{0}.{1}", typeof(HardwareStationExceptionLocalizerSample).Namespace, "Properties.Exception.HardwareStationExceptionMessages");

            /// <summary>
            /// Initializes a new instance of the <see cref="HardwareStationExceptionLocalizerSample"/> class with runtime default localization resource.
            /// </summary>
            public HardwareStationExceptionLocalizerSample()
                : base(ExceptionResourceFileName, typeof(HardwareStationExceptionLocalizerSample).GetTypeInfo().Assembly)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HardwareStationExceptionLocalizerSample"/> class with base resource file name and current assembly.
            /// </summary>
            /// <param name="baseResourceFileName">The base resource file full name.</param>
            /// <param name="baseResourceFileAssembly">The assembly that contains base resource file.</param>
            public HardwareStationExceptionLocalizerSample(string baseResourceFileName, Assembly baseResourceFileAssembly)
                : base(baseResourceFileName, baseResourceFileAssembly)
            {
            }
        }
    }
}
