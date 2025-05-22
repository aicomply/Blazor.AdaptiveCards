using AdaptiveCards.Rendering;
using AdaptiveCards.Rendering.Html;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveCards.Blazor.Renderers
{
    public static class CustomRenderers
    {

        public static AdaptiveRenderTransformers<HtmlTag, AdaptiveRenderContext> ActionTransformers { get; } = InitActionTransformers();

        public static HtmlTag ActionSetRender(AdaptiveActionSet actionSet, AdaptiveRenderContext context)
        {
            HtmlTag htmlTag = new DivTag().Style("box-sizing", "border-box").Style("width", "100%");
            AddActions(htmlTag, actionSet.Actions, context);
            return htmlTag;
        }

        /// <summary>
        /// Renders actions for the adaptive card.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static HtmlTag CustomAdaptiveActionRender(AdaptiveAction action, AdaptiveRenderContext context)
        {
            if (context.Config.SupportsInteractivity)
            {
                ActionsConfig actions = context.Config.Actions;
                HtmlTag htmlTag = new HtmlTag("button", allowSelfClose: false).Attr("type", "button").Style("overflow", "visible").Style("white-space", "nowrap")
                    .Style("flex-wrap", "wrap")
                    .Style("flex", "0 1 auto")
                    .Style("display", "inline-flex")
                    .Style("align-items", "center")
                    .Style("justify-content", "center")
                    .Style("margin-bottom", "8px")
                    .AddClass("ac-pushButton")
                    .Attr("onclick", "window.blazorAdaptiveCards.submitData(this, id)")
                    .Attr("id", GenerateRandomId());
                if (!string.IsNullOrWhiteSpace(action.Style) && !string.Equals(action.Style, "default", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(action.Style, "positive", StringComparison.OrdinalIgnoreCase))
                    {
                        string @default = context.Config.ContainerStyles.Default.ForegroundColors.Accent.Default;
                        string color = ColorUtil.GenerateLighterColor(@default);
                        htmlTag.Style("background-color", context.GetRGBColor(@default));
                        htmlTag.Attr("onMouseOver", "this.style.backgroundColor='" + context.GetRGBColor(color) + "'");
                        htmlTag.Attr("onMouseOut", "this.style.backgroundColor='" + context.GetRGBColor(@default) + "'");
                        htmlTag.Style("color", "#FFFFFF");
                        htmlTag.AddClass("ac-action-positive");
                    }
                    else if (string.Equals(action.Style, "destructive", StringComparison.OrdinalIgnoreCase))
                    {
                        string default2 = context.Config.ContainerStyles.Default.ForegroundColors.Attention.Default;
                        string color2 = ColorUtil.GenerateLighterColor(default2);
                        htmlTag.Style("color", context.GetRGBColor(default2));
                        htmlTag.Attr("onMouseOver", "this.style.color='" + context.GetRGBColor(color2) + "'");
                        htmlTag.Attr("onMouseOut", "this.style.color='" + context.GetRGBColor(default2) + "'");
                        htmlTag.AddClass("ac-action-destructive");
                    }
                    else
                    {
                        htmlTag.AddClass("ac-action-" + action.Style);
                    }
                }

                bool flag = !string.IsNullOrEmpty(action.Title);
                if (action.IconUrl != null)
                {
                    HtmlTag htmlTag2 = new HtmlTag("image", allowSelfClose: false).Attr("src", action.IconUrl).Style("max-height", $"{actions.IconSize}px");
                    if (actions.IconPlacement == IconPlacement.LeftOfTitle)
                    {
                        htmlTag.Style("flex-direction", "row");
                        if (flag)
                        {
                            htmlTag2.Style("margin-right", "4px");
                        }
                    }
                    else
                    {
                        htmlTag.Style("flex-direction", "column");
                        if (flag)
                        {
                            htmlTag2.Style("margin-bottom", "4px");
                        }
                    }

                    htmlTag.Append(htmlTag2);
                }

                HtmlTag child = new HtmlTag("div", allowSelfClose: false)
                {
                    Text = action.Title
                };
                htmlTag.Append(child);
                AddActionAttributes(action, htmlTag, context);
                return htmlTag;
            }

            return null;
        }

        public static HtmlTag AddActionAttributes(AdaptiveAction action, HtmlTag tag, AdaptiveRenderContext context)
        {
            tag.AddClass(GetActionCssClass(action)).Attr("role", "button").Attr("aria-label", action.Title ?? "")
                .Attr("tabindex", "0");
            AdaptiveToggleVisibilityAction adaptiveToggleVisibilityAction = null;
            if (action is AdaptiveToggleVisibilityAction adaptiveToggleVisibilityAction2)
            {
                string text = string.Empty;
                foreach (AdaptiveTargetElement targetElement in adaptiveToggleVisibilityAction2.TargetElements)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        text += ",";
                    }

                    string text2 = null;
                    string text3 = "Toggle";
                    if (targetElement != null)
                    {
                        text2 = targetElement.ElementId;
                        if (targetElement.IsVisible.HasValue)
                        {
                            text3 = targetElement.IsVisible.Value.ToString();
                        }
                    }

                    text = text + text2 + ":" + text3;
                }

                tag.Attr("data-ac-targetelements", text);
            }

            ActionTransformers.Apply(action, tag, context);
            return tag;
        }

        private static AdaptiveRenderTransformers<HtmlTag, AdaptiveRenderContext> InitActionTransformers()
        {
            AdaptiveRenderTransformers<HtmlTag, AdaptiveRenderContext> adaptiveRenderTransformers = new AdaptiveRenderTransformers<HtmlTag, AdaptiveRenderContext>();
            adaptiveRenderTransformers.Register(delegate (AdaptiveOpenUrlAction action, HtmlTag tag, AdaptiveRenderContext context)
            {
                tag.Attr("data-ac-url", action.Url);
            });
            adaptiveRenderTransformers.Register(delegate (AdaptiveSubmitAction action, HtmlTag tag, AdaptiveRenderContext context)
            {
                tag.Attr("data-ac-submitData", JsonConvert.SerializeObject(action.Data, Formatting.None));
            });
            adaptiveRenderTransformers.Register(delegate (AdaptiveShowCardAction action, HtmlTag tag, AdaptiveRenderContext context)
            {
                string value = GenerateRandomId();
                tag.Attr("data-ac-showCardId", value);
                tag.Attr("aria-controls", value);
                tag.Attr("aria-expanded", bool.FalseString);
            });
            return adaptiveRenderTransformers;
        }

        public static Func<AdaptiveAction, string> GetActionCssClass = delegate (AdaptiveAction action)
        {
            int num = action.Type.IndexOf(".") + 1;
            string text = action.Type.Substring(num, action.Type.Length - num);
            return "ac-action-" + text.Replace(text[0], char.ToLower(text[0]));
        };

        public static Func<string> GenerateRandomId => () => "ac-" + Guid.NewGuid().ToString().Substring(0, 8);

        public static void AddActions(HtmlTag uiContainer, IList<AdaptiveAction> actions, AdaptiveRenderContext context)
        {
            if (!context.Config.SupportsInteractivity || actions == null)
            {
                return;
            }

            HtmlTag htmlTag = new DivTag().AddClass("ac-actionset").Style("display", "flex").Style("flex-flow","wrap").Style("box-sizing","border-box");
            ActionsConfig actions2 = context.Config.Actions;
            List<HtmlTag> list = new List<HtmlTag>();
            if (actions2.ActionsOrientation == ActionsOrientation.Horizontal)
            {
                htmlTag.Style("flex-direction", "row");
                switch (actions2.ActionAlignment)
                {
                    case AdaptiveHorizontalAlignment.Center:
                        htmlTag.Style("justify-content", "center");
                        break;
                    case AdaptiveHorizontalAlignment.Right:
                        htmlTag.Style("justify-content", "flex-end");
                        break;
                    default:
                        htmlTag.Style("justify-content", "flex-start");
                        break;
                }
            }
            else
            {
                htmlTag.Style("flex-direction", "column");
                switch (actions2.ActionAlignment)
                {
                    case AdaptiveHorizontalAlignment.Center:
                        htmlTag.Style("align-items", "center");
                        break;
                    case AdaptiveHorizontalAlignment.Right:
                        htmlTag.Style("align-items", "flex-end");
                        break;
                    case AdaptiveHorizontalAlignment.Stretch:
                        htmlTag.Style("align-items", "stretch");
                        break;
                    default:
                        htmlTag.Style("align-items", "flex-start");
                        break;
                }
            }

            if (actions.Count > actions2.MaxActions)
            {
                context.Warnings.Add(new AdaptiveWarning(13, "Some actions were not rendered due to exceeding the maximum number of actions allowed"));
            }

            int num = Math.Min(actions2.MaxActions, actions.Count);
            IconPlacement iconPlacement = actions2.IconPlacement;
            bool flag = true;
            for (int i = 0; i < num; i++)
            {
                if (string.IsNullOrEmpty(actions[i].IconUrl))
                {
                    flag = false;
                    break;
                }
            }

            if (!flag)
            {
                actions2.IconPlacement = IconPlacement.LeftOfTitle;
            }

            for (int j = 0; j < num; j++)
            {
                HtmlTag htmlTag2 = context.Render(actions[j]);
                if (htmlTag2 != null)
                {
                    if (actions[j] is AdaptiveShowCardAction adaptiveShowCardAction)
                    {
                        string value = htmlTag2.Attributes["data-ac-showCardId"];
                        HtmlTag htmlTag3 = context.Render(adaptiveShowCardAction.Card);
                        if (htmlTag3 != null)
                        {
                            htmlTag3.Attr("id", value).AddClass("ac-showCard").Style("padding", "0")
                                .Style("display", "none")
                                .Style("margin-top", $"{actions2.ShowCard.InlineTopMargin}px");
                            list.Add(htmlTag3);
                        }
                    }

                    htmlTag.Children.Add(htmlTag2);
                }

                if (j < num - 1 && actions2.ButtonSpacing > 0)
                {
                    DivTag divTag = new DivTag();
                    if (actions2.ActionsOrientation == ActionsOrientation.Horizontal)
                    {
                        divTag.Style("flex", "0 0 auto");
                        divTag.Style("width", actions2.ButtonSpacing + "px");
                    }
                    else
                    {
                        divTag.Style("height", actions2.ButtonSpacing + "px");
                    }

                    htmlTag.Children.Add(divTag);
                }
            }

            if (htmlTag.Children.Any())
            {
                AddSeparator(uiContainer, new AdaptiveContainer(), context);
                uiContainer.Children.Add(htmlTag);
            }

            foreach (HtmlTag item in list)
            {
                uiContainer.Children.Add(item);
            }

            actions2.IconPlacement = iconPlacement;
        }

        public static HtmlTag AddSeparator(HtmlTag uiContainer, AdaptiveElement adaptiveElement, AdaptiveRenderContext context)
        {
            if (!adaptiveElement.Separator && adaptiveElement.Spacing == AdaptiveSpacing.None)
            {
                return null;
            }

            int spacing = context.Config.GetSpacing(adaptiveElement.Spacing);
            HtmlTag htmlTag = new DivTag().AddClass("ac-separator");
            if (adaptiveElement.Separator)
            {
                SeparatorConfig separator = context.Config.Separator;
                htmlTag.Style("padding-top", $"{spacing / 2}px").Style("margin-top", $"{spacing / 2}px").Style("border-top-color", context.GetRGBColor(separator.LineColor) ?? "")
                    .Style("border-top-width", $"{separator.LineThickness}px")
                    .Style("border-top-style", "solid");
            }
            else
            {
                htmlTag.Style("height", $"{spacing}px");
            }

            uiContainer.Children.Add(htmlTag);
            return htmlTag;
        }



    }
}
