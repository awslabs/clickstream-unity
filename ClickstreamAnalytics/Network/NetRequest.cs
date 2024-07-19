using System;
using System.Collections;
using System.Text;
using ClickstreamAnalytics.Provider;
using ClickstreamAnalytics.Util;
using UnityEngine.Networking;

namespace ClickstreamAnalytics.Network
{
    internal static class NetRequest
    {
        private const int RequestTimeoutSeconds = 15;

        public static IEnumerator SendRequest(
            string eventJson,
            ClickstreamContext context,
            int bundleSequenceId,
            SendType sendType,
            Action<bool, string, string, SendType> callback)
        {
            var content = eventJson;
            ClickstreamLog.Debug("start send request:\n" + content);
            var compression = "";
            if (context.Configuration.IsCompressEvents)
            {
                content = ClickstreamCompress.Gzip(eventJson);
                compression = "gzip";
            }

            var bodyRaw = Encoding.UTF8.GetBytes(content);
            var now = DateTimeOffset.UtcNow;
            var timestamp = now.ToUnixTimeMilliseconds();
            var endpoint = context.Configuration.Endpoint;
            var appId = context.Configuration.AppId;

            var url =
                $"{endpoint}?platform=Unity&appId={appId}&event_bundle_sequence_id={bundleSequenceId}&compression={compression}&upload_timestamp={timestamp}";

            using var webRequest = new UnityWebRequest(url, "POST");
            webRequest.timeout = RequestTimeoutSeconds;
            webRequest.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();
            var responseCode = webRequest.responseCode;
            callback(responseCode == 200, eventJson, webRequest.error, sendType);
        }
    }
}