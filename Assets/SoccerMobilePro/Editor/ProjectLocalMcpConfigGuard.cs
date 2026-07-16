using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SoccerMobilePro.Editor
{
    [InitializeOnLoad]
    public static class ProjectLocalMcpConfigGuard
    {
        public const string LocalServerUrl = "http://localhost:22113";

        private const double PollIntervalSeconds = 2d;
        private static readonly Regex RoutedUrlPattern = new Regex(
            "(?m)^url\\s*=\\s*\"http://localhost:22113/p/[A-Za-z0-9_-]+\"\\s*$",
            RegexOptions.CultureInvariant);

        private static readonly string ConfigPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", ".codex", "config.toml"));

        private static double nextPoll;
        private static long observedWriteTicks = long.MinValue;

        static ProjectLocalMcpConfigGuard()
        {
            EditorApplication.delayCall += () => NormalizeProjectConfig();
            EditorApplication.update += Poll;
        }

        public static string NormalizeConfigText(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return RoutedUrlPattern.Replace(text, $"url = \"{LocalServerUrl}\"");
        }

        public static bool NormalizeProjectConfig()
        {
            if (!File.Exists(ConfigPath)) return false;

            string current = File.ReadAllText(ConfigPath, Encoding.UTF8);
            string normalized = NormalizeConfigText(current);
            observedWriteTicks = File.GetLastWriteTimeUtc(ConfigPath).Ticks;
            if (string.Equals(current, normalized, StringComparison.Ordinal)) return false;

            File.WriteAllText(ConfigPath, normalized, new UTF8Encoding(false));
            observedWriteTicks = File.GetLastWriteTimeUtc(ConfigPath).Ticks;
            Debug.LogWarning("[Soccer Mobile Pro] Restored the project-local Unity MCP endpoint to http://localhost:22113.");
            return true;
        }

        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextPoll) return;
            nextPoll = EditorApplication.timeSinceStartup + PollIntervalSeconds;
            if (!File.Exists(ConfigPath)) return;

            long writeTicks = File.GetLastWriteTimeUtc(ConfigPath).Ticks;
            if (writeTicks == observedWriteTicks) return;
            NormalizeProjectConfig();
        }
    }
}
