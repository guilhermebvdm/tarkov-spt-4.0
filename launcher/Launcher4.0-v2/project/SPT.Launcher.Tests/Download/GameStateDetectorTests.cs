using System;
using System.IO;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;
using Xunit;

namespace SPT.Launcher.Tests.Download
{
    public class GameStateDetectorTests : IDisposable
    {
        private readonly string _testDir;

        public GameStateDetectorTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "TRL_Test_GameState_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
        }

        [Fact]
        public void IsBaseGameInstalled_EmptyDir_ReturnsFalse()
        {
            bool installed = GameStateDetector.IsBaseGameInstalled(_testDir);
            Assert.False(installed);
        }

        [Fact]
        public void IsBaseGameInstalled_ExeOnlyWithoutState_ReturnsFalse()
        {
            File.WriteAllText(Path.Combine(_testDir, "EscapeFromTarkov.exe"), "dummy");
            bool installed = GameStateDetector.IsBaseGameInstalled(_testDir);
            Assert.False(installed);
        }

        [Fact]
        public void MarkAsDownloading_SavesCorrectProgressAndNotCompleted()
        {
            GameStateDetector.MarkAsDownloading(_testDir, "hash123", 45.5, 4500, 10000);

            var state = GameStateDetector.LoadState(_testDir);
            Assert.Equal("Downloading", state.Status);
            Assert.False(state.Completed);
            Assert.Equal(45.5, state.ProgressPercentage);
            Assert.Equal("hash123", state.TorrentHash);
        }

        [Fact]
        public void MarkAsInstalled_SetsCompletedTrue()
        {
            File.WriteAllText(Path.Combine(_testDir, "EscapeFromTarkov.exe"), "dummy");
            Directory.CreateDirectory(Path.Combine(_testDir, "EscapeFromTarkov_Data"));

            GameStateDetector.MarkAsInstalled(_testDir, "hash_complete", 57000000000, "0.16.9");

            var state = GameStateDetector.LoadState(_testDir);
            Assert.True(state.Completed);
            Assert.Equal("Installed", state.Status);
            Assert.Equal(100.0, state.ProgressPercentage);

            bool isInstalled = GameStateDetector.IsBaseGameInstalled(_testDir);
            Assert.True(isInstalled);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }
            catch { }
        }
    }
}
