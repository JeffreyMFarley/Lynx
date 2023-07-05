using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lynx.UI.PasteWizard
{
    static public class Extensions
    {
        #region DataType Extensions

        static public string Display(this DataType d)
        {
            switch (d)
            {
                case DataType.AsString:
                    return "as String";

                case DataType.AsBoolean:
                    return "as Boolean";

                case DataType.AsDateTime:
                    return "as Date/Time";

                case DataType.AsID:
                    return "as ID";

                case DataType.AsNumeric:
                    return "as Numeric";
            }

            return string.Empty;
        }

        static public Type ToType(this DataType d)
        {
            switch (d)
            {
                case DataType.AsBoolean:
                    return typeof(Boolean);

                case DataType.AsDateTime:
                    return typeof(DateTime);

                case DataType.AsID:
                    return typeof(Guid);

                case DataType.AsNumeric:
                    return typeof(float);
            }

            return typeof(string);
        }

        #endregion

    }
}
