using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;

namespace Lynx.Models
{
    public class Entity : DataRow
    {
        public Entity(DataRowBuilder builder)
            : base(builder)
        {
            ID = Guid.NewGuid();
        }

        #region Standard Properties
        public Guid ID
        {
            get
            {
                return this.Field<Guid>(Domain.IDColumn);
            }
            set
            {
                this[Domain.IDColumn] = value;
            }
        }

        public string Name
        {
            get
            {
                return this.Field<string>(Domain.NameColumn);
            }
            set
            {
                this[Domain.NameColumn] = value;
            }
        }

        public string Description
        {
            get
            {
                return this.Field<string>(Domain.DescriptionColumn);
            }
            set
            {
                this[Domain.DescriptionColumn] = value;
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
