﻿using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Start_a_Town_
{
    public static class StatDefOf
    {
        public static readonly StatNewDef MaxHaulWeight = new("Max Haul Weight", a => a.GetAttribute(AttributeDef.Strength)?.Level ?? 0)
        {
            Description = "Exceeding the limit will drain stamina",
        };
        public static readonly StatNewDef Encumberance = new(
            "Encumberance",
            a =>
            {
                var haulWeight = a.Hauled?.TotalWeight ?? 0;
                if (haulWeight == 0)
                    return 0;
                var maxWeight = StatDefOf.MaxHaulWeight.GetValue(a);
                var ratio = haulWeight / maxWeight;
                ratio = MathHelper.Clamp(ratio, 0, 1);
                return ratio;
            })
        {
            Description = "Being encumbered affects walking speed.",
        };

        public static readonly StatNewDef WalkSpeed = new("Walk Speed")
        {
            Description = "Speed of walking",
            Type = StatNewDef.Types.Percentile
        };

        public static readonly StatNewDef StaminaThresholdForWork = new("Stamina Threshold For Work",
            a =>
            {
                var actor = a as Actor;
                var staminaBaseThreshold = .25f; //placeholder?
                var stamina = actor.GetResource(ResourceDefOf.Stamina);
                staminaBaseThreshold = stamina.GetThresholdValue(0);
                var activity1 = actor.GetTrait(TraitDefOf.Activity).Normalized;
                var num = activity1 * staminaBaseThreshold * .5f;
                var threshold = staminaBaseThreshold - num;
                return threshold;
            })
        {
            Description = "Won't start new tasks if stamina below this percentage.",
        };

        public static readonly StatNewDef MoodChangeRate = new("Mood Change Rate",
            a =>
            {
                var actor = a as Actor;
                var resilience = actor.GetTrait(TraitDefOf.Resilience).Normalized;
                var value = 1 + resilience * .5f;
                return value;
            })
        {
            Description = "The speed at which mood changes to reach target value.",
        };

        public static readonly StatNewDef Armor = new("Armor",
              a =>
              {
                  var actor = a as Actor;
                  var gear = actor.GetGear();
                  var value = gear.Sum(g => g.Def.ApparelProperties.ArmorValue);
                  return value;
              })
        {
            Description = "Protection against damage.",
            Type = StatNewDef.Types.Scalar
        };


        public static readonly StatNewDef ToolEffectiveness = new("Tool Efficiecy",
              a =>
              {
                  var tool = a as Entity;
                  var material = tool.GetMaterial(BoneDefOf.ToolHead);
                  if (material is null)
                      return 1; // is it ever possible for this to be null?
                  var matStrength = material.Density;
                  var efficiency = //tool.Def.ToolProperties?.Ability.Effectiveness ?? 
                                    tool.GetComponent<ToolAbilityComponent>().Props.Ability.Effectiveness;
                  var total = (float)efficiency * matStrength;
                  total *= a.Quality.Multiplier;
                  return total;
              })
        {
            Description = "Amount of work produced with each hit of the tool.",
            Type = StatNewDef.Types.Scalar
        };
        public static readonly StatNewDef ToolSpeed = new("Tool Speed",
              a =>
              {
                  var tool = a as Entity;
                  var material = tool?.GetMaterial(BoneDefOf.ToolHandle);
                  if (material is null)
                      return 1;
                  var aa = 20f; // what is this?
                  var density = Math.Max(aa, material.Density); // in case for some reason the material is air
                  //var total = density / 100f; // density should add ticks between each tool hit (NOT POSSIBLE THE WAY I HAVE ANIMATIONS SET UP)
                  var total = aa / density;
                  total *= a.Quality.Multiplier;
                  return total;
              })
        {
            Description = "Determines time between each tool strike.",
            Type = StatNewDef.Types.Scalar,
            StringFormat = "+##0%"
        };
        public static readonly StatNewDef WorkSpeed = new("Work Speed",
              a =>
              {
                  var actor = a as Actor;
                  var toolspeed = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolSpeed) ?? 0;
                  var speed = 1 + toolspeed;
                  return speed;
              })
        {
            Description = "Work speed",
            Type = StatNewDef.Types.Scalar,
            StringFormat = "+##0%"
        };
        public static readonly StatNewDef WorkEffectiveness = new("Work Effectiveness",
              a =>
              {
                  var actor = a as Actor;
                  var val = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolEffectiveness) ?? actor.GetMaterial(BoneDefOf.RightHand).Density;
                  return val;
                  //return 1 + val;
              })
        {
            Description = "Work effectiveness",
            Type = StatNewDef.Types.Scalar,
            StringFormat = "+##0"
        };
        public static readonly StatNewDef[] ToolStatPackage = { ToolEffectiveness, ToolSpeed };
        public static readonly StatNewDef[] NpcStatPackage = { MaxHaulWeight, Encumberance, WalkSpeed, Armor };
    }
}
