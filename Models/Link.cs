using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Lynx.Models
{
    public class Link : DataRow
    {
        public Link(DataRowBuilder builder)
            : base(builder)
        {
        }

        #region Typed Properties
        public Guid SourceID
        {
            get
            {
                return this.Field<Guid>(Domain.SourceIDColumn);
            }
            set
            {
                this[Domain.SourceIDColumn] = value;
            }
        }

        public Guid TargetID
        {
            get
            {
                return this.Field<Guid>(Domain.TargetIDColumn);
            }
            set
            {
                this[Domain.TargetIDColumn] = value;
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
    }
}
