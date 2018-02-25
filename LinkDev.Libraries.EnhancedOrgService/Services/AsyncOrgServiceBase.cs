﻿#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinkDev.Libraries.Common;
using LinkDev.Libraries.EnhancedOrgService.Exceptions;
using LinkDev.Libraries.EnhancedOrgService.Helpers;
using LinkDev.Libraries.EnhancedOrgService.Params;
using LinkDev.Libraries.EnhancedOrgService.Response;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using OperationStatus = LinkDev.Libraries.EnhancedOrgService.Response.OperationStatus;

#endregion

namespace LinkDev.Libraries.EnhancedOrgService.Services
{
	/// <summary>
	///     Author: Ahmed el-Sawalhy<br />
	///     Version: 4.1.1
	/// </summary>
	public abstract class AsyncOrgServiceBase : EnhancedOrgService, IAsyncOrgService
	{
		private readonly bool isAsyncAppHold;

		public AsyncOrgServiceBase(EnhancedServiceParams parameters) : base(parameters)
		{
			if (!parameters.IsConcurrencyEnabled)
			{
				throw new UnsupportedException("Cannot initiate an async org service"
					+ " unless concurrency is enabled.");
			}

			isAsyncAppHold = parameters.ConcurrencyParams.IsAsyncAppHold;
		}

		#region Async

		private Task GetDependenciesTask(params Task[] dependencies)
		{
			return dependencies.Aggregate<Task, Task>(null, (current, task) =>
				current?.ContinueWith(taskQ => task.Wait()) ?? Task.Run(() => task.Wait()));
		}

		private Task<TResult> GetDependentResult<TResult>(Func<TResult> function, params Task[] dependencies)
		{
			var task = GetDependenciesTask(dependencies)?.ContinueWith(taskQ => function()) ?? Task.Run(function);
			CheckAppHold(task);
			return task;
		}

		public async Task<Guid> CreateAsync(Entity entity, params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult(() => Create(entity), dependencies);
		}

		public async Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet,
			params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult(() => Retrieve(entityName, id, columnSet), dependencies);
		}

		public async Task<object> UpdateAsync(Entity entity, params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult<object>(
				() =>
				{
					Update(entity);
					return null;
				}, dependencies);
		}

		public async Task<object> DeleteAsync(string entityName, Guid id, params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult<object>(
				() =>
				{
					Delete(entityName, id);
					return null;
				}, dependencies);
		}

		public async Task<object> AssociateAsync(string entityName, Guid entityId, Relationship relationship,
			EntityReferenceCollection relatedEntities, params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult<object>(
				() =>
				{
					Associate(entityName, entityId, relationship, relatedEntities);
					return null;
				}, dependencies);
		}

		public async Task<object> DisassociateAsync(string entityName, Guid entityId, Relationship relationship,
			EntityReferenceCollection relatedEntities, params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult<object>(
				() =>
				{
					Disassociate(entityName, entityId, relationship, relatedEntities);
					return null;
				}, dependencies);
		}

