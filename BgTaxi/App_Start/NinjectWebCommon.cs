[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(BgTaxi.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(BgTaxi.App_Start.NinjectWebCommon), "Stop")]

namespace BgTaxi.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Models;
    using Models.Models;
    using Services.Contracts;
    using Services;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            
            kernel
                .Bind<IDatabase>()
                .To<Database>()
                .InRequestScope();
            kernel
              .Bind<ICarService>()
              .To<CarService>()
              .InRequestScope();
            kernel
              .Bind<IRequestService>()
              .To<RequestService>()
              .InRequestScope();

            kernel
              .Bind<IDriverService>()
              .To<DriverService>()
              .InRequestScope();

            kernel
              .Bind<IAccessTokenService>()
              .To<AccessTokenService>()
              .InRequestScope();
            kernel
              .Bind<ICompanyService>()
              .To<CompanyService>()
              .InRequestScope();
            kernel
              .Bind<IDispatcherService>()
              .To<DispatcherService>()
              .InRequestScope();
            kernel
              .Bind<IDeviceService>()
              .To<DeviceService>()
              .InRequestScope();
        }        
    }
}
