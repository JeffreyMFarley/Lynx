using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Esoteric.UI;
using Esoteric.Hollerith;
using Esoteric.Hollerith.Defaults;
using Esoteric.Hollerith.Presentation;

namespace Lynx.UI.Hollerith
{
    public class Bin<U> : TypedBin<Card, U>, IDataColumnBin
    {

        #region IDataColumnBin Members

        public DataColumn Column
        {
            get;
            set;
        }

        public string ValueAsString
        {
            get
            {
                return (ValueToSet != null) ? ValueToSet.ToString() : null;
            }
            set
            {
                ValueToSet = (U)Convert.ChangeType(value, typeof(U));
            }
        }

        #endregion

        public override string FieldName
        {
            get
            {
                return Column.ColumnName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override bool SetValue(Card card)
        {
            card[Column.ColumnName] = ValueAsString;
            return true;
        }
    }
}
