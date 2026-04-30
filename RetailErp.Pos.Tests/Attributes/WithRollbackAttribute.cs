using System.Reflection;
using System.Transactions;
using Xunit.Sdk;

namespace RetailErp.Pos.Tests.Attributes;

public sealed class WithRollbackAttribute : BeforeAfterTestAttribute
{
	private TransactionScope? _transactionScope;

	public override void Before(MethodInfo methodUnderTest)
	{
		_transactionScope = new TransactionScope(
				TransactionScopeOption.Required,
				new TransactionOptions
				{
					IsolationLevel = IsolationLevel.ReadCommitted,
					Timeout = TimeSpan.FromMinutes(2)
				},
				TransactionScopeAsyncFlowOption.Enabled);
	}

	public override void After(MethodInfo methodUnderTest)
	{
		_transactionScope?.Dispose();
	}
}