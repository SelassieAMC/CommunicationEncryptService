using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;

namespace servicioCliente.AppUtils
{
    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var webHostService = new CustomWebHostService(host);
            ServiceBase.Run(webHostService);
        }
    }
}