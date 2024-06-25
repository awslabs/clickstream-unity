# AWS Solution Clickstream Analytics SDK for Unity

## Introduction

Clickstream Unity SDK can help you easily collect and report events from Unity Games to AWS. This SDK is part of an AWS solution - [Clickstream Analytics on AWS](https://github.com/awslabs/clickstream-analytics-on-aws), which provisions data pipeline to ingest and process event data into AWS services such as S3, Redshift.

The SDK provide easy to use API for data collection. In addition, we've added features that automatically collect common user events and attributes (e.g., page view, first open) to simplify data collection for users.

Visit our [Documentation site](https://awslabs.github.io/clickstream-analytics-on-aws/en/latest/sdk-manual/unity/) to learn more about Clickstream Unity SDK.

## Integrate SDK

### Include SDK

We use Unity Package Manager to distribute our SDK

1. Open `Window` and click `Package Manager` in Unity Editor.
2. Click `+` button, then select `Add package from git URL...`
3. Input `https://github.com/awslabs/clickstream-unity` , then click `Add` to wait for completion.

### Initialize the SDK

```c#
using ClickstreamAnalytics;

ClickstreamAnalytics.Init(new ClickstreamConfiguration
{
    AppId = "shop",
    Endpoint = "https://example.com/collect"
});
```

### Start using

#### Record event

```c#
using ClickstreamAnalytics;

var attributes = new Dictionary<string, object>()
{
    { "event_category", "shoes" },
    { "currency", "CNY" },
    { "value", 279.9 }
};
ClickstreamAnalytics.Record("button_click", attributes);

// record event with name
ClickstreamAnalytics.Record({ name: 'button_click' });
```

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This project is licensed under the Apache-2.0 License.
