using Esoteric.DAL;
using Esoteric.BLL.Interfaces;
using Lynx.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Lynx.Models;

namespace Lynx.UI.PasteWizard.ETL
{
    public class ProcessNewEntity<E> : IEntitySetBuilder, IDisposable
        where E : IExtractSource
    {
        #region Public Properties

        public E Source
        {
            get
            {
                if (source == null)
                {
                    source = Activator.CreateInstance<E>();
                }
                return source;
            }
        }
        E source;

        public List<ITransform> Pipeline
        {
            get
            {
                if (pipeline == null)
                {
                    pipeline = new List<ITransform>();
                }
                return pipeline;
            }
        }
        List<ITransform> pipeline;

        public string TargetName { get; set; }

        #endregion

        #region IEtlProcess Members

        public bool Initialize()
        {
            Source.Load();

            Target = (string.IsNullOrEmpty(TargetName)) ? new EntitySet() : new EntitySet(TargetName);

            foreach(var op in Pipeline)
                op.Beginning(Source.View, Target);

            return true;
        }

        public void Execute(IProgressUI progress)
        {
            if (progress != null)
                progress.SetMinAndMax(0, Source.View.Count);

            foreach (DataRowView drv in Source.View)
            {
                if( progress != null )
                    progress.Increment();

                var loadRow = Target.NewRow();

                foreach (var op in Pipeline)
                    op.Transform(drv.Row, loadRow);

                Target.Rows.Add(loadRow);
            }
        }

        public void Finish()
        {
        }

        #endregion

        #region IEntitySetBuilder Members

        public EntitySet Target
        {
            get
            {
                return target ?? (target = new EntitySet());
            }
            set
            {
                target = value;
            }
        }
        EntitySet target;

        public bool Build(IProgressUI progress)
        {
            bool result = false;

            if (!Initialize())
                return result;

            try
            {
                if( progress != null )
                    progress.Beginning();
                Execute(progress);

                result = true;
            }
            finally
            {
                Finish();
                if (progress != null)
                    progress.Finished();
            }

            return result;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                source.Dispose();
                target.Dispose();
            }
        }

        #endregion

    }
}
