using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SoccerMobilePro.Platform;
using SoccerMobilePro.SettingsUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace SoccerMobilePro.SettingsUI.PlayModeTests
{
    public sealed class SettingsPanelPlayModeTests
    {
        private GameObject root;

        [TearDown]
        public void TearDown()
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            if (EventSystem.current != null) UnityEngine.Object.DestroyImmediate(EventSystem.current.gameObject);
            SettingsUiServices.ResetComposition();
        }

        [UnityTest]
        public IEnumerator FirstLaunch_RequiresApplyAndPersistsThroughService()
        {
            var service = new StubService(false, SettingsPanelPresenter.DefaultLocale);
            SettingsPanelBehaviour panel = CreatePanel(service);
            yield return null;

            Assert.That(panel.IsPanelVisible, Is.True);
            panel.CancelChanges();
            Assert.That(panel.IsPanelVisible, Is.True, "First launch cannot be dismissed before locale confirmation.");

            string vietnameseTitle = panel.CurrentTitle;
            panel.SelectLocale(SettingsPanelPresenter.EnglishLocale);
            Assert.That(panel.CurrentTitle, Is.Not.EqualTo(vietnameseTitle), "Selecting a locale should hot-switch visible copy before Apply.");
            Assert.That(panel.ApplySelection(), Is.True);
            Assert.That(panel.IsPanelVisible, Is.False);
            Assert.That(service.SavedLocale, Is.EqualTo(SettingsPanelPresenter.EnglishLocale));
        }

        [UnityTest]
        public IEnumerator Cancel_RestoresPreviouslyConfirmedLocale()
        {
            var service = new StubService(true, SettingsPanelPresenter.EnglishLocale);
            SettingsPanelBehaviour panel = CreatePanel(service);
            yield return null;

            Assert.That(panel.IsPanelVisible, Is.False);
            panel.Open();
            panel.SelectLocale(SettingsPanelPresenter.DefaultLocale);
            panel.CancelChanges();

            Assert.That(panel.IsPanelVisible, Is.False);
            Assert.That(panel.Presenter.SelectedLocale, Is.EqualTo(SettingsPanelPresenter.EnglishLocale));
            Assert.That(service.SaveCalls, Is.Zero);
        }

        [UnityTest]
        public IEnumerator SaveFailure_ShowsFallbackAndKeepsPanelOpen()
        {
            var service = new StubService(false, "unsupported") { FailSave = true };
            SettingsPanelBehaviour panel = CreatePanel(service);
            yield return null;

            Assert.That(panel.Presenter.SelectedLocale, Is.EqualTo(SettingsPanelPresenter.DefaultLocale));
            Assert.That(panel.ApplySelection(), Is.False);
            Assert.That(panel.IsPanelVisible, Is.True);
            Assert.That(panel.Presenter.ErrorKey, Is.EqualTo("save_failed"));
        }

        [UnityTest]
        public IEnumerator Reset_UsesVietnameseDefault()
        {
            var service = new StubService(true, SettingsPanelPresenter.EnglishLocale);
            SettingsPanelBehaviour panel = CreatePanel(service);
            yield return null;

            panel.Open();
            string englishTitle = panel.CurrentTitle;
            Assert.That(panel.ResetToDefaults(), Is.True);
            Assert.That(panel.Presenter.SelectedLocale, Is.EqualTo(SettingsPanelPresenter.DefaultLocale));
            Assert.That(panel.CurrentTitle, Is.Not.EqualTo(englishTitle), "Reset should hot-switch copy to the default locale.");
            panel.CancelChanges();
            Assert.That(panel.Presenter.SelectedLocale, Is.EqualTo(SettingsPanelPresenter.EnglishLocale));
            Assert.That(service.SaveCalls, Is.Zero);
        }

        [Test]
        public void PlatformService_SuggestsDeviceEnglishAndSavesAtomically()
        {
            DefaultSettingsRegistry registry = SoccerMobileSettingsRegistry.CreateDefault();
            var initial = new SettingsSnapshot(
                string.Empty,
                2,
                4,
                new Dictionary<string, string>
                {
                    [PlatformSettingsUiService.DisplayLocaleKey] = SettingsPanelPresenter.DefaultLocale,
                    [PlatformSettingsUiService.LocaleConfirmedKey] = "false"
                },
                null,
                new DateTimeOffset(2026, 7, 16, 0, 0, 0, TimeSpan.Zero));
            var inner = new InMemorySettingsRepository(registry, new VersionedSettingsMigrator(2), initial);
            var repository = new TrackingRepository(inner);
            var service = new PlatformSettingsUiService(
                repository,
                "en-US",
                () => new DateTimeOffset(2026, 7, 16, 1, 0, 0, TimeSpan.Zero));

            Assert.That(service.HasConfirmedLocale, Is.False);
            Assert.That(service.ConfirmedLocale, Is.EqualTo(SettingsPanelPresenter.EnglishLocale));
            Assert.That(service.TrySaveLocale(SettingsPanelPresenter.EnglishLocale, out string error), Is.True, error);

            Assert.That(repository.SingleWriteCalls, Is.Zero);
            Assert.That(repository.BatchWriteCalls, Is.EqualTo(1));
            Assert.That(repository.LastBatch, Has.Count.EqualTo(2));
            Assert.That(repository.LastBatch[PlatformSettingsUiService.DisplayLocaleKey], Is.EqualTo("en"));
            Assert.That(repository.LastBatch[PlatformSettingsUiService.LocaleConfirmedKey], Is.EqualTo("true"));
            Assert.That(repository.Load().Revision, Is.EqualTo(5));
            Assert.That(service.HasConfirmedLocale, Is.True);
        }

        [UnityTest]
        public IEnumerator SafeArea_ConvertsPixelsToAnchors()
        {
            root = new GameObject("CanvasRoot", typeof(RectTransform));
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 500f);
            var safeAreaObject = new GameObject("SafeArea", typeof(RectTransform), typeof(SettingsSafeArea));
            safeAreaObject.transform.SetParent(root.transform, false);
            var safeArea = safeAreaObject.GetComponent<SettingsSafeArea>();
            safeArea.enabled = false;
            safeArea.Apply(new Rect(100f, 50f, 800f, 400f), 1000, 500);
            yield return null;

            var rect = safeAreaObject.GetComponent<RectTransform>();
            Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(0.1f, 0.1f)));
            Assert.That(rect.anchorMax, Is.EqualTo(new Vector2(0.9f, 0.9f)));
        }

        [UnityTest]
        public IEnumerator VietnameseStringTable_LoadsExactDiacriticsAndExcludesQaPseudoLocale()
        {
            yield return LocalizationSettings.InitializationOperation;
            Locale vietnamese = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("vi-VN"));
            Assert.That(vietnamese, Is.Not.Null);
            Assert.That(LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("qps-ploc")), Is.Null);
            LocalizationSettings.SelectedLocale = vietnamese;
            var tableOperation = LocalizationSettings.StringDatabase.GetTableAsync("SettingsUI", vietnamese);
            yield return tableOperation;
            StringTable table = tableOperation.Result;

            Assert.That(table, Is.Not.Null);
            Assert.That(table.GetEntry("settings.title").LocalizedValue, Is.EqualTo("Ngôn ngữ & cài đặt"));
            Assert.That(table.GetEntry("settings.apply").LocalizedValue, Is.EqualTo("Áp dụng"));
            Assert.That(table.GetEntry("settings.error.save").LocalizedValue, Is.EqualTo("Không thể lưu cài đặt. Vui lòng thử lại."));
        }

        [UnityTest]
        public IEnumerator FocusNavigation_CoversLocaleAndActionButtons()
        {
            var service = new StubService(false, SettingsPanelPresenter.DefaultLocale);
            SettingsPanelBehaviour panel = CreatePanel(service);
            yield return null;

            Button vietnamese = GameObject.Find("Vietnamese").GetComponent<Button>();
            Button english = GameObject.Find("English").GetComponent<Button>();
            Button reset = GameObject.Find("Reset").GetComponent<Button>();
            Button cancel = GameObject.Find("Cancel").GetComponent<Button>();
            Button apply = GameObject.Find("Apply").GetComponent<Button>();

            Assert.That(vietnamese.navigation.selectOnRight, Is.SameAs(english));
            Assert.That(vietnamese.navigation.selectOnDown, Is.SameAs(reset));
            Assert.That(english.navigation.selectOnDown, Is.SameAs(apply));
            Assert.That(reset.navigation.selectOnRight, Is.SameAs(cancel));
            Assert.That(cancel.navigation.selectOnRight, Is.SameAs(apply));
            Assert.That(apply.navigation.selectOnUp, Is.SameAs(english));
        }

        private SettingsPanelBehaviour CreatePanel(ISettingsUiService service)
        {
            root = new GameObject("Settings UI Test");
            var panel = root.AddComponent<SettingsPanelBehaviour>();
            panel.Initialize(service);
            return panel;
        }

        private sealed class StubService : ISettingsUiService
        {
            private string locale;

            public StubService(bool confirmed, string locale)
            {
                HasConfirmedLocale = confirmed;
                this.locale = locale;
            }

            public bool HasConfirmedLocale { get; private set; }
            public string ConfirmedLocale => locale;
            public string SavedLocale { get; private set; }
            public int SaveCalls { get; private set; }
            public bool FailSave { get; set; }

            public bool TrySaveLocale(string value, out string error)
            {
                SaveCalls++;
                if (FailSave)
                {
                    error = "save_failed";
                    return false;
                }

                locale = value;
                SavedLocale = value;
                HasConfirmedLocale = true;
                error = string.Empty;
                return true;
            }

        }

        private sealed class TrackingRepository : ISettingsRepository
        {
            private readonly ISettingsRepository inner;

            public TrackingRepository(ISettingsRepository inner)
            {
                this.inner = inner;
            }

            public int SingleWriteCalls { get; private set; }
            public int BatchWriteCalls { get; private set; }
            public IReadOnlyDictionary<string, string> LastBatch { get; private set; }

            public SettingsSnapshot Load() => inner.Load();

            public SettingsWriteStatus TryWrite(string key, string value, long expectedRevision, DateTimeOffset nowUtc)
            {
                SingleWriteCalls++;
                return inner.TryWrite(key, value, expectedRevision, nowUtc);
            }

            public SettingsWriteStatus TryWriteBatch(IReadOnlyDictionary<string, string> updates, long expectedRevision, DateTimeOffset nowUtc)
            {
                BatchWriteCalls++;
                LastBatch = new Dictionary<string, string>(updates, StringComparer.Ordinal);
                return inner.TryWriteBatch(updates, expectedRevision, nowUtc);
            }
        }
    }
}
