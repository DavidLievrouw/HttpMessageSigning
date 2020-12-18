using System;

namespace Dalion.HttpMessageSigning.Verification.SqlServer.Infrastructure {
    [AttributeUsage(AttributeTargets.Class)]
    public class DisableTransactionScopeAttribute : Attribute {
    }
}