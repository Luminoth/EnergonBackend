using System;

namespace EnergonSoftware.Core
{
    public sealed class ErrorEventArgs : EventArgs
    {
        private string _error;
        public string Error
        {
            get { return null != Exception && string.IsNullOrEmpty(_error) ? Exception.Message : _error; }
            set { _error = value; }
        }
        public Exception Exception { get; set; }
    }
}
