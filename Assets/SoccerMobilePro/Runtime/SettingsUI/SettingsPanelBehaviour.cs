using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SoccerMobilePro.Platform;

namespace SoccerMobilePro.SettingsUI
{
    public sealed class SettingsPanelBehaviour : MonoBehaviour
    {
        private static readonly Color BackdropColor = new Color(0.015f, 0.035f, 0.07f, 0.96f);
        private static readonly Color PanelColor = new Color(0.06f, 0.10f, 0.16f, 1f);
        private static readonly Color AccentColor = new Color(0.12f, 0.78f, 0.62f, 1f);
        private static readonly Color MutedColor = new Color(0.18f, 0.24f, 0.31f, 1f);

        private SettingsPanelPresenter presenter;
        private GameObject panel;
        private TMP_Text title;
        private TMP_Text description;
        private TMP_Text error;
        private TMP_Text applyLabel;
        private TMP_Text cancelLabel;
        private TMP_Text resetLabel;
        private Button vietnameseButton;
        private Button englishButton;
        private Button settingsButton;
        private Button resetButton;
        private Button cancelButton;
        private Button applyButton;

        public SettingsPanelPresenter Presenter => presenter;
        public bool IsPanelVisible => panel != null && panel.activeSelf;
        public string CurrentTitle => title == null ? string.Empty : title.text;

        public void Initialize(ISettingsUiService service)
        {
            presenter = new SettingsPanelPresenter(service);
            LocalizationBridge.TrySelect(presenter.SelectedLocale);
            BuildUi();
            Refresh();
            StartCoroutine(RefreshAfterLocalizationInitialization());
        }

        public void Open()
        {
            presenter.Open();
            Refresh();
        }

        public void SelectLocale(string locale)
        {
            presenter.SelectLocale(locale);
            LocalizationBridge.TrySelect(locale);
            Refresh();
        }

        public bool ApplySelection()
        {
            bool succeeded = presenter.Apply();
            if (succeeded) LocalizationBridge.TrySelect(presenter.SelectedLocale);
            Refresh();
            return succeeded;
        }

        public void CancelChanges()
        {
            presenter.Cancel();
            LocalizationBridge.TrySelect(presenter.SelectedLocale);
            Refresh();
        }

        public bool ResetToDefaults()
        {
            bool succeeded = presenter.Reset();
            if (succeeded) LocalizationBridge.TrySelect(presenter.SelectedLocale);
            Refresh();
            return succeeded;
        }

        private IEnumerator RefreshAfterLocalizationInitialization()
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
                yield return LocalizationSettings.InitializationOperation;
            if (presenter == null) yield break;
            LocalizationBridge.TrySelect(presenter.SelectedLocale);
            Refresh();
        }

