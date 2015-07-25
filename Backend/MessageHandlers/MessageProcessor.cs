using System.Collections.Concurrent;

using EnergonSoftware.Backend.Messages;

namespace EnergonSoftware.Backend.MessageHandlers
{
    public class MessageProcessor
    {
        private ConcurrentQueue<Message> _messageQueue;

        public void EnqueueMessage(Message message)
        {
            _messageQueue.Enqueue(message);
        }

        public void RunAsync()
        {
            Message message;
            if(_messageQueue.TryDequeue(out message)) {
// do stuff
            }
        }
    }
}
