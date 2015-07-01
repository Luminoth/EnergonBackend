using System.ServiceProcess;

namespace EnergonSoftware.Core.Util
{
    // NOTE: must be a subclass in order to access OnStart()
    public class ServiceWrapper : ServiceBase
    {
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
