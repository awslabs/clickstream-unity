using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace ClickstreamAnalytics.Tests
{
    public class ClickstreamPerformanceTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void TestPerformanceWhenUseMemoryCache()
        {
            ClickstreamAnalytics.Init(new ClickstreamConfiguration
            {
                AppId = "testApp",
                Endpoint = "https://example.com/collect",
                IsLogEvents = false,
                IsUseMemoryCache = true
            });
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < 100; i++)
            {
                ClickstreamAnalytics.Record("testEvent", ClickstreamTests.GetProperties());
            }

            stopwatch.Stop();
            var elapsedMilliseconds = (stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000;
            UnityEngine.Debug.Log("Save 100 events cost: " + elapsedMilliseconds + " ms");
            Assert.True(elapsedMilliseconds < 16);
        }


        [Test]
        public void TestPerformanceWhenUsePlayerPrefs()
        {
            ClickstreamAnalytics.Init(new ClickstreamConfiguration
            {
                AppId = "testApp",
                Endpoint = "https://example.com/collect",
                IsLogEvents = false,
                IsUseMemoryCache = false
            });
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < 10; i++)
            {
                ClickstreamAnalytics.Record("testEvent", ClickstreamTests.GetProperties());
            }

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000;
            UnityEngine.Debug.Log("Save 10 events cost: " + elapsedMilliseconds + " ms");
            Assert.True(elapsedMilliseconds < 16);
        }
    }
}