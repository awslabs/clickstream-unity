# AWS Solution Clickstream Analytics SDK for Unity

## Introduction

Clickstream Unity SDK can help you easily collect and report events from Unity Games to AWS. This SDK is part of an AWS solution - [Clickstream Analytics on AWS](https://github.com/awslabs/clickstream-analytics-on-aws), which provisions data pipeline to ingest and process event data into AWS services such as S3, Redshift.

The SDK provide easy to use API for data collection. In addition, we've added features that automatically collect common user events and attributes (e.g., app start and scene load) to simplify data collection for users.

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
    AppId = "your AppId",
    Endpoint = "https://example.com/collect"
});
```

### Start using

#### Record event

```c#
using ClickstreamAnalytics;

var attributes = new Dictionary<string, object>
{
    { "event_category", "shoes" },
    { "currency", "CNY" },
    { "value", 279.9 }
};
ClickstreamAnalytics.Record("button_click", attributes);

// record event with name
ClickstreamAnalytics.Record('button_click');
```

#### Login and logout

```c#
using ClickstreamAnalytics;

// when user login success.
ClickstreamAnalytics.SetUserId("UserId");

// when user logout
ClickstreamAnalytics.SetUserId(null);
```

#### Add user attribute

```c#
using ClickstreamAnalytics;

var userAttrs = new Dictionary<string, object> {
    { "userName", "carl" },
    { "userAge", 22 }
};
ClickstreamAnalytics.SetUserAttributes(userAttrs);
```

When opening for the first time after integrating the SDK, you need to manually set the user attributes once, and current login user's attributes will be cached in PlayerPrefs, so the next time game start you don't need to set all user's attribute again, of course you can use the same api `ClickstreamAnalytics.SetUserAttributes()` to update the current user's attribute when it changes.

#### Add global attribute

1. Add global attributes when initializing the SDK

   The following example code shows how to add global attributes when initializing the SDK.

   ```c#
   using ClickstreamAnalytics;
   
   ClickstreamAnalytics.Init(new ClickstreamConfiguration
   {
       AppId = "your AppId",
       Endpoint = "https://example.com/collect",
       GlobalAttributes = new Dictionary<string, object>
       {
           { "_app_install_channel", "Amazon Store" }
       }
   });
   ```

2. Add global attributes after initializing the SDK

   ```c#
   using ClickstreamAnalytics;
   
   ClickstreamAnalytics.SetGlobalAttributes(new Dictionary<string, object>
   {
       { "_traffic_source_source", "Search engine" }, { "level", 10 }
   });
   ```

It is recommended to set global attributes when initializing the SDK, global attributes will be included in all events that occur after it is set, you also can remove a global attribute by setting its value to `null`.

#### Other configurations

In addition to the required `AppId` and `Endpoint`, you can configure other information to get more customized usage:

```c#
using ClickstreamAnalytics;

ClickstreamAnalytics.Init(new ClickstreamConfiguration
{
    AppId = "your AppId",
    Endpoint = "https://example.com/collect",
    SendEventsInterval = 10000,
    IsCompressEvents = true,
    IsLogEvents = false,
    IsUseMemoryCache = false,
    IsTrackAppStartEvents = true,
    IsTrackAppEndEvents = true,
    IsTrackSceneLoadEvents = true,
    IsTrackSceneUnLoadEvents = true
});
```

Here is an explanation of each property:

- **AppId (Required)**: the app id of your project in control plane.
- **Endpoint (Required)**: the endpoint path you will upload the event to AWS server.
- **SendEventsInterval**: event sending interval millisecond, works only bath send mode, the default value is `10000`
- **IsLogEvents**: whether to print out event json for debugging, default is false.
- **IsUseMemoryCache**: whether to use memory to cache events for better performance, default is false.
- **IsTrackAppStartEvents**: whether auto record app start events when game becomes visible, default is `true`
- **IsTrackAppEndEvents**: whether auto record app end events when game becomes invisible, default is `true`
- **IsTrackSceneLoadEvents**: whether auto record scene load events, default is `true`
- **IsTrackSceneUnLoadEvents**: whether auto record scene unload events, default is `true`

#### Configuration update

You can update the default configuration after initializing the SDK, below are the additional configuration options you can customize.

```c#
using ClickstreamAnalytics;

ClickstreamAnalytics.UpdateConfiguration(new Configuration
{
    AppId = "your AppId",
    Endpoint = "https://example.com/collect",
    IsCompressEvents = false,
    IsLogEvents = true,
    IsTrackAppStartEvents = false,
    IsTrackAppEndEvents = false,
    IsTrackSceneLoadEvents = false,
    IsTrackSceneUnLoadEvents = false
});
```

#### Debug events

You can follow the steps below to view the event raw json and debug your events.

1. Using `ClickstreamAnalytics.Init()` API and set the `IsLogEvents` attribute to true in debug mode.
2. Integrate the SDK and start your game in Unity Editor, then open the **Console** tab.
3. Input `[ClickstreamAnalytics]` in the filter, and you will see the json content of all events recorded by Clickstream Unity SDK.


## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This project is licensed under the Apache-2.0 License.
