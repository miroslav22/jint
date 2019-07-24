using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Descriptors.Specialized;

namespace Jint.Runtime.Interop
{
    public sealed class TypeReference : FunctionInstance, IConstructor, IObjectWrapper
    {
        private HashSet<string> _deletedProperties;

        private TypeReference(Engine engine)
            : base(engine, "typereference", null, null, false, "TypeReference")
        {
        }

        public Type ReferenceType { get; set; }

        public static TypeReference CreateTypeReference(Engine engine, Type type)
        {
            var obj = new TypeReference(engine);
            obj.Extensible = true;
            obj.ReferenceType = type;

            // The value of the [[Prototype]] internal property of the TypeReference constructor is the Function prototype object
            obj.Prototype = engine.Function.PrototypeObject;
            obj._length = PropertyDescriptor.AllForbiddenDescriptor.NumberZero;

            // The initial value of Boolean.prototype is the Boolean prototype object
            obj._prototype = new PropertyDescriptor(engine.Object.PrototypeObject, PropertyFlag.AllForbidden);

            return obj;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            // direct calls on a TypeReference constructor object is equivalent to the new operator
            return Construct(arguments);
        }

        public ObjectInstance Construct(JsValue[] arguments)
        {
            if (arguments.Length == 0 && ReferenceType.IsValueType)
            {
                var instance = Activator.CreateInstance(ReferenceType);
                var result = TypeConverter.ToObject(Engine, JsValue.FromObject(Engine, instance));

                return result;
            }

            var constructors = ReferenceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            foreach (var tuple in TypeConverter.FindBestMatch(_engine, constructors, (info, b) => arguments))
            {
                var method = tuple.Item1;

                var parameters = new object[arguments.Length];
                var methodParameters = method.GetParameters();
                try
                {
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        var parameterType = methodParameters[i].ParameterType;

                        if (typeof(JsValue).IsAssignableFrom(parameterType))
                        {
                            parameters[i] = arguments[i];
                        }
                        else
                        {
                            parameters[i] = Engine.ClrTypeConverter.Convert(
                                arguments[i].ToObject(),
                                parameterType,
                                CultureInfo.InvariantCulture);
                        }
                    }

                    var constructor = (ConstructorInfo) method;
                    var instance = constructor.Invoke(parameters);
                    var result = TypeConverter.ToObject(Engine, FromObject(Engine, instance));

                    // todo: cache method info

                    return result;
                }
                catch
                {
                    // ignore method
                }
            }

            ExceptionHelper.ThrowTypeError(_engine, "No public methods with the specified arguments were found.");
            return null;
        }

        public override bool HasInstance(JsValue v)
        {
            if (v.IsObject())
            {
                var wrapper = v.AsObject() as IObjectWrapper;
                if (wrapper != null)
                    return wrapper.Target.GetType() == ReferenceType;
            }

            return base.HasInstance(v);
        }

        public override bool DefineOwnProperty(string propertyName, PropertyDescriptor desc, bool throwOnError)
        {
            if (throwOnError)
            {
                ExceptionHelper.ThrowTypeError(_engine, "Can't define a property of a TypeReference");
            }

            return false;
        }

        public override bool Delete(string propertyName, bool throwOnError)
        {
            var deletedProperties = _deletedProperties ?? (_deletedProperties = new HashSet<string>());
            var commonPropertyName = char.ToLowerInvariant(propertyName[0]) + (propertyName.Length > 1 ? propertyName.Substring(1) : "");

            if (deletedProperties.Contains(commonPropertyName) == false)
                deletedProperties.Add(commonPropertyName);

            return true;
        }

        public override void Put(string propertyName, JsValue value, bool throwOnError)
        {
            var commonPropertyName = char.ToLowerInvariant(propertyName[0]) + (propertyName.Length > 1 ? propertyName.Substring(1) : "");

            if (_deletedProperties?.Contains(commonPropertyName) == true)
            {
                _deletedProperties.Remove(commonPropertyName);

                if (_deletedProperties.Count == 0)
                    _deletedProperties = null;
            }

            if (!CanPut(propertyName))
            {
                if (throwOnError)
                {
                    ExceptionHelper.ThrowTypeError(Engine);
                }

                return;
            }

            var ownDesc = GetOwnProperty(propertyName);

            if (ownDesc == null)
            {
                if (throwOnError)
                {
                    ExceptionHelper.ThrowTypeError(_engine, "Unknown member: " + propertyName);
                }
                else
                {
                    return;
                }
            }

            //  If property was previously undefined then we want to initialize it
            if (ownDesc == PropertyDescriptor.Undefined)
            {
                ownDesc = new PropertyDescriptor(value, true, true, true);
                FastSetProperty(propertyName, ownDesc);
            }

            ownDesc.Value = value;
        }

        public override PropertyDescriptor GetOwnProperty(string propertyName)
        {
            var commonPropertyName = char.ToLowerInvariant(propertyName[0]) + (propertyName.Length > 1 ? propertyName.Substring(1) : "");

            if (_deletedProperties?.Contains(commonPropertyName) == true)
                return PropertyDescriptor.Undefined;

            if (TryGetProperty(propertyName, out var x))
            {
                return x;
            }

            // todo: cache members locally

            if (ReferenceType.IsEnum)
            {
                Array enumValues = Enum.GetValues(ReferenceType);
                Array enumNames = Enum.GetNames(ReferenceType);

                for (int i = 0; i < enumValues.Length; i++)
                {
                    if (enumNames.GetValue(i) as string == propertyName)
                    {
                        return new PropertyDescriptor((int) enumValues.GetValue(i), PropertyFlag.AllForbidden);
                    }
                }
                return PropertyDescriptor.Undefined;
            }

            foreach (var p in ReferenceType.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (EqualsIgnoreCasing(p.Name, propertyName))
                    return new PropertyInfoDescriptor(Engine, p, Type);
            }


            foreach (var f in ReferenceType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (EqualsIgnoreCasing(f.Name, propertyName))
                    return new FieldInfoDescriptor(Engine, f, Type);
            }


            List<MethodInfo> methodInfo = null;
            foreach (var mi in ReferenceType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (EqualsIgnoreCasing(mi.Name, propertyName))
                {
                    methodInfo = methodInfo ?? new List<MethodInfo>();
                    methodInfo.Add(mi);
                }
            }

            if (methodInfo == null || methodInfo.Count == 0)
            {
                return PropertyDescriptor.Undefined;
            }

            return new PropertyDescriptor(new MethodInfoFunctionInstance(Engine, methodInfo.ToArray()), PropertyFlag.AllForbidden);
        }

        public object Target => ReferenceType;

        private static bool EqualsIgnoreCasing(string s1, string s2)
        {
            bool equals = false;
            if (s1.Length == s2.Length)
            {
                if (s1.Length > 0)
                {
                    equals = char.ToLowerInvariant(s1[0]) == char.ToLowerInvariant(s2[0]);
                }
                if (equals && s1.Length > 1)
                {
                    equals = s1.Substring(1) == s2.Substring(1);
                }
            }
            return equals;
        }
    }
}
