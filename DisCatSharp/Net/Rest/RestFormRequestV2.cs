// RestFormRequestV2.cs
using System;
using System.Collections.Generic;

namespace DisCatSharp.Net.V2
{
    internal sealed class RestFormRequestV2 : BaseRestRequestV2
    {
        public RestFormRequestV2(
            DiscordOAuth2Client client,
            RateLimitBucketV2 bucket,
            Uri url,
            RestRequestMethod method,
            string route,
            Dictionary<string, string> formData,
            IReadOnlyDictionary<string, string>? headers = null,
            double? ratelimitWaitOverride = null)
            : base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
        {
            this.FormData = formData;
        }

        public Dictionary<string, string> FormData { get; }
    }
}
