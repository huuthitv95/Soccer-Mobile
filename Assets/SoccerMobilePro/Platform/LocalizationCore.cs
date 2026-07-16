using System;
using System.Collections.Generic;

namespace SoccerMobilePro.Platform
{
    public readonly struct LocaleResolutionResult
    {
        public LocaleResolutionResult(LocalePreference preference, bool requiresConfirmation)
        {
            Preference = preference ?? throw new ArgumentNullException(nameof(preference));
            RequiresConfirmation = requiresConfirmation;
        }

        public LocalePreference Preference { get; }
        public bool RequiresConfirmation { get; }
    }

    public interface ILocaleResolver
    {
        LocaleResolutionResult Resolve(
            LocalePreference accountPreference,
            LocalePreference localPreference,
            string deviceLocale,
            DateTimeOffset nowUtc);
    }

    public sealed class DefaultLocaleResolver : ILocaleResolver
    {
        public const string FallbackLocale = "vi-VN";
        private static readonly string[] SupportedLocales = { "vi-VN", "en" };

        public LocaleResolutionResult Resolve(
            LocalePreference accountPreference,
            LocalePreference localPreference,
            string deviceLocale,
            DateTimeOffset nowUtc)
        {
            string locale;
            if (TryGetSupported(accountPreference, out locale))
                return Create(locale, "account", accountPreference.Version, accountPreference.VoiceLocale, nowUtc, false);

            if (TryGetSupported(localPreference, out locale))
                return Create(locale, "local", localPreference.Version, localPreference.VoiceLocale, nowUtc, false);

            if (TryGetExact(deviceLocale, out locale))
                return Create(locale, "device-exact", 1, locale, nowUtc, true);

            if (TryGetLanguage(deviceLocale, out locale))
                return Create(locale, "device-language", 1, locale, nowUtc, true);

            return Create(FallbackLocale, "fallback", 1, FallbackLocale, nowUtc, true);
        }

        public static bool IsSupported(string locale)
        {
            return TryGetExact(locale, out _);
        }

        private static bool TryGetSupported(LocalePreference preference, out string locale)
        {
            locale = null;
            return preference != null && TryGetExact(preference.Locale, out locale);
        }

        private static bool TryGetExact(string candidate, out string locale)
        {
            foreach (string supported in SupportedLocales)
            {
                if (string.Equals(candidate, supported, StringComparison.OrdinalIgnoreCase))
                {
                    locale = supported;
                    return true;
                }
            }

            locale = null;
            return false;
        }

        private static bool TryGetLanguage(string candidate, out string locale)
        {
            string language = string.IsNullOrWhiteSpace(candidate)
                ? string.Empty
                : candidate.Split('-', '_')[0];
            if (string.Equals(language, "vi", StringComparison.OrdinalIgnoreCase))
            {
                locale = "vi-VN";
                return true;
            }

            if (string.Equals(language, "en", StringComparison.OrdinalIgnoreCase))
            {
                locale = "en";
                return true;
            }

            locale = null;
            return false;
        }

        private static LocaleResolutionResult Create(
            string locale,
            string source,
            int version,
            string voiceLocale,
            DateTimeOffset nowUtc,
            bool requiresConfirmation)
        {
            string supportedVoice = IsSupported(voiceLocale) ? voiceLocale : locale;
            var preference = new LocalePreference(locale, supportedVoice, source, nowUtc, version);
            return new LocaleResolutionResult(preference, requiresConfirmation);
        }
    }
}
