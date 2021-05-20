using System;
using NUnit.Framework;

namespace CAC.Core.TestUtilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class IntegrationTestAttribute : CategoryAttribute
    {
        public IntegrationTestAttribute()
            : base("integration")
        {
        }
    }
}
