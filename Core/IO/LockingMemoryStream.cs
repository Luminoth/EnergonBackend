using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.IO
{
    public class LockingMemoryStream : MemoryStream
    {
        private SemaphoreSlim _lock = new SemaphoreSlim(1);

#region Dispose
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                _lock.Dispose();
            }
            base.Dispose(disposing);
        }
#endregion

        public async Task LockAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);
        }

        public void Release()
        {
            _lock.Release();
        }
    }
}
