﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable once CheckNamespace
namespace PawnEditor;

public class Dialog_SelectBackstory : Dialog_SelectThing<BackstoryDef>
{
    protected override string PageTitle => "ChooseStuffForRelic".Translate() + " " + "Backstory".Translate().ToLower();

    public Dialog_SelectBackstory(Pawn pawn) : base(DefDatabase<BackstoryDef>.AllDefs.ToList(), pawn)
    {
        Listing = new Listing_Thing<BackstoryDef>(_quickSearchWidget.filter, ThingList, CurPawn, showIcon: false);
        
        OnSelected = backstoryDef =>
        {
            if (backstoryDef.slot == BackstorySlot.Childhood)
            {
                CurPawn.story.Childhood = backstoryDef;
            }
            else
            {
                CurPawn.story.Adulthood = backstoryDef;
            }
        };
    }
    
    protected override List<TFilter<BackstoryDef>> Filters()
    {
        List<TFilter<BackstoryDef>> filters = base.Filters();
        filters.Add(new TFilter<BackstoryDef>("PawnEditor.ShuffableOnly".Translate(), true, item => item.shuffleable, "PawnEditor.ShuffableOnlyDesc".Translate()));

        var backstorySlotDict = new Dictionary<FloatMenuOption, Func<BackstoryDef, bool>>();
        Enum.GetValues(typeof(BackstorySlot)).Cast<BackstorySlot>().ToList()
            .ForEach(bs =>
            {
                string label = bs.ToString();
                FloatMenuOption option = new FloatMenuOption(label, () => { });
                backstorySlotDict.Add(option, bd => bd.slot == bs);
            });
        filters.Add(new TFilter<BackstoryDef>("PawnEditor.BackstorySlot".Translate(), false, backstorySlotDict, "PawnEditor.BackstorySlotDesc".Translate()));

        var backstoryDefs = ThingList.OfType<BackstoryDef>();
        for (int i = 0; i < 5; i++)
        {
            var skillGainDict = new Dictionary<FloatMenuOption, Func<BackstoryDef, bool>>();
            DefDatabase<SkillDef>.AllDefs.Where(sd => backstoryDefs.Any(bd => bd.skillGains.ContainsKey(sd))).ToList()
                .ForEach(sd =>
                {
                    string label = sd.skillLabel.CapitalizeFirst();
                    FloatMenuOption option = new FloatMenuOption(label, () => { });
                    skillGainDict.Add(option, bd => bd.skillGains.ContainsKey(sd) && bd.skillGains[sd] > 0);
                });
            filters.Add(new TFilter<BackstoryDef>("PawnEditor.SkillGain".Translate(), false, skillGainDict, "PawnEditor.SkillGainDesc".Translate()));
        }

        filters.Add(new TFilter<BackstoryDef>("PawnEditor.WorkDisables".Translate(), false, item => item.workDisables == WorkTags.None, "PawnEditor.WorkDisablesDesc".Translate()));

        filters.Add(new TFilter<BackstoryDef>("PawnEditor.SkillLose".Translate(), false, item => item.skillGains.Values.All(i => i > 0), "PawnEditor.SkillLoseDesc".Translate()));

        return filters;
    }

    protected override void DrawInfoCard(ref Rect inRect)
    {
        base.DrawInfoCard(ref inRect);
        Text.Font = GameFont.Small;
        foreach (BackstorySlot slot in Enum.GetValues(typeof(BackstorySlot)))
        {
            BackstoryDef backstory = CurPawn.story.GetBackstory(slot);
            if (backstory != null)
            {
                Rect rect1 = inRect.TakeTopPart(22f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect1, slot == BackstorySlot.Adulthood ? "Adulthood".Translate() : "Childhood".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                string str = backstory.TitleCapFor(CurPawn.gender);
                Rect rect2 = new Rect(rect1);
                rect2.x += 90f;
                rect2.width = Text.CalcSize(str).x + 10f;
                Color color = GUI.color;
                GUI.color = CharacterCardUtility.StackElementBackground;
                GUI.DrawTexture(rect2, BaseContent.WhiteTex);
                GUI.color = color;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect2, str.Truncate(rect2.width));
                Text.Anchor = TextAnchor.UpperLeft;
                if (Mouse.IsOver(rect2))
                    Widgets.DrawHighlight(rect2);
                if (Mouse.IsOver(rect2))
                    TooltipHandler.TipRegion(rect2, (TipSignal)backstory.FullDescriptionFor(CurPawn).Resolve());
                inRect.yMin += 4f;
            }
        }

        inRect.yMin += 16f;
    }

    protected override void DrawOptions(ref Rect inRect)
    {
    }
}