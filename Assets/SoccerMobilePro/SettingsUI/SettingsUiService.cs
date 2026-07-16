using System;
using System.IO;
using System.Collections.Generic;
using SoccerMobilePro.Platform;

namespace SoccerMobilePro.SettingsUI
{
    public interface ISettingsUiService
    {
        bool HasConfirmedLocale { get; }
        string ConfirmedLocale { get; }
        bool TrySaveLocale(string locale, out string error);
    }

    public static class SettingsUiServices
    {
        private static ISettingsUiService current;

        public static ISettingsUiService Current => current ?? (current = new SessionSettingsUiService());
        public static bool IsConfigured => current != null;

        public static void Configure(ISettingsUiService service)
        {
            current = service ?? throw new ArgumentNullException(nameof(service));
        }

        public static void Configure(ISettingsRepository repository, string deviceLocale, Func<DateTimeOffset> utcNow = null)
        {
            Configure(new PlatformSettingsUiService(repository, deviceLocale, utcNow));
        }

        public static void ResetComposition()
        {
            current = null;
        }
    }

    public sealed class PlatformSettingsUiService : ISettingsUiService
    {
        public const string DisplayLocaleKey = "locale.text";
        public const string LocaleConfirmedKey = "locale.confirmed";
        private readonly ISettingsRepository repository;
        private readonly Func<DateTimeOffset> utcNow;
        private readonly string suggestedLocale;

        public PlatformSettingsUiService(ISettingsRepository repository, string deviceLocale, Func<DateTimeOffset> utcNow = null)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
            LocaleResolutionResult resolution = new DefaultLocaleResolver().Resolve(null, null, deviceLocale, this.utcNow());
            suggestedLocale = resolution.Preference.Locale;
        }

        public bool HasConfirmedLocale
        {
            get
            {
                SettingsSnapshot snapshot = repository.Load();
                return snapshot.Values.TryGetValue(LocaleConfirmedKey, out string confirmed)
                    && string.Equals(confirmed, "true", StringComparison.Ordinal)
                    && snapshot.Values.TryGetValue(DisplayLocaleKey, out string locale)
                    && SettingsPanelPresenter.IsSupportedLocale(locale);
            }
        }

        public string ConfirmedLocale
        {
            get
            {
                SettingsSnapshot snapshot = repository.Load();
                return snapshot.Values.TryGetValue(DisplayLocaleKey, out string locale)
                    && HasConfirmedLocale
                    && SettingsPanelPresenter.IsSupportedLocale(locale)
                        ? locale
                        : suggestedLocale;
            }
        }

        public bool TrySaveLocale(string locale, out string error)
        {
            if (!SettingsPanelPresenter.IsSupportedLocale(locale))
            {
                error = "unsupported_locale";
                return false;
            }

            try
            {
                var updates = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [DisplayLocaleKey] = locale,
                    [LocaleConfirmedKey] = "true"
                };
                SettingsSnapshot snapshot = repository.Load();
                SettingsWriteStatus status = repository.TryWriteBatch(updates, snapshot.Revision, utcNow());
                error = MapError(status);
                return status == SettingsWriteStatus.Saved;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error = "save_failed";
                return false;
            }
        }

        private static string MapError(SettingsWriteStatus status)
        {
            switch (status)
            {
                case SettingsWriteStatus.Saved: return string.Empty;
                case SettingsWriteStatus.RevisionConflict: return "revision_conflict";
                case SettingsWriteStatus.ReadOnly: return "read_only";
                case SettingsWriteStatus.InvalidKey: return "missing_locale_definition";
                default: return "save_failed";
            }
        }
    }

    internal sealed class SessionSettingsUiService : ISettingsUiService
    {
        private string locale = SettingsPanelPresenter.DefaultLocale;

        public bool HasConfirmedLocale { get; private set; }
        public string ConfirmedLocale => locale;

        public bool TrySaveLocale(string value, out string error)
        {
            if (!SettingsPanelPresenter.IsSupportedLocale(value))
            {
                error = "unsupported_locale";
                return false;
            }

            locale = value;
            HasConfirmedLocale = true;
            error = string.Empty;
            return true;
        }

    }
}