		public async Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult(() => RetrieveMultiple(query), dependencies);
		}

		public Task<TResponseType> ExecuteAsync<TResponseType>(OrganizationRequest request,
			params Task[] dependencies)
			where TResponseType : OrganizationResponse
		{
			ValidateState();
			return ExecuteAsync<TResponseType>(request, null, dependencies);
		}

		public async Task<TResponseType> ExecuteAsync<TResponseType>(OrganizationRequest request,
			Func<IOrganizationService, OrganizationRequest, OrganizationRequest> undoFunction,
			params Task[] dependencies)
			where TResponseType : OrganizationResponse
		{
			ValidateState();
			return await GetDependentResult(() => (TResponseType)Execute(request, undoFunction), dependencies);
		}

		#region Execute bulk

		public Task<Dictionary<OrganizationRequest, ExecuteBulkResponse>> ExecuteBulkAsync(
			List<OrganizationRequest> requestsList, bool isReturnResponses, params Task[] dependencies)
		{
			ValidateState();
			return ExecuteBulkAsync(requestsList, isReturnResponses, 1000, true, null, dependencies);
		}
		
		public Task<Dictionary<OrganizationRequest, ExecuteBulkResponse>> ExecuteBulkAsync(
			List<OrganizationRequest> requestsList, bool isReturnResponses, int bulkSize, params Task[] dependencies)
		{
			ValidateState();
			return ExecuteBulkAsync(requestsList, isReturnResponses, bulkSize,  true, null ,dependencies);
		}

		public async Task<Dictionary<OrganizationRequest, ExecuteBulkResponse>> ExecuteBulkAsync(
			List<OrganizationRequest> requests,
			bool isReturnResponses = false, int batchSize = 1000, bool isContinueOnError = true,
			Action<int, int, IDictionary<OrganizationRequest, ExecuteBulkResponse>> bulkFinishHandler = null,
			params Task[] dependencies)
		{
			ValidateState();
			return await GetDependentResult(() =>
				ExecuteBulk(requests, isReturnResponses, batchSize, isContinueOnError, bulkFinishHandler), dependencies);
		}

		#endregion

		#region Retrieve multiple

		public async Task<List<TEntityType>> RetrieveMultipleAsync<TEntityType>(QueryExpression query,
			int limit = -1, params Task[] dependencies)
			where TEntityType : Entity
		{
			ValidateState();

			QueryExpression clonedQuery;

			using (var service = GetService())
			{
				clonedQuery = RequestHelper.CloneQuery(service, query);
			}

			return await GetDependentResult(() => RetrieveMultiple<TEntityType>(clonedQuery, limit), dependencies);
		}

		public async Task<List<TEntityType>> RetrieveMultipleRangePagedAsync<TEntityType>(QueryExpression query,
			int pageStart = 1, int pageEnd = 1, int pageSize = 5000, params Task[] dependencies)
			where TEntityType : Entity
		{
			ValidateState();

			QueryExpression clonedQuery;

			using (var service = GetService())
			{
				clonedQuery = RequestHelper.CloneQuery(service, query);
			}

			return await GetDependentResult(() => RetrieveMultipleRangePaged<TEntityType>(clonedQuery, pageStart, pageEnd, pageSize), dependencies);
		}

		public async Task<List<TEntityType>> RetrieveMultiplePageAsync<TEntityType>(QueryExpression query,
			int pageSize = 5000, int page = 1, params Task[] dependencies)
			where TEntityType : Entity
		{
			ValidateState();

			QueryExpression clonedQuery;

			using (var service = GetService())
			{
				clonedQuery = RequestHelper.CloneQuery(service, query);
			}

			return await GetDependentResult(() => RetrieveMultiplePage<TEntityType>(clonedQuery, pageSize, page), dependencies);
		}

		#endregion

		#endregion

		#region Utility

		public async Task<int> GetRecordsCountAsync(QueryBase query)
		{
			ValidateState();

			var task = Task.Run(() => GetRecordsCount(query));
			CheckAppHold(task);

			return await task;
		}

		public async Task<int> GetPagesCountAsync(QueryBase query, int pageSize = 5000)
		{
			ValidateState();

			var task = Task.Run(() => GetPagesCount(query, pageSize));
			CheckAppHold(task);

			return await Task.Run(() => GetPagesCount(query, pageSize));
		}

		public async Task<QueryExpression> CloneQueryAsync(QueryBase query)
		{
			ValidateState();

			var task = Task.Run(() => CloneQuery(query));
			CheckAppHold(task);

			return await Task.Run(() => CloneQuery(query));
		}

		#endregion

		private void CheckAppHold(Task task)
		{
			if (isAsyncAppHold)
			{
				new Thread(task.Wait).Start();
			}
		}
	}
}
