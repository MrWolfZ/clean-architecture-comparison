using System;

namespace CAC.Core.Application.CommandHandling.Behaviors
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class CommandHandlerBehaviorAttribute : Attribute
    {
    }
}
