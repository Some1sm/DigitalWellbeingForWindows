using DigitalWellbeing.Core.Data;
using DigitalWellbeing.Core.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalWellbeing.Tests
{
    public class AppUsageRepositoryTests
    {
        private string _testFolder;
        private AppUsageRepository _repo;

        [SetUp]
        public void Setup()
        {
            _testFolder = Path.Combine(Path.GetTempPath(), "DW_Tests_" + Guid.NewGuid());
            Directory.CreateDirectory(_testFolder);
            _repo = new AppUsageRepository(_testFolder);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_testFolder))
            {
                Directory.Delete(_testFolder, true);
            }
        }

        [Test]
        public void UpdateUsage_CreatesFileAndWritesData()
        {
            var usage = new List<AppUsage>
            {
                new AppUsage("notepad", "Notepad", TimeSpan.FromMinutes(5))
            };

            _repo.UpdateUsage(DateTime.Now, usage);

            var loaded = _repo.GetUsageForDate(DateTime.Now);
            Assert.That(loaded.Count, Is.EqualTo(1));
            Assert.That(loaded[0].ProcessName, Is.EqualTo("notepad"));
            Assert.That(loaded[0].Duration.TotalMinutes, Is.EqualTo(5));
        }

        [Test]
        public void UpdateUsage_OverwritesData()
        {
            var date = DateTime.Now;
            var usage1 = new List<AppUsage> { new AppUsage("abc", "ABC", TimeSpan.FromMinutes(10)) };
            var usage2 = new List<AppUsage> { new AppUsage("abc", "ABC", TimeSpan.FromMinutes(5)) };

            _repo.UpdateUsage(date, usage1);
            _repo.UpdateUsage(date, usage2);

            var loaded = _repo.GetUsageForDate(date);
            Assert.That(loaded.Count, Is.EqualTo(1));
            // Expect 5 because it overwrites
            Assert.That(loaded[0].Duration.TotalMinutes, Is.EqualTo(5));
        }

        [Test]
        public void ConcurrentAccess_HandledGracefully()
        {
            var date = DateTime.Now;
            // Write a larger dataset to increase write time
            var usage = new List<AppUsage>();
            for(int k=0; k<100; k++) usage.Add(new AppUsage($"p{k}", $"P{k}", TimeSpan.FromSeconds(k)));

            // Simulate Service writing (Overwrite)
            var task1 = Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    _repo.UpdateUsage(date, usage);
                    Thread.Sleep(5);
                }
            });

            // Simulate UI reading
            var task2 = Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var data = _repo.GetUsageForDate(date);
                    // Just verify we got something (or nothing) but didn't crash
                    // And if we got data, it should be valid
                    if(data.Count > 0) 
                    {
                        Assert.That(data.Count, Is.EqualTo(100));
                    }
                    Thread.Sleep(5);
                }
            });

            Assert.DoesNotThrow(() => Task.WaitAll(task1, task2));
        }
    }
}
