using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EWSEditor.PropertyInformation.TypeValues
{
    public abstract class SimpleTypeValueBase<T> : ITypeValue
    {
        Type[] ITypeValue.SupportedTypes => new[] { typeof(T) };

        public virtual void GetValue(T obj, StringBuilder sb) { }

        public virtual string GetValue(T obj)
        {
            var sb = new StringBuilder();
            GetValue(obj, sb);
            return sb.ToString();
        }

        string ITypeValue.GetValue(object rawValue)
        {
            if (!(rawValue is T obj))
                return string.Empty;

            return GetValue(obj);
        }

        string ITypeValue.GetValue(object ownerInstance, PropertyInfo propInfo)
        {
            return ((ITypeValue)this).GetValue(propInfo.GetValue(ownerInstance));
        }
    }
}