        private void BuildUi()
        {
            EnsureEventSystem();

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform safeRoot = CreateRect("SafeArea", transform, Vector2.zero, Vector2.one);
            safeRoot.gameObject.AddComponent<SettingsSafeArea>();

            settingsButton = CreateButton("SettingsButton", safeRoot, new Vector2(0.80f, 0.88f), new Vector2(0.98f, 0.98f), MutedColor, out TMP_Text settingsLabel);
            settingsLabel.text = "SET";
            settingsLabel.fontSize = 28;
            settingsButton.onClick.AddListener(Open);

            panel = CreateRect("LocalizationSettingsPanel", safeRoot, Vector2.zero, Vector2.one).gameObject;
            Image backdrop = panel.AddComponent<Image>();
            backdrop.color = BackdropColor;

            RectTransform card = CreateRect("Card", panel.transform, new Vector2(0.10f, 0.14f), new Vector2(0.90f, 0.86f));
            card.gameObject.AddComponent<Image>().color = PanelColor;

            title = CreateText("Title", card, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.94f), 34, TextAnchor.MiddleCenter);
            description = CreateText("Description", card, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.80f), 20, TextAnchor.MiddleCenter);

            vietnameseButton = CreateButton("Vietnamese", card, new Vector2(0.10f, 0.49f), new Vector2(0.48f, 0.64f), MutedColor, out TMP_Text viLabel);
            viLabel.text = "Tiếng Việt";
            vietnameseButton.onClick.AddListener(() => SelectLocale(SettingsPanelPresenter.DefaultLocale));

            englishButton = CreateButton("English", card, new Vector2(0.52f, 0.49f), new Vector2(0.90f, 0.64f), MutedColor, out TMP_Text enLabel);
            enLabel.text = "English";
            englishButton.onClick.AddListener(() => SelectLocale(SettingsPanelPresenter.EnglishLocale));

            error = CreateText("Error", card, new Vector2(0.08f, 0.37f), new Vector2(0.92f, 0.47f), 18, TextAnchor.MiddleCenter);
            error.color = new Color(1f, 0.42f, 0.42f);

            resetButton = CreateButton("Reset", card, new Vector2(0.08f, 0.13f), new Vector2(0.34f, 0.29f), MutedColor, out resetLabel);
            resetButton.onClick.AddListener(() => ResetToDefaults());
            cancelButton = CreateButton("Cancel", card, new Vector2(0.37f, 0.13f), new Vector2(0.63f, 0.29f), MutedColor, out cancelLabel);
            cancelButton.onClick.AddListener(CancelChanges);
            applyButton = CreateButton("Apply", card, new Vector2(0.66f, 0.13f), new Vector2(0.92f, 0.29f), AccentColor, out applyLabel);
            applyButton.onClick.AddListener(() => ApplySelection());

            vietnameseButton.navigation = ExplicitNavigation(null, englishButton, null, resetButton);
            englishButton.navigation = ExplicitNavigation(vietnameseButton, null, null, applyButton);
            resetButton.navigation = ExplicitNavigation(null, cancelButton, vietnameseButton, null);
            cancelButton.navigation = ExplicitNavigation(resetButton, applyButton, vietnameseButton, null);
            applyButton.navigation = ExplicitNavigation(cancelButton, null, englishButton, null);
        }

        private void Refresh()
        {
            bool vietnamese = presenter.SelectedLocale == SettingsPanelPresenter.DefaultLocale;
            title.text = LocalizationBridge.Get("settings.title", vietnamese ? "Ngôn ngữ & cài đặt" : "Language & settings");
            description.text = LocalizationBridge.Get("settings.description", vietnamese ? "Chọn ngôn ngữ hiển thị" : "Choose display language");
            applyLabel.text = LocalizationBridge.Get("settings.apply", vietnamese ? "Áp dụng" : "Apply");
            cancelLabel.text = LocalizationBridge.Get("settings.cancel", vietnamese ? "Hủy" : "Cancel");
            resetLabel.text = LocalizationBridge.Get("settings.reset", vietnamese ? "Đặt lại" : "Reset");
            error.text = LocalizeError(presenter.ErrorKey, vietnamese);

            SetButtonColor(vietnameseButton, vietnamese ? AccentColor : MutedColor);
            SetButtonColor(englishButton, vietnamese ? MutedColor : AccentColor);
            panel.SetActive(presenter.IsOpen);
            settingsButton.gameObject.SetActive(!presenter.IsOpen);

            if (presenter.IsOpen && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(vietnamese ? vietnameseButton.gameObject : englishButton.gameObject);
            }
        }

        private static string LocalizeError(string errorKey, bool vietnamese)
        {
            if (string.IsNullOrEmpty(errorKey)) return string.Empty;
            if (errorKey == "unsupported_locale")
                return LocalizationBridge.Get("settings.error.unsupported", vietnamese ? "Ngôn ngữ chưa được hỗ trợ." : "Unsupported language.");
            return LocalizationBridge.Get("settings.error.save", vietnamese ? "Không thể lưu cài đặt. Vui lòng thử lại." : "Could not save settings. Please try again.");
        }

        private static Navigation ExplicitNavigation(Selectable left, Selectable right, Selectable up, Selectable down)
        {
            return new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = left,
                selectOnRight = right,
                selectOnUp = up,
                selectOnDown = down
            };
        }

        private static void SetButtonColor(Button button, Color normal)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.selectedColor = Color.Lerp(normal, Color.white, 0.18f);
            colors.highlightedColor = colors.selectedColor;
            button.colors = colors;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var item = new GameObject(name, typeof(RectTransform));
            item.transform.SetParent(parent, false);
            var rect = (RectTransform)item.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static TMP_Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int size, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF - Fallback");
            text.fontSize = size;
            text.alignment = ToTmpAlignment(alignment);
            text.color = Color.white;
            text.enableAutoSizing = true;
            text.fontSizeMin = 14;
            text.fontSizeMax = size;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, out TMP_Text label)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            Button button = rect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = Color.Lerp(color, Color.black, 0.16f);
            button.colors = colors;
            label = CreateText("Label", rect, Vector2.zero, Vector2.one, 22, TextAnchor.MiddleCenter);
            return button;
        }

        private static TextAlignmentOptions ToTmpAlignment(TextAnchor alignment)
        {
            return alignment == TextAnchor.MiddleCenter ? TextAlignmentOptions.Center : TextAlignmentOptions.Center;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var eventSystem = new GameObject("SettingsUI EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(eventSystem);
        }
    }

    internal static class LocalizationBridge
    {
        private const string TableName = "SettingsUI";

        public static bool TrySelect(string localeCode)
        {
            try
            {
                if (!LocalizationSettings.InitializationOperation.IsDone) return false;
                Locale locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(localeCode));
                if (locale == null) return false;
                LocalizationSettings.SelectedLocale = locale;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string Get(string key, string fallback)
        {
            try
            {
                if (!LocalizationSettings.InitializationOperation.IsDone) return fallback;
                StringTable table = LocalizationSettings.StringDatabase.GetTable(TableName);
                StringTableEntry entry = table == null ? null : table.GetEntry(key);
                string localized = entry == null ? null : entry.LocalizedValue;
                return string.IsNullOrEmpty(localized) ? fallback : localized;
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }

    [RequireComponent(typeof(RectTransform))]
    public sealed class SettingsSafeArea : MonoBehaviour
    {
        private Rect lastArea;
        private Vector2Int lastSize;

        private void OnEnable() => Apply(Screen.safeArea, Screen.width, Screen.height);

        private void Update()
        {
            if (lastArea != Screen.safeArea || lastSize.x != Screen.width || lastSize.y != Screen.height)
                Apply(Screen.safeArea, Screen.width, Screen.height);
        }

        public void Apply(Rect safeArea, int width, int height)
        {
            if (width <= 0 || height <= 0) return;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
            rect.anchorMax = new Vector2(safeArea.xMax / width, safeArea.yMax / height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            lastArea = safeArea;
            lastSize = new Vector2Int(width, height);
        }
    }

    public static class SettingsUiBootstrap
    {
        private const string MainMenuScene = SceneIds.MainMenu;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            ConfigureFileRepositoryIfNeeded();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void ConfigureFileRepositoryIfNeeded()
        {
            if (SettingsUiServices.IsConfigured) return;
            try
            {
                DefaultSettingsRegistry registry = SoccerMobileSettingsRegistry.CreateDefault();
                var migrator = new VersionedSettingsMigrator(2);
                var initial = new SettingsSnapshot(
                    string.Empty,
                    2,
                    0,
                    new Dictionary<string, string>(),
                    new Dictionary<string, string>(),
                    new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero));
                string path = Path.Combine(Application.persistentDataPath, "settings", "settings.snapshot");
                string deviceLocale = ResolveDeviceLocale();
                SettingsUiServices.Configure(new FileSettingsRepository(path, registry, migrator, initial), deviceLocale);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Settings file repository unavailable; using session-only fallback. " + exception.Message);
            }
        }

        private static string ResolveDeviceLocale()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Vietnamese: return SettingsPanelPresenter.DefaultLocale;
                case SystemLanguage.English: return SettingsPanelPresenter.EnglishLocale;
                default: return CultureInfo.CurrentUICulture.Name;
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!string.Equals(scene.name, MainMenuScene, StringComparison.Ordinal)) return;
            if (UnityEngine.Object.FindObjectOfType<SettingsPanelBehaviour>() != null) return;

            var root = new GameObject("Soccer Mobile Pro Settings UI");
            var behaviour = root.AddComponent<SettingsPanelBehaviour>();
            behaviour.Initialize(SettingsUiServices.Current);
        }
    }
}
