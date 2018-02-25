#region Imports

using System;
using System.Linq;
using LinkDev.Libraries.EnhancedOrgService.Exceptions;
using LinkDev.Libraries.EnhancedOrgService.Helpers;
using LinkDev.Libraries.EnhancedOrgService.Params;

#endregion

namespace LinkDev.Libraries.EnhancedOrgService.Builders
{
	public sealed class EnhancedServiceBuilder : ProcessBase
	{
		private readonly EnhancedServiceParams parameters = new EnhancedServiceParams();

		public static EnhancedServiceBuilder NewBuilder => new EnhancedServiceBuilder();

		private EnhancedServiceBuilder()
		{ }

		public EnhancedServiceBuilder Initialise(string connectionString)
		{
			ValidateInitialised(false);
			ValidateFinalised(false);

			if (connectionString.Trim(';').Split(';').SelectMany(e => e.Split('=')).Count() % 2 != 0)
			{
				throw new FormatException("Connection string format is incorrect.");
			}

			IsInitialised = true;
			parameters.ConnectionString = connectionString;

			return this;
		}

		public EnhancedServiceBuilder AddCaching(CachingParams cachingParams = null)
		{
			ValidateInitialised();
			ValidateFinalised(false);

			parameters.IsCachingEnabled = true;
			parameters.CachingParams = cachingParams ?? new CachingParams();

			return this;
		}

		public EnhancedServiceBuilder AddTransactions(TransactionParams transactionParams = null)
		{
			ValidateInitialised();
			ValidateFinalised(false);

			parameters.IsTransactionsEnabled = true;
			parameters.TransactionParams = transactionParams ?? new TransactionParams();

			return this;
		}

		public EnhancedServiceBuilder AddConcurrency(ConcurrencyParams concurrencyParams = null)
		{
			ValidateInitialised();
			ValidateFinalised(false);

			parameters.IsConcurrencyEnabled = true;
			parameters.ConcurrencyParams = concurrencyParams ?? new ConcurrencyParams();

			return this;
		}

		public EnhancedServiceBuilder HoldAppForAsync()
		{
			ValidateInitialised();
			ValidateFinalised(false);

			if (!parameters.IsConcurrencyEnabled)
			{
				throw new UnsupportedException("Concurrency is not enabled.");
			}

			parameters.ConcurrencyParams.IsAsyncAppHold = true;

			return this;
		}

		public EnhancedServiceBuilder Finalise()
		{
			ValidateInitialised();
			ValidateFinalised(false);

			var connectionString = parameters.ConnectionString;

				var connStrArray = connectionString.Trim(';').Split(';').Select(e => e.Split('=')).ToArray();
				var isReplaced = false;

				foreach (var part in connStrArray)
				{
					if (part[0].Trim().ToLower() == "requirenewinstance")
					{
						part[1] = true.ToString().ToLower();
						isReplaced = true;
						break;
					}
				}

				connectionString = connStrArray.Select(e => e.Aggregate((e1, e2) => $"{e1}={e2}"))
					.Aggregate((e1, e2) => $"{e1};{e2}");

				if (!isReplaced)
				{
					connectionString += ";RequireNewInstance=true";
				}

				parameters.ConnectionString = connectionString;

			if (parameters.CachingParams != null)
			{
				parameters.CachingParams.IsLocked = true;
			}

			if (parameters.TransactionParams != null)
			{
				parameters.TransactionParams.IsLocked = true;
			}

			if (parameters.ConcurrencyParams != null)
			{
				parameters.ConcurrencyParams.IsLocked = true;
			}

			parameters.IsLocked = true;
			IsFinalised = true;

			return this;
		}

		public EnhancedServiceParams GetBuild()
		{
			ValidateFinalised();
			return parameters;
		}
	}
}
