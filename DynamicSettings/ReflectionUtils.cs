using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
            object result;
            (obj as DynamicObject).TryGetMember(new GetBinder(memberName, false), out result);
            return result;
        }

        public static bool SetDynamicProperty(object obj, string memberName, object value)
        {
            return (obj as DynamicObject).TrySetMember(new SetBinder(memberName, false), value);
        }

        private class GetBinder : GetMemberBinder
        {
            public GetBinder(string name, bool ignoreCase) : base(name, ignoreCase)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        private class SetBinder : SetMemberBinder
        {
            public SetBinder(string name, bool ignoreCase) : base(name, ignoreCase)
            {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }
    }
}
