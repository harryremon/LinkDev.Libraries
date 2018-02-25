using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkDev.Libraries.EnhancedOrgService.Builders;
using LinkDev.Libraries.EnhancedOrgService.Helpers;
using LinkDev.Libraries.EnhancedOrgService.Services;
using Microsoft.Xrm.Sdk;

namespace LinkDev.Libraries.EnhancedOrgService.Factories
{
	public interface IEnhancedServiceFactory<out TEnhancedOrgService>
		where TEnhancedOrgService : IEnhancedOrgService
	{
		TEnhancedOrgService CreateEnhancedService();
		IOrganizationService CreateCrmService();
	}
}
