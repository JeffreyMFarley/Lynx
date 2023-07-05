using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Lynx.UI.PasteWizard.ETL
{
    abstract public class TransformBase : ITransform
    {
        #region ITransform Members

        abstract public string DisplayName
        {
            get;
        }

        virtual public void Beginning(DataView extractTable, DataTable loadTable) { }

        abstract public void Transform(DataRow source, DataRow target);

        virtual public void Ending(DataView extractTable, DataTable loadTable) { }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
