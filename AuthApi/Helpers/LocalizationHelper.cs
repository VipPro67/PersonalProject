using AuthApi.Resources;
using Microsoft.Extensions.Localization;

namespace AuthApi.Helpers
{
    public class LocalizationHelper
    {
        private readonly IStringLocalizer<Resource> _localization;

        public LocalizationHelper(IStringLocalizer<Resource> localization)
        {
            _localization = localization;
        }

        public string GetComplexMessage(string objectName, string message, params object[] args)
        {
            return $"{_localization[objectName]} {_localization[message, args]}";
        }
        public string GetSimpleMessage(string resourceKey)
        {
            return $"{_localization[resourceKey]}";
        }
    }
}