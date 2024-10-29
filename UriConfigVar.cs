using System;

namespace UGameCore.Utilities
{
    public class UriConfigVar : StringConfigVar
    {
        public Uri ValueUri { get => GetUriFromString(ValueString); set => ValueString = value?.ToString(); }
        public Uri DefaultValueUri { get => GetUriFromString(DefaultValueString); init => DefaultValueString = value?.ToString(); }

        public UriKind UriKind { get; init; } = UriKind.Absolute;


        Uri GetUriFromString(string str) => string.IsNullOrEmpty(str) ? null : new Uri(str, UriKind);

        public override void Validate(ConfigVarValue value)
        {
            string uriAsString = ExtractGenericValue(value);

            if (!string.IsNullOrEmpty(uriAsString))
            {
                if (!Uri.IsWellFormedUriString(uriAsString, UriKind))
                    throw new ArgumentException($"Uri is not well formed: {uriAsString}");

                if (!Uri.TryCreate(uriAsString, UriKind, out Uri _))
                    throw new ArgumentException($"Failed to create Uri from string: {uriAsString}");
            }

            base.Validate(value);
        }
    }
}
