// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    internal partial class DifficultyIconTooltip : VisibilityContainer, ITooltip<DifficultyIconTooltipContent>
    {
        private OsuSpriteText difficultyName = null!;
        private StarRatingDisplay starRating = null!;
        private OsuSpriteText bpm = null!;
        private OsuSpriteText length = null!;

        private FillFlowContainer difficultyFillFlowContainer = null!;
        private FillFlowContainer miscFillFlowContainer = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        difficultyName = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold)
                        },
                        starRating = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        difficultyFillFlowContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                        },
                        miscFillFlowContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                length = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                                bpm = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                            }
                        }
                    }
                }
            };
        }

        private DifficultyIconTooltipContent? displayedContent;

        public void SetContent(DifficultyIconTooltipContent content)
        {
            if (displayedContent != null)
                starRating.Current.UnbindFrom(displayedContent.Difficulty);

            displayedContent = content;

            starRating.Current.BindTarget = displayedContent.Difficulty;
            difficultyName.Text = displayedContent.BeatmapInfo.DifficultyName;

            if (displayedContent.TooltipType == DifficultyIconTooltipType.StarRating)
            {
                difficultyFillFlowContainer.Hide();
                miscFillFlowContainer.Hide();
                return;
            }

            difficultyFillFlowContainer.Show();
            miscFillFlowContainer.Show();

            double rate = 1;
            if (displayedContent.Mods != null)
                rate = ModUtils.CalculateRateWithMods(displayedContent.Mods);

            double bpmAdjusted = displayedContent.BeatmapInfo.BPM * rate;

            Ruleset ruleset = displayedContent.Ruleset.CreateInstance();
            var beatmapAttributes = ruleset.GetBeatmapAttributesForDisplay(displayedContent.BeatmapInfo, displayedContent.Mods ?? [])
                                           .Select(attr => new OsuSpriteText
                                           {
                                               Font = OsuFont.Style.Caption1,
                                               Text = $@"{attr.Acronym}: {attr.AdjustedValue:0.##}"
                                           });

            difficultyFillFlowContainer.Clear();
            difficultyFillFlowContainer.AddRange(beatmapAttributes);

            TimeSpan lengthTimeSpan = TimeSpan.FromMilliseconds(displayedContent.BeatmapInfo.Length / rate);
            length.Text = "Length: " + lengthTimeSpan.ToFormattedDuration();
            bpm.Text = " BPM: " + Math.Round(bpmAdjusted, 0);
        }

        public void Move(Vector2 pos) => Position = pos;

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }

    internal class DifficultyIconTooltipContent
    {
        public readonly IBeatmapInfo BeatmapInfo;
        public readonly IBindable<StarDifficulty> Difficulty;
        public readonly IRulesetInfo Ruleset;
        public readonly Mod[]? Mods;
        public readonly DifficultyIconTooltipType TooltipType;

        public DifficultyIconTooltipContent(IBeatmapInfo beatmapInfo, IBindable<StarDifficulty> difficulty, IRulesetInfo rulesetInfo, Mod[]? mods, DifficultyIconTooltipType tooltipType)
        {
            if (tooltipType == DifficultyIconTooltipType.None)
                throw new ArgumentOutOfRangeException(nameof(tooltipType), tooltipType, "Cannot instantiate a tooltip without a type");

            BeatmapInfo = beatmapInfo;
            Difficulty = difficulty;
            Ruleset = rulesetInfo;
            Mods = mods;
            TooltipType = tooltipType;
        }
    }
}
