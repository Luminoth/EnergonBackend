using System.ServiceProcess;

namespace EnergonSoftware.Core.Util
{
    public class ServiceWrapper : ServiceBase
    {
        public void Start(bool runAsService, string[] args)
        {
            if(runAsService) {
                ServiceBase[] services = new ServiceBase[]
                { 
                    this
                };
                ServiceBase.Run(services);
            } else {
                OnStart(args);
            }
        }
    }
}
