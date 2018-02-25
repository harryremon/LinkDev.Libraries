#region Imports

using System.Collections.Concurrent;
using System.Linq;
using LinkDev.Libraries.EnhancedOrgService.Factories;
using LinkDev.Libraries.EnhancedOrgService.Services;
using Microsoft.Xrm.Sdk;

#endregion

namespace LinkDev.Libraries.EnhancedOrgService.Pools
{
	public class EnhancedServicePool<TService> : IEnhancedServicePool<TService>
		where TService : EnhancedOrgServiceBase
	{
		private readonly EnhancedServiceFactory<TService> factory;

		private readonly ConcurrentQueue<IOrganizationService> crmServicesQueue = new ConcurrentQueue<IOrganizationService>();
		private readonly ConcurrentQueue<TService> servicesQueue = new ConcurrentQueue<TService>();

		public EnhancedServicePool(EnhancedServiceFactory<TService> factory)
		{
			this.factory = factory;
		}

		public TService GetService(int threads = 1)
		{
			servicesQueue.TryDequeue(out var service);
			return GetInitialisedService(threads, service);
		}

		private IOrganizationService GetCrmService()
		{
			crmServicesQueue.TryDequeue(out var crmService);
			return crmService ?? factory.CreateCrmService();
		}

		private TService GetInitialisedService(int threads,
			TService enhancedService = null)
		{
			enhancedService = enhancedService ?? factory.CreateEnhancedService();
			enhancedService.EnhancedServicePool = this;
			enhancedService.FillServicesQueue(Enumerable.Range(0, threads).Select(e => GetCrmService()));
			return enhancedService;
		}

		public void ReleaseService(EnhancedOrgServiceBase enhancedService)
		{
			var releasedServices = enhancedService.ClearServicesQueue();

			foreach (var service in releasedServices)
			{
				crmServicesQueue.Enqueue(service);
			}
		}
	}
}
