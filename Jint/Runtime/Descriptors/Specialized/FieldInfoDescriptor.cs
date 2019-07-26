﻿using System.Globalization;
using System.Reflection;
using Jint.Native;

namespace Jint.Runtime.Descriptors.Specialized
{
    public sealed class FieldInfoDescriptor : PropertyDescriptor
    {
        private readonly Engine _engine;
        private readonly FieldInfo _fieldInfo;
        private readonly object _item;

        public FieldInfoDescriptor(Engine engine, FieldInfo fieldInfo, object item) : base(PropertyFlag.CustomJsValue)
        {
            _engine = engine;
            _fieldInfo = fieldInfo;
            _item = item;

            Writable = true;
        }

        protected internal override JsValue CustomValue
        {
            get => JsValue.FromObject(_engine, _fieldInfo.GetValue(_item));
            set
            {
                var currentValue = value;
                object obj;
                if (_fieldInfo.FieldType == typeof (JsValue))
                {
                    obj = currentValue;
                }
                else
                {
                    // attempt to convert the JsValue to the target type
                    obj = currentValue.ToObject();
                    if (obj.GetType() != _fieldInfo.FieldType)
                    {
                        obj = _engine.ClrTypeConverter.Convert(obj, _fieldInfo.FieldType, CultureInfo.InvariantCulture);
                    }
                }

                _fieldInfo.SetValue(_item, obj);
            }
        }
    }
}
