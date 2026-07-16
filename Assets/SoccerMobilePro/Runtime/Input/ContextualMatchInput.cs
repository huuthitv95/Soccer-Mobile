using System;
using System.Collections.Generic;
using SoccerMobilePro.MatchCore;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SoccerMobilePro.Input
{
    public enum MatchInputContext
    {
        OnBall = 0,
        OffBall = 1,
        SetPiece = 2,
        Goalkeeper = 3,
        UI = 4
    }

    public enum HudControlPreset
    {
        Legacy = 0,
        Standard = 1,
        LeftHanded = 2
    }

    public readonly struct HudLayoutProfile
    {
        private HudLayoutProfile(
            HudControlPreset preset,
            Vector2 movementAnchor,
            Vector2 actionAnchor,
            float scale,
            float opacity,
            float deadZone)
        {
            Preset = preset;
            MovementAnchor = movementAnchor;
            ActionAnchor = actionAnchor;
            Scale = Mathf.Clamp(scale, 0.75f, 1.5f);
            Opacity = Mathf.Clamp01(opacity);
            DeadZone = Mathf.Clamp(deadZone, 0.05f, 0.5f);
        }

        public HudControlPreset Preset { get; }
        public Vector2 MovementAnchor { get; }
        public Vector2 ActionAnchor { get; }
        public float Scale { get; }
        public float Opacity { get; }
        public float DeadZone { get; }
        public bool IsLeftHanded => Preset == HudControlPreset.LeftHanded;

        public static HudLayoutProfile Create(HudControlPreset preset)
        {
            switch (preset)
            {
                case HudControlPreset.LeftHanded:
                    return new HudLayoutProfile(preset, new Vector2(0.84f, 0.76f), new Vector2(0.16f, 0.76f), 1f, 0.9f, 0.12f);
                case HudControlPreset.Standard:
                    return new HudLayoutProfile(preset, new Vector2(0.16f, 0.76f), new Vector2(0.84f, 0.76f), 1f, 0.9f, 0.12f);
                default:
                    return new HudLayoutProfile(HudControlPreset.Legacy, new Vector2(0.16f, 0.76f), new Vector2(0.84f, 0.76f), 1f, 1f, 0.1f);
            }
        }
    }

    public readonly struct InputBindingConflict
    {
        public InputBindingConflict(string mapName, string path, string firstAction, string secondAction)
        {
            MapName = mapName;
            Path = path;
            FirstAction = firstAction;
            SecondAction = secondAction;
        }

        public string MapName { get; }
        public string Path { get; }
        public string FirstAction { get; }
        public string SecondAction { get; }
    }

    public static class InputBindingConflictValidator
    {
        public static IReadOnlyList<InputBindingConflict> FindConflicts(InputActionAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            var conflicts = new List<InputBindingConflict>();
            foreach (InputActionMap map in asset.actionMaps)
            {
                var owners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (InputAction action in map.actions)
                {
                    foreach (InputBinding binding in action.bindings)
                    {
                        if (binding.isComposite || binding.isPartOfComposite || string.IsNullOrEmpty(binding.effectivePath))
                        {
                            continue;
                        }

                        string key = binding.effectivePath + "|" + binding.groups;
                        if (owners.TryGetValue(key, out string owner) && owner != action.name)
                        {
                            conflicts.Add(new InputBindingConflict(map.name, binding.effectivePath, owner, action.name));
                        }
                        else
                        {
                            owners[key] = action.name;
                        }
                    }
                }
            }

            return conflicts;
        }
    }

    public sealed class ContextualMatchInputAdapter : IDisposable
    {
        public const string OnBallMap = "Match_OnBall";
        public const string OffBallMap = "Match_OffBall";
        public const string SetPieceMap = "SetPiece";
        public const string GoalkeeperMap = "Goalkeeper";
        public const string UiMap = "UI";

        private readonly InputActionAsset asset;
        private readonly string actorId;
        private long nextSequenceId = 1;
        private InputActionMap activeMap;

        public ContextualMatchInputAdapter(InputActionAsset asset, string actorId)
        {
            this.asset = asset != null ? UnityEngine.Object.Instantiate(asset) : throw new ArgumentNullException(nameof(asset));
            this.actorId = string.IsNullOrWhiteSpace(actorId) ? "local-player" : actorId;
            SetContext(MatchInputContext.UI);
        }

        public MatchInputContext Context { get; private set; }
        public InputActionAsset Asset => asset;
        public InputActionMap ActiveMap => activeMap;

        public event Action<MatchCommand> CommandCreated;

        public void SetContext(MatchInputContext context)
        {
            asset.Disable();
            activeMap = asset.FindActionMap(GetMapName(context), true);
            Context = context;
            activeMap.Enable();
        }

        public bool TryCreateCommand(
            string actionName,
            int clientTick,
            Vector2 direction,
            float magnitude,
            out MatchCommand command)
        {
            command = default;
            if (activeMap == null || activeMap.FindAction(actionName, false) == null)
            {
                return false;
            }

            if (!TryResolveCommandType(Context, actionName, out MatchCommandType commandType))
            {
                return false;
            }

            Vector2 normalizedDirection = Vector2.ClampMagnitude(direction, 1f);
            command = new MatchCommand(
                nextSequenceId++,
                Math.Max(0, clientTick),
                actorId,
                commandType,
                directionX: normalizedDirection.x,
                directionY: normalizedDirection.y,
                magnitude: Mathf.Clamp01(magnitude),
                modifiers: Context.ToString());
            CommandCreated?.Invoke(command);
            return true;
        }

        public void Dispose()
        {
            asset.Disable();
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(asset);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        public static string GetMapName(MatchInputContext context)
        {
            switch (context)
            {
                case MatchInputContext.OnBall:
                    return OnBallMap;
                case MatchInputContext.OffBall:
                    return OffBallMap;
                case MatchInputContext.SetPiece:
                    return SetPieceMap;
                case MatchInputContext.Goalkeeper:
                    return GoalkeeperMap;
                default:
                    return UiMap;
            }
        }

        private static bool TryResolveCommandType(
            MatchInputContext context,
            string actionName,
            out MatchCommandType commandType)
        {
            if (actionName == "Move" || actionName == "Aim" || actionName == "Navigate")
            {
                commandType = context == MatchInputContext.SetPiece
                    ? MatchCommandType.SetPieceAim
                    : MatchCommandType.PlayerMove;
                return true;
            }

            switch (actionName)
            {
                case "Sprint": commandType = MatchCommandType.Sprint; return true;
                case "Pass": commandType = MatchCommandType.Pass; return true;
                case "ThroughPass": commandType = MatchCommandType.ThroughPass; return true;
                case "Shoot": commandType = MatchCommandType.Shoot; return true;
                case "Skill": commandType = MatchCommandType.Skill; return true;
                case "SwitchPlayer": commandType = MatchCommandType.SwitchPlayer; return true;
                case "Press": commandType = MatchCommandType.Press; return true;
                case "Tackle": commandType = MatchCommandType.Tackle; return true;
                case "SlideTackle": commandType = MatchCommandType.SlideTackle; return true;
                case "MatchUp": commandType = MatchCommandType.MatchUp; return true;
                case "Rush": commandType = MatchCommandType.GoalkeeperRush; return true;
                case "Dive": commandType = MatchCommandType.GoalkeeperDive; return true;
                case "Catch": commandType = MatchCommandType.GoalkeeperCatch; return true;
                case "Distribute": commandType = MatchCommandType.Distribute; return true;
                case "Power": commandType = MatchCommandType.SetPiecePower; return true;
                case "Curl": commandType = MatchCommandType.SetPieceCurl; return true;
                case "TriggerRunner": commandType = MatchCommandType.TriggerRunner; return true;
                case "Submit": commandType = MatchCommandType.Confirm; return true;
                case "Cancel": commandType = MatchCommandType.Cancel; return true;
                case "Pause": commandType = MatchCommandType.Pause; return true;
                default:
                    commandType = default;
                    return false;
            }
        }
    }

    public static class ContextualMatchInputRuntime
    {
        public const string FeaturePrefKey = "smp_contextual_input_v1";
        public const string ResourcePath = "Input/SoccerMobileControls";
        private const int QueueLimit = 64;

        private static readonly Queue<MatchCommand> commandQueue = new Queue<MatchCommand>(QueueLimit);

        public static ContextualMatchInputAdapter Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Shutdown();
            if (PlayerPrefs.GetInt(FeaturePrefKey, 0) != 1)
            {
                return;
            }

            InputActionAsset inputAsset = Resources.Load<InputActionAsset>(ResourcePath);
            if (inputAsset == null)
            {
                Debug.LogWarning("Contextual input asset missing; keeping legacy controls.");
                return;
            }

            Current = new ContextualMatchInputAdapter(inputAsset, "local-player");
            Current.CommandCreated += Enqueue;
            Subscribe(Current.Asset);
        }

        public static void SetContext(MatchInputContext context)
        {
            Current?.SetContext(context);
        }

        public static bool SubmitTouchAction(string actionName, Vector2 direction, float magnitude)
        {
            return Current != null && Current.TryCreateCommand(actionName, Time.frameCount, direction, magnitude, out _);
        }

        public static bool TryDequeue(out MatchCommand command)
        {
            if (commandQueue.Count == 0)
            {
                command = default;
                return false;
            }

            command = commandQueue.Dequeue();
            return true;
        }

        public static void Shutdown()
        {
            commandQueue.Clear();
            if (Current == null)
            {
                return;
            }

            Current.CommandCreated -= Enqueue;
            Current.Dispose();
            Current = null;
        }

        private static void Subscribe(InputActionAsset inputAsset)
        {
            foreach (InputActionMap map in inputAsset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    action.performed += OnActionPerformed;
                }
            }
        }

        private static void OnActionPerformed(InputAction.CallbackContext context)
        {
            if (Current == null || context.action.actionMap != Current.ActiveMap)
            {
                return;
            }

            Vector2 direction = context.action.expectedControlType == "Vector2"
                ? context.ReadValue<Vector2>()
                : Vector2.zero;
            Current.TryCreateCommand(context.action.name, Time.frameCount, direction, 1f, out _);
        }

        private static void Enqueue(MatchCommand command)
        {
            if (commandQueue.Count == QueueLimit)
            {
                commandQueue.Dequeue();
            }

            commandQueue.Enqueue(command);
        }
    }
}
