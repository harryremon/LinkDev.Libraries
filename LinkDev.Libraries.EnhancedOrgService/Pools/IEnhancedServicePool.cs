using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkDev.Libraries.EnhancedOrgService.Builders;
using LinkDev.Libraries.EnhancedOrgService.Factories;
using LinkDev.Libraries.EnhancedOrgService.Helpers;
using LinkDev.Libraries.EnhancedOrgService.Services;
using Microsoft.Xrm.Sdk;

namespace LinkDev.Libraries.EnhancedOrgService.Pools
{
    public interface IEnhancedServicePool<out TService>
	    where TService : EnhancedOrgServiceBase
	{
		TService GetService(int threads = 1);
	    void ReleaseService(EnhancedOrgServiceBase service);
    }
}
