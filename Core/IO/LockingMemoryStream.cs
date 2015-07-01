using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.IO
{
    public class LockingMemoryStream : MemoryStream
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

#region Dispose
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                _lock.Dispose();
            }

            base.Dispose(disposing);
        }
#endregion

        /// <summary>
        /// Acquires the stream lock.
        /// </summary>
        public async Task LockAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Releases this stream lock.
        /// </summary>
        public void Release()
        {
            _lock.Release();
        }
    }
}
