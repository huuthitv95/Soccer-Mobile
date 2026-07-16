using System;

namespace SoccerMobilePro.SettingsUI
{
    public sealed class SettingsPanelPresenter
    {
        public const string DefaultLocale = "vi-VN";
        public const string EnglishLocale = "en";

        private readonly ISettingsUiService service;
        private string initialLocale;

        public SettingsPanelPresenter(ISettingsUiService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            initialLocale = Normalize(service.ConfirmedLocale);
            SelectedLocale = initialLocale;
            IsOpen = !service.HasConfirmedLocale;
        }

        public bool IsOpen { get; private set; }
        public string SelectedLocale { get; private set; }
        public string ErrorKey { get; private set; } = string.Empty;
        public bool IsFirstLaunch => !service.HasConfirmedLocale;

        public void Open()
        {
            initialLocale = Normalize(service.ConfirmedLocale);
            SelectedLocale = initialLocale;
            ErrorKey = string.Empty;
            IsOpen = true;
        }

        public void SelectLocale(string locale)
        {
            if (!IsSupportedLocale(locale)) return;
            SelectedLocale = locale;
            ErrorKey = string.Empty;
        }

        public bool Apply()
        {
            if (!service.TrySaveLocale(SelectedLocale, out string error))
            {
                ErrorKey = string.IsNullOrEmpty(error) ? "save_failed" : error;
                return false;
            }

            initialLocale = SelectedLocale;
            ErrorKey = string.Empty;
            IsOpen = false;
            return true;
        }

        public void Cancel()
        {
            SelectedLocale = initialLocale;
            ErrorKey = string.Empty;
            if (!IsFirstLaunch) IsOpen = false;
        }

        public bool Reset()
        {
            SelectedLocale = DefaultLocale;
            ErrorKey = string.Empty;
            return true;
        }

        public static bool IsSupportedLocale(string locale)
        {
            return string.Equals(locale, DefaultLocale, StringComparison.Ordinal)
                || string.Equals(locale, EnglishLocale, StringComparison.Ordinal);
        }

        private static string Normalize(string locale)
        {
            return IsSupportedLocale(locale) ? locale : DefaultLocale;
        }
    }
}
