## Introduction

Adaptive Cards for Blazor is a community project that provides Adaptive Cards support for your Blazor applications. 

This is based on [https://github.com/mikoskinen/Blazor.AdaptiveCards](https://github.com/mikoskinen/Blazor.AdaptiveCards)

This is a cutdown and updated version intended to just display an Adaptive card supplied as Json, but offering the opportunity to customize the displayed card's style.
The underlying AdaptiveCard renderer uses a json configuration file to style the cards. These styles are injected into the generated HTML as style attributes, making them immune to external css.
Examples of this json can be found [here](https://github.com/microsoft/AdaptiveCards/tree/master/samples/HostConfig) for various Microsoft products, such as Teams.

For documentation, please see the original this is forked from. This version was built to provide a blazor chat solution that mirrored the failities available through the bot framework.
Templating has been removed for efficiency. Sourcelink support has been added.
The one key change is that you can now inject HostConfig Json into an AdaptiveCard component

```csharp
<AdaptiveCard Schema="@text" OnSubmitAction="@RespondWithCardAction" HostConfig="@config"></AdaptiveCard>
```


