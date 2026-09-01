using System;
using System.Collections.Generic;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public abstract class SymbolizedBasePhysicalQuantity : BasePhysicalQuantity
    {
        /// <summary>
        /// the symbol used for this base unit in physical quantity descriptions: L, T, M, ...
        /// </summary>
        public abstract string DimensionSymbol { get; }

    }
}
