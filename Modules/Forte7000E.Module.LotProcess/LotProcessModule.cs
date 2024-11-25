using Forte7000E.Module.LotProcess.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Forte7000E.Module.LotProcess
{
    public class LotProcessModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

            var regionManager = containerProvider.Resolve<IRegionManager>();
           // regionManager.RegisterViewWithRegion("ContentRegion", typeof(LotProcessView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
        }
    }
}