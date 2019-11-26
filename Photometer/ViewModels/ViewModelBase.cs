using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace Photometer.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
