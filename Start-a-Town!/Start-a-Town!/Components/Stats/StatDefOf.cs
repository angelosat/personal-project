﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static public class StatDefOf
    {
        static public readonly StatNewDef MaxHaulWeight = new StatNewDef("MaxHaulWeight", a => (a.GetAttribute(AttributeDef.Strength)?.Level ?? 0))// / 2)
        {
            Description = "Exceeding the limit will drain stamina",
            Label = "Haul weight limit",
            //Builder = new ValueBuilderFromAttribute(AttributeDef.Strength).DivideBy(2)
        };
        static public readonly StatNewDef Encumberance = new StatNewDef(
            "Encumberance",
            a =>
            {
                var haulWeight = a.GetHauled()?.TotalWeight ?? 0;
                if (haulWeight == 0)
                    return 0;
                var maxWeight = StatDefOf.MaxHaulWeight.GetValue(a);
                var ratio = haulWeight / maxWeight;// (maxWeight - haulWeight) / haulWeight;
                ratio = MathHelper.Clamp(ratio, 0, 1);
                return ratio;// 1 - ratio;
            })
        {
            Description = "Being encumbered affects walking speed.",
            Label = "Encumberance",
        };

        static public readonly StatNewDef WalkSpeed = new StatNewDef("WalkSpeed")
        {
            Description = "Speed of walking",
            Label = "Walk speed",
            Type = StatNewDef.Types.Percentile
        };

        static public readonly StatNewDef StaminaThresholdForWork = new StatNewDef("StaminaThresholdForWork",
            a =>
            {
                var actor = a as Actor;
                //var stamina = actor.GetResource(ResourceDef.Stamina);
                //var staminaPercentage = stamina.Percentage;
                var staminaBaseThreshold = .25f; //placeholder?
                var stamina = actor.GetResource(ResourceDef.Stamina);
                staminaBaseThreshold = stamina.GetThresholdValue(0);
                var activity1 = actor.GetTrait(TraitDefOf.Activity).Normalized;
                var num = activity1 * staminaBaseThreshold * .5f;
                var threshold = staminaBaseThreshold - num;
                return threshold;
                //var tired = staminaPercentage < threshold;
                //return tired;
            })
        {
            Description = "Won't start new tasks if stamina below this percentage.",
            Label = "Work Stamina Threshold",
            //Type = Types.Percentile
        };

        static public readonly StatNewDef MoodChangeRate = new StatNewDef("MoodChangeRate",
            a =>
            {
                var actor = a as Actor;
                var resilience = actor.GetTrait(TraitDefOf.Resilience).Normalized;
                var value = 1 + resilience * .5f;
                return value;
            })
        {
            Description = "The speed at which mood changes to reach target value.",
            Label = "Mood change rate",
            //Type = Types.Percentile
        };

        static public readonly StatNewDef Armor = new StatNewDef("Armor",
              a =>
              {
                  var actor = a as Actor;
                  var gear = actor.GetGear();
                  var value = gear.Sum(g => g.Def.ApparelProperties.ArmorValue);
                  return value;
              })
        {
            Description = "Protection against damage.",
            Label = "Damage mitigation",
            Type = StatNewDef.Types.Scalar
        };


        static public readonly StatNewDef ToolEfficiency = new("ToolEfficiecy",
              a =>
              {
                  var tool = a as Entity;
                  var material = tool.GetMaterial(BoneDef.EquipmentHead);
                  if (material is null)
                      return 1;
                  var matStrength = material.Density;
                  var efficiency = tool.Def.ToolProperties.Ability.Efficiency;
                  var total = (float)efficiency * matStrength;
                  total *= a.Quality.Multiplier;
                  return total;
              })
        {
            Description = "Amount of work produced with each hit of the tool.",
            Label = "Tool efficiency",
            Type = StatNewDef.Types.Scalar
        };
        static public readonly StatNewDef ToolSpeed = new("ToolSpeed",
              a =>
              {
                  var tool = a as Entity;
                  var material = tool?.GetMaterial(BoneDef.EquipmentHandle);
                  if (material is null)
                      return 1;
                  var aa = 20f;
                  var density = Math.Max(aa, material.Density); // in case for some reason the material is air
                  //var total = density / 100f; // density should add ticks between each tool hit (NOT POSSIBLE THE WAY I HAVE ANIMATIONS SET UP)
                  var total = aa / density;
                  total *= a.Quality.Multiplier;
                  return total;
              })
        {
            Description = "Determines time between each tool strike.",
            Label = "Tool speed",
            Type = StatNewDef.Types.Scalar,
            StringFormat = "+##0%"
        };
        static public readonly StatNewDef WorkSpeed = new("WorkSpeed",
              a =>
              {
                  var actor = a as Actor;
                  //var toolspeed = StatDefOf.ToolSpeed.GetValue(actor.GetEquipmentSlot(Components.GearType.Mainhand));
                  var toolspeed = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolSpeed) ?? 0;
                  var speed = 1 + toolspeed;
                  return speed;
              })
        {
            Description = "Work speed",
            Label = "Work speed",
            Type = StatNewDef.Types.Scalar,
            StringFormat = "+##0%"
        };
        static public readonly StatNewDef[] ToolStatPackage = { ToolEfficiency, ToolSpeed };
        static public readonly StatNewDef[] NpcStatPackage = { MaxHaulWeight, Encumberance, WalkSpeed, Armor };
    }
}
