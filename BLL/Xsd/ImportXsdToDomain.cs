using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Esoteric;
using Esoteric.BLL.Interfaces;
using Esoteric.DAL.Interface;
using Lynx.Models;
using Lynx.Interfaces;

namespace Lynx.XsdExchange
{
    public class ImportXsdToDomain : IEtlProcess, IDisposable
    {
        public ImportXsdToDomain()
        {
            CompositionInitializer.SatisfyImports(this);
        }

        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region Private Properties
        XsdExtractor Extract
        {
            get
            {
                return extractor ?? (extractor = new XsdExtractor());
            }
        }
        XsdExtractor extractor;

        XsdToDomainTransform Transform
        {
            get
            {
                return transformer ?? (transformer = new XsdToDomainTransform());
            }
        }
        XsdToDomainTransform transformer;
        #endregion

        #region Public Properties
        public FileInfo SourceFile { get; set; }
        #endregion

        #region IEtlProcess Members
        public bool Initialize()
        {
            if (SourceFile == null)
                return false;

            Extract.Source = SourceFile;

            Transform.EntitySetRepository = ActiveDomain.Manager.EntitySets;
            Transform.LinkSetRepository = ActiveDomain.Manager.LinkSets;

            return true;
        }

        public void Execute(IProgressUI progress)
        {
            int max = 0, i = 0;

            progress.SetMessage("Initializing");
            progress.SetMinAndMax(0, 1);

            foreach (var e in Extract.EnumerateEntities())
            {
                if (Extract.ProgressMax > max)
                {
                    max = Extract.ProgressMax;
                    progress.SetMinAndMax(0, max);
                }
                progress.IncrementTo(i++);
                progress.SetMessage(e.Name.LocalName);
                Transform.Accept(e);
            }

            foreach (var l in Extract.EnumerateLinks())
                Transform.Accept(l);
        }

        public void Finish()
        {
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
                if (extractor != null) extractor.Dispose();
            }
        }

        ~ImportXsdToDomain()
        {
            Dispose(false);
        }
        #endregion
    }
}
