﻿#region Imports

using System;
using LinkDev.Libraries.EnhancedOrgService.Response;
using Microsoft.Xrm.Sdk;

#endregion

namespace LinkDev.Libraries.EnhancedOrgService.Transactions
{
	internal interface ITransactionManager
	{
		bool IsTransactionInEffect();

		Transaction BeginTransaction(string transactionId = null, OperationBase startingPoint = null);

		void ProcessRequest(IOrganizationService service, OperationBase operation,
			Func<IOrganizationService, OrganizationRequest, OrganizationRequest> undoFunction = null);

		void UndoTransaction(IOrganizationService service, Transaction transaction = null);

		void EndTransaction(Transaction transaction = null);
	}
}
