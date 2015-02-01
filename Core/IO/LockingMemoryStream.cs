using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.IO
{
    public class LockingMemoryStream : MemoryStream
    {
        // TODO: this never gets disposed of correctly
        private SemaphoreSlim _lock = new SemaphoreSlim(1);

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
