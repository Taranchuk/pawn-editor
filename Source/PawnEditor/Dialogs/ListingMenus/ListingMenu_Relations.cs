﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnEditor;

[HotSwappable]
public class ListingMenu_Relations : ListingMenu<PawnRelationDef>
{
    private readonly Pawn _otherPawn;

    public ListingMenu_Relations(Pawn pawn, Pawn otherPawn, List<Filter<PawnRelationDef>> filters = null)
        : base(DefDatabase<PawnRelationDef>.AllDefs.Where(rd => rd.CanAddRelation(pawn, otherPawn)).ToList(), r => r.LabelCap, def =>
            {
                Func<List<Pawn>, bool> createInt = _ =>
                {
                    def.AddDirectRelation(pawn, otherPawn);
                    return true;
                };
                var required = 0;
                Func<Pawn, bool> predicate = null;
                var highlightGender = false;

                if (def.implied && !def.CanAddImpliedRelation(pawn, otherPawn, out required, out createInt, out predicate, out highlightGender)) return;

                bool Create(List<Pawn> list)
                {
                    if (!def.implied && pawn.relations.GetDirectRelationsCount(def) > 0)
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("PawnEditor.RelationExists".Translate(pawn.NameShortColored, def.LabelCap),
                            () => createInt(list), true));
                        return false;
                    }

                    createInt(list);
                    return true;
                }

                if (required > 0)
                {
                    PawnEditor.AllPawns.UpdateCache(null, PawnCategory.Humans);
                    var list = PawnEditor.AllPawns.GetList();
                    list.Remove(pawn);
                    list.Remove(otherPawn);
                    if (predicate != null) list.RemoveAll(p => !predicate(p));
                    Find.WindowStack.Add(new ListingMenu_Pawns(list, pawn, "Add".Translate().CapitalizeFirst(), Create, required, "Back".Translate(),
                        () => Find.WindowStack.Add(new ListingMenu_Relations(pawn, otherPawn, filters)), highlightGender));
                }
                else Create(new());
            }, "ChooseStuffForRelic".Translate() + " " + "PawnEditor.Relation".Translate(),
            r => r.description, null, filters, pawn, null, "Back".Translate(), () =>
            {
                PawnEditor.AllPawns.UpdateCache(null, PawnCategory.Humans);
                var list = PawnEditor.AllPawns.GetList();
                list.Remove(pawn);
                Find.WindowStack.Add(new ListingMenu_Pawns(list, pawn, "Next".Translate(),
                    p => Find.WindowStack.Add(new ListingMenu_Relations(pawn, p, filters))));
            }) =>
        _otherPawn = otherPawn;

    protected override string NextLabel =>
        Listing.Selected.implied && Listing.Selected.CanAddImpliedRelation(Pawn, _otherPawn, out var count, out _, out _, out _) && count > 0
            ? "Next".Translate()
            : "Add".Translate().CapitalizeFirst();

    protected override void DrawSelected(ref Rect inRect)
    {
        Widgets.DrawTextureFitted(new(inRect.x, inRect.y, 32f, 32f), PawnEditor.GetPawnTex(_otherPawn, new(25, 25), Rot4.South, cameraZoom: 2f), .8f);
        inRect.xMin += 32f;


        var labelStr = $"{"StartingPawnsSelected".Translate()}: ";
        var labelWidth = Text.CalcSize(labelStr).x;
        var selectedStr = _otherPawn.Name.ToStringShort + ", " + (Listing.Selected != null
            ? (TaggedString)Listing.LabelGetter(Listing.Selected)
            : "None".Translate().Colorize(ColoredText.SubtleGrayColor));
        Widgets.Label(inRect, labelStr.Colorize(ColoredText.SubtleGrayColor));
        inRect.xMin += labelWidth;
        Widgets.Label(inRect, selectedStr);
    }
}
