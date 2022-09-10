using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HaskellRunner.Helpers
{
    public static class ComUtils
    {
        public static object Get(object obj, string key, object[] args = null)
        {
            return obj.GetType().InvokeMember(
                key,
                System.Reflection.BindingFlags.GetProperty,
                null,
                obj,
                args);
        }

        public static IDictionary<string, dynamic> ExtractParametrized(
            dynamic obj,
            PropertyInfo prop,
            IEnumerable<Type> paramTypes,
            List<object> args = null)
        {
            if (args == null) args = new List<object>();

            var enumResults = new Dictionary<string, dynamic>();

            foreach (var field in paramTypes.First().GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var nextArgs = args.Select(x => x).ToList();
                nextArgs.Add(field.GetRawConstantValue());
                enumResults[field.Name] = (paramTypes.Count() == 1) ?
                    Get(obj, prop.Name, nextArgs.ToArray<object>()) :
                    ExtractParametrized(obj, prop, paramTypes.Skip(1), nextArgs);
            }

            return enumResults;
        }

        public static IDictionary<string, dynamic> Extract(dynamic obj)
        {
            var results = new Dictionary<string, dynamic>();
            var properties = obj.GetType().GetProperties();

            foreach (var p in properties)
            {
                var prop = p as PropertyInfo;
                var arity = prop.GetGetMethod().GetParameters().Length;
                results[p.Name] = (arity == 0) ?
                     Get(obj, p.Name) :
                     ExtractParametrized(obj, prop, prop.GetGetMethod()
                         .GetParameters().Select(pi => pi.ParameterType));
            }

            return results;
        }
    }
}
