using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Configurable variable. 
    /// It is used to provide high-level access to program configuration. 
    /// These variables control behavior of program, they can be configured at runtime by various 
    /// means (eg. command line, UI settings, Console, etc), and persisted to disk for later usage.
    /// </summary>
    public abstract class ConfigVar
    {
        public enum PersistType
        {
            /// <summary>
            /// <see cref="ConfigVar"/> should not be persisted into config.
            /// </summary>
            None,

            /// <summary>
            /// <see cref="ConfigVar"/> should be persisted into config, and loaded from it on application start.
            /// </summary>
            OnAppStart,
        }

        public string SerializationName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public PersistType Persist { get; init; } = PersistType.OnAppStart;
        public bool ApplyDefaultValueWhenNotPresentInConfig { get; init; } = false;
        public string Category { get; init; } = string.Empty;
        public ConfigVarValue DefaultValue { get; init; }
        ConfigVarValue? LastKnownValue { get; set; }

        public Func<bool> IsAvailableCallback { get; init; } = () => true;
        public Func<ConfigVarValue> GetValueCallback { get; init; }
        public Action<ConfigVarValue> SetValueCallback { get; init; }
        public Action<ConfigVarValue> ValidateCallback { get; init; }

        public string FinalSerializationName => string.IsNullOrWhiteSpace(this.SerializationName)
            ? (string.IsNullOrWhiteSpace(this.Description)
                ? throw new ArgumentException("You must specify serialization name or description")
                : this.Description)
            : this.SerializationName;

        public ConfigVar()
        {
        }

        public abstract ConfigVarValue LoadValueFromString(string str);

        public abstract string SaveValueToString(ConfigVarValue obj);

        public void SetValue(ConfigVarValue value)
        {
            this.Validate(value);
            
            if (this.IsAvailableCallback() && this.SetValueCallback != null)
            {
                this.SetValueCallback(value);
                this.LastKnownValue = this.GetValueCallback?.Invoke() ?? value;
            }
            else
            {
                this.LastKnownValue = value;
            }
        }

        public ConfigVarValue GetValue()
        {
            if (this.IsAvailableCallback() && this.GetValueCallback != null)
            {
                ConfigVarValue value = this.GetValueCallback();
                this.LastKnownValue = value;
                return value;
            }

            return this.LastKnownValue ?? this.DefaultValue;
        }

        public virtual void Validate(ConfigVarValue value)
        {
            this.ValidateCallback?.Invoke(value);
        }
    }

    public struct ConfigVarValue : IEquatable<ConfigVarValue>
    {
        public Union16 Union16Value;
        public object ReferenceValue;

        public int IntValue { readonly get => Union16Value.Part1.IntValuePart1; set => Union16Value.Part1.IntValuePart1 = value; }
        public ulong Uint64Value { readonly get => Union16Value.Part1.Uint64Value; set => Union16Value.Part1.Uint64Value = value; }
        public float FloatValue { readonly get => Union16Value.Part1.FloatValuePart1; set => Union16Value.Part1.FloatValuePart1 = value; }
        public string StringValue { readonly get => (string)ReferenceValue; set => ReferenceValue = value; }
        public bool BoolValue { readonly get => Union16Value.Part1.BoolValue; set => Union16Value.Part1.BoolValue = value; }

        public readonly bool Equals(ConfigVarValue other)
        {
            return this.Union16Value.Equals(other.Union16Value) && this.ReferenceValue.Equals(other.ReferenceValue);
        }
    }

    public abstract class ConfigVar<T> : ConfigVar
    {
        public Func<T> GetValueCallbackGeneric
        {
            init => GetValueCallback = () => CreateValueFromGenericValue(value());
        }

        public Action<T> SetValueCallbackGeneric
        {
            init => SetValueCallback = (val) => value(ExtractGenericValue(val));
        }

        public T ValueGeneric { get => ExtractGenericValue(GetValue()); set => SetValue(CreateValueFromGenericValue(value)); }

        public abstract T ExtractGenericValue(ConfigVarValue value);
        public abstract ConfigVarValue CreateValueFromGenericValue(T genericValue);
    }

    public abstract class NumericConfigVar<T> : ConfigVar<T>
        where T : struct, IComparable<T>
    {
        public T? MinValue;
        public T? MaxValue;
        public T[] AdditionalAllowedValues = Array.Empty<T>();

        public override void Validate(ConfigVarValue value)
        {
            T genericValue = this.ExtractGenericValue(value);

            bool isAllowedValue = this.AdditionalAllowedValues.Contains(genericValue);

            if (isAllowedValue)
            {
                base.Validate(value);
                return;
            }

            if (this.MinValue.HasValue && this.MaxValue.HasValue) // nicer error message is given in this case
            {
                if (!genericValue.BetweenInclusive(this.MinValue.Value, this.MaxValue.Value))
                    throw new ArgumentException($"{this.FinalSerializationName} must be between {this.MinValue.Value} and {this.MaxValue.Value}");
            }

            if (this.MinValue.HasValue && genericValue.IsLessThan(this.MinValue.Value))
                throw new ArgumentException($"{this.FinalSerializationName} can not be less than {this.MinValue.Value}");

            if (this.MaxValue.HasValue && genericValue.IsHigherThan(this.MaxValue.Value))
                throw new ArgumentException($"{this.FinalSerializationName} can not be higher than {this.MaxValue.Value}");

            base.Validate(value);
        }
    }

    public class IntConfigVar : NumericConfigVar<int>
    {
        public Func<int> GetValueCallbackInt
        {
            init => GetValueCallback = () => new ConfigVarValue { IntValue = value() };
        }

        public Action<int> SetValueCallbackInt 
        {
            init => SetValueCallback = (val) => value(val.IntValue);
        }

        public int DefaultValueInt { get => DefaultValue.IntValue; init => DefaultValue = new ConfigVarValue { IntValue = value }; }


        public IntConfigVar() { }

        public IntConfigVar(string description, int minValue, int maxValue)
        {
            this.Description = description;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

        public override ConfigVarValue LoadValueFromString(string str)
        {
            int value = int.Parse(str, CultureInfo.InvariantCulture);
            return new ConfigVarValue { IntValue = value };
        }

        public override string SaveValueToString(ConfigVarValue configVarValue)
        {
            return configVarValue.IntValue.ToString(CultureInfo.InvariantCulture);
        }

        public override int ExtractGenericValue(ConfigVarValue value) => value.IntValue;

        public override ConfigVarValue CreateValueFromGenericValue(int genericValue) => new ConfigVarValue { IntValue = genericValue };
    }

    public class FloatConfigVar : NumericConfigVar<float>
    {
        public Func<float> GetValueCallbackFloat
        {
            init => GetValueCallback = () => new ConfigVarValue { FloatValue = value() };
        }

        public Action<float> SetValueCallbackFloat
        {
            init => SetValueCallback = (val) => value(val.FloatValue);
        }

        public float DefaultValueFloat { get => DefaultValue.FloatValue; init => DefaultValue = new ConfigVarValue { FloatValue = value }; }


        public FloatConfigVar() { }

        public FloatConfigVar(string description, float minValue, float maxValue)
        {
            this.Description = description;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

        public override ConfigVarValue LoadValueFromString(string str)
        {
            float value = float.Parse(str, CultureInfo.InvariantCulture);
            return new ConfigVarValue { FloatValue = value };
        }

        public override string SaveValueToString(ConfigVarValue configVarValue)
        {
            return configVarValue.FloatValue.ToString(CultureInfo.InvariantCulture);
        }

        public override float ExtractGenericValue(ConfigVarValue value) => value.FloatValue;

        public override ConfigVarValue CreateValueFromGenericValue(float genericValue) => new ConfigVarValue { FloatValue = genericValue };

        public override void Validate(ConfigVarValue value)
        {
            if (float.IsNaN(value.FloatValue))
                throw new ArgumentException("Value can not be NaN");

            base.Validate(value);
        }
    }

    public class BoolConfigVar : ConfigVar<bool>
    {
        public bool DefaultValueBool { get => DefaultValue.BoolValue; init => DefaultValue = new ConfigVarValue { BoolValue = value }; }

        public BoolConfigVar() { }

        public BoolConfigVar(string description)
        {
            this.Description = description;
        }

        public override ConfigVarValue LoadValueFromString(string str)
        {
            if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)
                && (intValue == 1 || intValue == 0))
            {
                return new ConfigVarValue { BoolValue = intValue != 0 };
            }

            if (bool.TryParse(str, out bool boolValue))
                return new ConfigVarValue { BoolValue = boolValue };

            throw new ArgumentException($"Invalid boolean value provided for '{this.FinalSerializationName}', use 1|0 or true|false");
        }

        public override string SaveValueToString(ConfigVarValue configVarValue)
        {
            return configVarValue.BoolValue ? "1" : "0";
        }

        public override bool ExtractGenericValue(ConfigVarValue value) => value.BoolValue;

        public override ConfigVarValue CreateValueFromGenericValue(bool genericValue) => new ConfigVarValue { BoolValue = genericValue };
    }

    public class StringConfigVar : ConfigVar<string>
    {
        public int displayWidth = 200;
        public int maxNumCharacters = 0;

        public override ConfigVarValue CreateValueFromGenericValue(string genericValue) => new ConfigVarValue { StringValue = genericValue };

        public override string ExtractGenericValue(ConfigVarValue value) => value.StringValue;

        public override ConfigVarValue LoadValueFromString(string str)
        {
            return new ConfigVarValue { StringValue = str };
        }

        public override string SaveValueToString(ConfigVarValue configVarValue)
        {
            return configVarValue.StringValue;
        }
    }

    public class EnumConfigVar<T> : ConfigVar<T>
        where T : struct, Enum, IConvertible
    {
        public override ConfigVarValue CreateValueFromGenericValue(T genericValue) 
            => new ConfigVarValue { StringValue = genericValue.ToString() };

        public override T ExtractGenericValue(ConfigVarValue value) 
            => Enum.Parse<T>(value.StringValue, true);

        //static readonly Array s_enumValues = Enum.GetValues(typeof(T));
        //static readonly string[] s_enumNames = Enum.GetNames(typeof(T));


        public override ConfigVarValue LoadValueFromString(string str)
        {
            T enumValue = Enum.Parse<T>(str, true);
            //return new ConfigVarValue { Uint64Value = enumValue.ToUInt64(CultureInfo.InvariantCulture) };
            return new ConfigVarValue { StringValue = str };
        }

        public override string SaveValueToString(ConfigVarValue configVarValue)
        {
            //configVarValue.Uint64Value.ToType(Enum.GetUnderlyingType(typeof(T)), CultureInfo.InvariantCulture);

            Enum.Parse<T>(configVarValue.StringValue, true);
            //return configVarValue.Uint64Value.ToString(CultureInfo.InvariantCulture);
            return configVarValue.StringValue;
        }
    }

    public class MultipleOptionsConfigVar : ConfigVar
    {
        public string[] Options { get; set; } = Array.Empty<string>();

        public override ConfigVarValue LoadValueFromString(string str)
        {
            if (!this.Options.Contains(str))
                throw new ArgumentException($"Specified value '{str}' is not among possible options");

            return new ConfigVarValue { StringValue = str };
        }

        public override string SaveValueToString(ConfigVarValue configVarValue)
        {
            if (!this.Options.Contains(configVarValue.StringValue))
                throw new ArgumentException($"Specified value '{configVarValue.StringValue}' is not among possible options");

            return configVarValue.StringValue;
        }
    }

    /// <summary>
    /// Object that can register <see cref="ConfigVar"/>s.
    /// </summary>
    public interface IConfigVarRegistrator
    {
        public class Context
        {
            public List<ConfigVar> ConfigVars { get; } = new();
        }

        void Register(Context context)
        {
            IConfigVarRegistrator registrator = this;
            var type = registrator.GetType();
            var fields = type.GetFields(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            var properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!typeof(ConfigVar).IsAssignableFrom(field.FieldType))
                    continue;

                ConfigVar configVar = (ConfigVar)field.GetValue(registrator);
                context.ConfigVars.Add(configVar);
            }

            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;
                if (!typeof(ConfigVar).IsAssignableFrom(property.PropertyType))
                    continue;

                ConfigVar configVar = (ConfigVar)property.GetValue(registrator);
                context.ConfigVars.Add(configVar);
            }
        }
    }
}
