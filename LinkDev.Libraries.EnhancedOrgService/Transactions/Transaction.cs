﻿using LinkDev.Libraries.EnhancedOrgService.Response;
using Microsoft.Xrm.Sdk;

namespace LinkDev.Libraries.EnhancedOrgService.Transactions
{
	/// <summary>
	///     Author: Ahmed el-Sawalhy (LINK Development)
	/// </summary>
	public class Transaction
	{
		public readonly string Id;

		/// <summary>
		/// If true, indicates that the transaction hasn't ended yet.
		/// </summary>
		public bool Current = true;

		internal OperationBase StartingPoint;

		internal Transaction(string id, OperationBase startingPoint = null)
		{
			Id = id;
			StartingPoint = startingPoint;
		}
	}
}
