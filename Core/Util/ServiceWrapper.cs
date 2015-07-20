using System.ServiceProcess;

namespace EnergonSoftware.Core.Util
{
    /// <summary>
    /// Wrapper for Windows Services
    /// </summary>
    /// <remarks>
    /// This must be a subclass in order to access OnStart()
    /// </remarks>
    public class ServiceWrapper : ServiceBase
    {
        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <param name="runAsService">if set to <c>true</c> this runs as a service, otherwise it runs in the foreground.</param>
        /// <param name="args">The arguments.</param>
        public void Start(bool runAsService, string[] args)
        {
            if(runAsService) {
                ServiceBase[] services = { 
                    this
                };
                Run(services);
            } else {
                OnStart(args);
            }
        }
    }
}
