using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSettings
{
    internal static class ReflectionUtils
    {
        private static PropertyInfo GetProperty(object obj, string settingName)
        {
            var property = obj.GetType().GetProperty(settingName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                throw new DynamicSettingsException($"Instance of class `{obj.GetType().Name}` does not have public property `{settingName}`.");
            }
            else
            {
                return property;
            }
        }

        public static void SetObjectProperty(object obj, string memberName, object value)
        {
            GetProperty(obj, memberName).SetValue(obj, value);
        }

        public static object GetObjectProperty(object obj, string memberName)
        {
            return GetProperty(obj, memberName).GetValue(obj);
        }

        public static object GetDynamicProperty(object obj, string memberName)
        {
            var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, obj);
        }

        public static bool SetDynamicProperty(object obj, string memberName, object value)
        {
            try
            {
                var binder = Microsoft.CSharp.RuntimeBinder.Binder.SetMember(CSharpBinderFlags.None,
                   memberName, obj.GetType(),
                   new List<CSharpArgumentInfo>{
                       CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                       CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});

                var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);
                callsite.Target(callsite, obj, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
