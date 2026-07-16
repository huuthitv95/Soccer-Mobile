using NUnit.Framework;
using SoccerMobilePro.Editor;

namespace SoccerMobilePro.MatchCore.Tests
{
    public sealed class ProjectLocalMcpConfigGuardTests
    {
        [Test]
        public void NormalizeConfigText_ReplacesOnlyRoutedLocalEndpoint()
        {
            const string input = "[mcp_servers.ai-game-developer]\nurl = \"http://localhost:22113/p/21bc7938\"\n";

            string normalized = ProjectLocalMcpConfigGuard.NormalizeConfigText(input);

            Assert.That(normalized, Does.Contain("url = \"http://localhost:22113\""));
            Assert.That(normalized, Does.Not.Contain("/p/21bc7938"));
        }

        [Test]
        public void NormalizeConfigText_PreservesUnrelatedServerUrls()
        {
            const string input = "[mcp_servers.blender]\nurl = \"http://localhost:9876/p/example\"\n";

            Assert.That(ProjectLocalMcpConfigGuard.NormalizeConfigText(input), Is.EqualTo(input));
        }
    }
}
