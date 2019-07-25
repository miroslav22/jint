using System;
using System.Runtime.CompilerServices;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Global;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;

namespace Jint
{
    public static class JsValueExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AsBoolean(this JsValue value)
        {
            if (value._type != Types.Boolean)
            {
                ExceptionHelper.ThrowArgumentException($"Expected boolean but got {value._type}");
            }

            return ((JsBoolean) value)._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AsNumber(this JsValue value)
        {
            if (value._type != Types.Number)
            {
                ExceptionHelper.ThrowArgumentException($"Expected number but got {value._type}");
            }

            return ((JsNumber) value)._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AsString(this JsValue value)
        {
            if (value._type != Types.String)
            {
                ExceptionHelper.ThrowArgumentException($"Expected string but got {value._type}");
            }

            return AsStringWithoutTypeCheck(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string AsStringWithoutTypeCheck(this JsValue value)
        {
            return value.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AsSymbol(this JsValue value)
        {
            if (value._type != Types.Symbol)
            {
                ExceptionHelper.ThrowArgumentException($"Expected symbol but got {value._type}");
            }

            return ((JsSymbol) value)._value;
        }

        public static T AsNetObject<T>(this JsValue value) => (T) value.AsNetObject(typeof(T));

        public static object AsNetObject(this JsValue value, Type type = null)
        {
            if (value.IsUndefined())
            {
                if (type != null)
                    ExceptionHelper.ThrowArgumentException($"Cannot convert undefined to type {type.Name}");

                return JsValue.Undefined;
            }

            if (value.IsNull())
            {
                if (type != null && type.IsByRef)
                    ExceptionHelper.ThrowArgumentException($"Cannot convert null to type {type.Name}");

                return null;
            }

            if (value.IsBoolean())
            {
                if (type != null && type != typeof(bool))
                    ExceptionHelper.ThrowArgumentException($"Cannot convert bool to type {type.Name}");

                return value.AsBoolean();
            }

            if (value.IsString())
            {
                if (type != null && type != typeof(string))
                    ExceptionHelper.ThrowArgumentException($"Cannot convert string to type {type.Name}");

                return value.AsString();
            }

            if (value.IsNumber())
            {
                var doubleValue = value.AsNumber();
                object numberValue = doubleValue;

                //  If whole number then convert to int
                if (Math.Abs(doubleValue - (int)doubleValue) < double.Epsilon)
                    numberValue = (int)doubleValue;

                if (type?.IsValueType == false)
                    ExceptionHelper.ThrowArgumentException($"Cannot convert number to type {type.Name}");

                if (type != null)
                    numberValue = Convert.ChangeType(numberValue, type);

                return numberValue;
            }

            if (value.IsArray())
            {
                if (type != null && type.IsArray == false)
                    ExceptionHelper.ThrowArgumentException($"Cannot convert array to type {type.Name}");

                var array = value.AsArray();
                var objectArray = new object[array.GetLength()];

                for (var i = 0; i < objectArray.Length; i++)
                    objectArray[i] = array.Get(i.ToString()).AsNetObject();

                return ConvertArrayTypeToElementCommonBaseType(objectArray);
            }

            if (value.IsObject())
            {
                var obj = value.AsObject();

                if (obj is ObjectWrapper objectWrapper)
                {
                    var wrappedObject = objectWrapper.Target;

                    if(type?.IsInstanceOfType(wrappedObject) == false)
                        ExceptionHelper.ThrowArgumentException($"Cannot convert {wrappedObject.GetType().Name} to type {type.Name}");

                    return wrappedObject;
                }

                if (type?.IsInstanceOfType(obj) == false)
                    ExceptionHelper.ThrowArgumentException($"Cannot convert object to type {type.Name}");

                return obj;
            }
            

            ExceptionHelper.ThrowArgumentException($"Cannot convert {value.Type} to type {type?.Name ?? "unknown"}");
            throw new Exception();
        }

        private static object ConvertArrayTypeToElementCommonBaseType(object[] array)
        {
            var arrayLength = array.Length;

            //  If all elements are a common base type then convert the array to this type
            if (arrayLength >= 1)
            {
                var currentType = array[0].GetType();
                var allNumeric = currentType == typeof(double) || currentType == typeof(int);

                for (var i = 1; i < arrayLength; i++)
                {
                    var nextType = array[i].GetType();

                    if (nextType != typeof(double) && nextType != typeof(int))
                        allNumeric = false;

                    while ((currentType.IsAssignableFrom(nextType) == false || currentType == typeof(ValueType)) && currentType != typeof(object))
                        currentType = currentType.BaseType;

                    if (currentType == typeof(object))
                        break;
                }

                //  If type is object but all numeric then convert to double array
                if (currentType == typeof(object) && allNumeric)
                    currentType = typeof(double);

                if (currentType != typeof(object))
                {
                    var typedArray = Array.CreateInstance(currentType, arrayLength);

                    for (var i = 0; i < arrayLength; i++)
                        typedArray.SetValue(Convert.ChangeType(array[i], currentType), i);

                    return typedArray;
                }
            }

            return array;
        }
    }
}