﻿using System.Globalization;
using System.Reflection;
using Jint.Native;

namespace Jint.Runtime.Descriptors.Specialized
{
    public sealed class PropertyInfoDescriptor : PropertyDescriptor
    {
        private readonly Engine _engine;
        private readonly PropertyInfo _propertyInfo;
        private readonly object _item;
        private object _overriddenValue;

        public PropertyInfoDescriptor(Engine engine, PropertyInfo propertyInfo, object item) : base(PropertyFlag.CustomJsValue)
        {
            _engine = engine;
            _propertyInfo = propertyInfo;
            _item = item;

            Writable = true;
        }

        protected internal override JsValue CustomValue
        {
            get => JsValue.FromObject(_engine, _overriddenValue ?? _propertyInfo.GetValue(_item, null));
            set
            {
                var currentValue = value;
                object obj;
                if (_propertyInfo.PropertyType == typeof (JsValue))
                {
                    obj = currentValue;
                }
                else
                {
                    // attempt to convert the JsValue to the target type
                    obj = currentValue.ToObject();
                    if (obj != null && obj.GetType() != _propertyInfo.PropertyType)
                    {
                        if (_engine.ClrTypeConverter.TryConvert(obj, _propertyInfo.PropertyType, CultureInfo.InvariantCulture, out var convertedObject) == false)
                        {
                            _overriddenValue = obj;
                            return;
                        }

                        obj = convertedObject;
                    }
                }

                _propertyInfo.SetValue(_item, obj, null);
                _overriddenValue = null;
            }
        }
    }
}
