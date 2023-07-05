using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Esoteric.BLL.Interfaces;

namespace Lynx.Interfaces
{
    static public class Extensions
    {
        #region IEtlProcess extensions
        static public bool RunProcessAsynch(this IEtlProcess instance, IProgressUI progress)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (!instance.Initialize())
                    return;

                try
                {
                    progress.Beginning();
                    instance.Execute(progress);
                }
                finally
                {
                    instance.Finish();
                    progress.Finished();
                }
            };
            worker.RunWorkerCompleted += delegate
            {
                worker.Dispose();
            };
            worker.RunWorkerAsync();

            return true;
        }

        static public bool RunProcessInThread(this IEtlProcess instance, IProgressUI progress)
        {
            bool result = false;

            if (!instance.Initialize())
                return result;

            try
            {
                progress.Beginning();
                instance.Execute(progress);

                result = true;
            }
            finally
            {
                instance.Finish();
                progress.Finished();
            }

            return result;
        }
        #endregion
    }
}
