using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkDev.Libraries.EnhancedOrgService.Builders;
using LinkDev.Libraries.EnhancedOrgService.Factories;
using LinkDev.Libraries.EnhancedOrgService.Params;
using LinkDev.Libraries.EnhancedOrgService.Pools;

namespace LinkDev.Libraries.EnhancedOrgService.Helpers
{
    public static class EnhancedServiceHelper
    {
	    public static EnhancedServicePool<Services.EnhancedOrgService> GetPool(string connectionString, 
			EnhancedServiceParams serviceParams = null)
	    {
			var builder = EnhancedServiceBuilder.NewBuilder.Initialise(connectionString);

		    if (serviceParams?.IsCachingEnabled == true)
		    {
			    serviceParams.CachingParams = serviceParams.CachingParams
					?? new CachingParams
					   {
						   CacheMode = CacheMode.PrivatePerInstance
					   };
				builder.AddCaching(serviceParams.CachingParams);
		    }

		    if (serviceParams?.IsConcurrencyEnabled == true)
		    {
			    builder.AddConcurrency(serviceParams.ConcurrencyParams);
		    }

		    if (serviceParams?.IsTransactionsEnabled == true)
		    {
			    builder.AddTransactions(serviceParams.TransactionParams);
		    }

			var build = builder.Finalise().GetBuild();
		    var factory = new EnhancedServiceFactory<Services.EnhancedOrgService>(build);
		    return new EnhancedServicePool<Services.EnhancedOrgService>(factory);
		}

	    public static EnhancedServicePool<Services.AsyncOrgService> GetAsyncPool(string connectionString, 
			EnhancedServiceParams serviceParams = null, bool isHoldAppForAsync = true)
	    {
			var builder = EnhancedServiceBuilder.NewBuilder.Initialise(connectionString);

		    if (serviceParams?.IsCachingEnabled == true)
		    {
			    serviceParams.CachingParams = serviceParams.CachingParams
				    ?? new CachingParams
					   {
						   CacheMode = CacheMode.PrivatePerInstance
					   };
			    builder.AddCaching(serviceParams.CachingParams);
		    }

		    if (serviceParams?.IsConcurrencyEnabled != false)
		    {
			    builder.AddConcurrency(serviceParams?.ConcurrencyParams);

			    if (isHoldAppForAsync)
			    {
				    builder.HoldAppForAsync();
			    }
		    }

		    if (serviceParams?.IsTransactionsEnabled == true)
		    {
			    builder.AddTransactions(serviceParams.TransactionParams);
		    }

			var build = builder.Finalise().GetBuild();
		    var factory = new EnhancedServiceFactory<Services.AsyncOrgService>(build);
		    return new EnhancedServicePool<Services.AsyncOrgService>(factory);
		}
	}
}
