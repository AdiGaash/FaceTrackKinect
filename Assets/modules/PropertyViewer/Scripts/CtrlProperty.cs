using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace yak
{
    public enum CtrlLevel
    {
        DEFAULT,//0
        EXPERT,//1
        EDITOR_ONLY,//2
    }

    public enum PropertyType
    {
        FIELD, PROPERTY
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class Ctrl : Attribute
    {
        public string label { get; set; }
        public string tooltip { get; set; }
        public bool visible { get; set; }
        public string numberFormat { get; set; }
        public CtrlLevel level { get; set; }

        public Ctrl(string label = null)
        {
            this.label = label;
        }

        // -------------------------------------------------------------------
        public class CtrlConnection
        {
            public object obj { get; set; }
            public Ctrl attr { get; set; }
            public string propertyName { get; set; }
            public PropertyType propertyType { get; set; }
            public Type fieldType { get; set; }

            public T getValue<T>()
            {
                try
                {
                    object res = null;
                    switch (propertyType)
                    {
                        case PropertyType.FIELD:
                            res = obj.GetType().GetField(propertyName).GetValue(obj);
                            break;
                        case PropertyType.PROPERTY:
                            res = obj.GetType().GetProperty(propertyName).GetGetMethod().Invoke(obj, new object[0]);
                            break;
                    }

                    if (res != null)
                        if (typeof(T) == typeof(string))
                        {
                            if (!string.IsNullOrEmpty(attr.numberFormat))
                            {
                                var str = "{0:" + attr.numberFormat + "}";
                                if (fieldType == typeof(float))
                                {
                                    return (T)Convert.ChangeType(String.Format(str, Convert.ChangeType(res, typeof(float))), typeof(string));
                                }
                                else if (fieldType == typeof(double))
                                {
                                    return (T)Convert.ChangeType(String.Format(str, Convert.ChangeType(res, typeof(double))), typeof(string));
                                }
                                else if (fieldType == typeof(int))
                                {
                                    return (T)Convert.ChangeType(String.Format(str, Convert.ChangeType(res, typeof(int))), typeof(string));
                                }
                                else if (fieldType == typeof(long))
                                {
                                    return (T)Convert.ChangeType(String.Format(str, Convert.ChangeType(res, typeof(long))), typeof(string));
                                }
                                else if (fieldType == typeof(byte))
                                {
                                    return (T)Convert.ChangeType(String.Format(str, Convert.ChangeType(res, typeof(byte))), typeof(string));
                                }
                                else
                                {
                                    //hmm...
                                    return (T)Convert.ChangeType(res.ToString(), typeof(string));
                                }
                            }
                            else
                            {
                                return (T)Convert.ChangeType(res.ToString(), typeof(string));
                            }
                        }
                        else
                        {
                            return (T)Convert.ChangeType(res, typeof(T));
                        }
                    else
                        return default(T);
                }
                catch (Exception e)
                {
                    Debug.LogWarningFormat("can't get value for {0}.{1}\n{2}", obj.GetType(), propertyName, e.ToString());
                    return default(T);
                }
            }

            public void setValue(object value)
            {
                try
                {
                    var converted = Convert.ChangeType(value, fieldType);
                    switch (propertyType)
                    {
                        case PropertyType.FIELD:
                            obj.GetType().GetField(propertyName).SetValue(obj, converted);
                            break;
                        case PropertyType.PROPERTY:
                            obj.GetType().GetProperty(propertyName).GetSetMethod().Invoke(obj, new object[] { converted });
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarningFormat("can't set value for {0}.{1}\n{2}", obj.GetType(), propertyName, e.ToString());
                }
            }
        }

        public static List<CtrlConnection> getForObjects(List<object> obj)
        {
            List<CtrlConnection> res = new List<CtrlConnection>();

            if (obj == null)
                return res;
            
            foreach (var component in obj)
            {
                var fields = component.GetType().GetFields();
                foreach (var item in fields)
                {
                    Ctrl[] attrs = (Ctrl[])Attribute.GetCustomAttributes(item, typeof(Ctrl));
                    foreach (var attr in attrs)
                    {
                        res.Add(new CtrlConnection()
                        {
                            attr = attr,
                            obj = component,
                            propertyName = item.Name,
                            propertyType = PropertyType.FIELD,
                            fieldType = item.FieldType
                        });
                    }
                }

                var props = component.GetType().GetProperties();
                foreach (var item in props)
                {
                    Ctrl[] attrs = (Ctrl[])Attribute.GetCustomAttributes(item, typeof(Ctrl));
                    foreach (var attr in attrs)
                    {
                        res.Add(new CtrlConnection()
                        {
                            attr = attr,
                            obj = component,
                            propertyName = item.Name,
                            propertyType = PropertyType.PROPERTY,
                            fieldType = item.PropertyType
                        });
                    }
                }
            }

            return res;
        }
    }


    // -------------------------------------------------------------------
    public class CtrlText : Ctrl
    {

    }

    public class CtrlSlider : Ctrl
    {
        public float min { get; set; }
        public float max { get; set; }

        public CtrlSlider(string label = null)
            : base(label)
        {

        }
    }

    // -------------------------------------------------------------------
    public class CtrlInputText : Ctrl
    {

    }

    public class CtrlToggle : Ctrl
    {

    }

    // -------------------------------------------------------------------
    public class CtrlCategory : Ctrl
    {

    }

    public class CtrlSpace : Ctrl
    {

    }

    // -------------------------------------------------------------------

    // -------------------------------------------------------------------
}
