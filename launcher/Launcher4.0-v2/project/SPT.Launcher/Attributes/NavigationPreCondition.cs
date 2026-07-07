using SPT.Launcher.Models;
using SPT.Launcher.ViewModels;
using ReactiveUI;
using System;

namespace SPT.Launcher.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class NavigationPreCondition : Attribute
    {
        public abstract NavigationPreConditionResult TestPreCondition(IScreen Host);
    }
}
