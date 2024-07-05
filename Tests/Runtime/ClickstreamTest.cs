using System.Collections.Generic;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace ClickstreamAnalytics.Tests
{
    public class ClickstreamTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ClickstreamAnalytics.Init(new ClickstreamConfiguration
            {
                AppId = "testApp",
                Endpoint = "https://example.com/collect",
                IsLogEvents = true
            });
        }

        [Test]
        public void TestUpdateConfiguration()
        {
            ClickstreamAnalytics.UpdateConfiguration(new Configuration
            {
                AppId = "testApp1",
                Endpoint = "https://clickstream.example.com/collect",
                IsCompressEvents = false,
                IsLogEvents = true,
                IsTrackAppStartEvents = false,
                IsTrackAppEndEvents = false,
                IsTrackSceneLoadEvents = false,
                IsTrackSceneUnLoadEvents = false
            });
        }

        [Test]
        public void TestRecordEventWithName()
        {
            ClickstreamAnalytics.Record("testEvent");
        }

        [Test]
        public void TestRecordEventWithAllAttributesType()
        {
            var properties = new Dictionary<string, object>
            {
                { "product_name", "shoes" },
                { "quantity", 42 },
                { "order_number", 123456U },
                { "rating", (byte)5 },
                { "temperature", (sbyte)-10 },
                { "discount", (short)-200 },
                { "item_count", (ushort)1000 },
                { "big_number", 9876543210L },
                { "huge_number", 12345678901234567890UL },
                { "price", 19.99f },
                { "pi", 3.141592653589793 },
                { "tax_rate", 0.075m },
                { "in_stock", true }
            };
            ClickstreamAnalytics.Record("testEvent", properties);
        }


        [Test]
        public void TestSetUserId()
        {
            ClickstreamAnalytics.SetUserId("123");
            ClickstreamAnalytics.SetUserId(null);
        }

        [Test]
        public void TestSetUserAttributes()
        {
            var userAttrs = new Dictionary<string, object>
            {
                { "userName", "carl" },
                { "userAge", 22 }
            };
            ClickstreamAnalytics.SetUserAttributes(userAttrs);
        }

        [Test]
        public void TestSetGlobalAttributes()
        {
            var globalAttrs = new Dictionary<string, object>
            {
                { "level", 5.1 }, { "class", 6 }, { "isOpenNotification", true }, { "channel", "amazon store" }
            };
            ClickstreamAnalytics.SetGlobalAttributes(globalAttrs);
            ClickstreamAnalytics.SetGlobalAttributes(new Dictionary<string, object>
            {
                { "class", null }
            });
        }

        [Test]
        public void TestFlushEvents()
        {
            ClickstreamAnalytics.FlushEvents();
        }
    }
}