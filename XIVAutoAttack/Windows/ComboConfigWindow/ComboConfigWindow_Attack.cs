﻿using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using XIVAutoAttack.Data;
using XIVAutoAttack.SigReplacers;

namespace XIVAutoAttack.Windows.ComboConfigWindow;

internal partial class ComboConfigWindow
{
    private void DrawAttack()
    {
        ImGui.Text("你可以选择开启想要的职业的连续GCD战技、技能，若职业与当前职业相同则有命令宏提示。");

        string folderLocation = Service.Configuration.ScriptComboFolder;
        if (ImGui.InputText("自定义循环路径", ref folderLocation, 256))
        {
            Service.Configuration.ScriptComboFolder = folderLocation;
            Service.Configuration.Save();
        }

        ImGui.SameLine();
        Spacing();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.FolderOpen))
        {
            IconReplacer.LoadFromFolder();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("点击以载入自定义循环");
        }

        if (!Directory.Exists(Service.Configuration.ScriptComboFolder))
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "请设定一个路径以正常使用自定义循环！");
        }

        ImGui.BeginChild("攻击", new Vector2(0f, -1f), true);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 5f));
        int num = 1;


        foreach (var key in IconReplacer.CustomCombosDict.Keys)
        {
            var combos = IconReplacer.CustomCombosDict[key];
            if (combos == null || combos.Length == 0) continue;

            if (ImGui.CollapsingHeader(key.ToName()))
            {
                if (ImGui.IsItemHovered() && _roleDescriptionValue.TryGetValue(key, out string roleDesc))
                {
                    ImGui.SetTooltip(roleDesc);
                }
                for (int i = 0; i < combos.Length; i++)
                {
                    if (i > 0) ImGui.Separator();
                    var combo = IconReplacer.GetChooseCombo(combos[i]);
                    var canAddButton = Service.ClientState.LocalPlayer != null && combo.JobIDs.Contains((ClassJobID)Service.ClientState.LocalPlayer.ClassJob.Id);

                    DrawTexture(combo, () =>
                    {
                        var actions = combo.Config;
                        foreach (var boolean in actions.bools)
                        {
                            Spacing();
                            bool val = boolean.value;
                            if (ImGui.Checkbox($"#{num}: {boolean.description}", ref val))
                            {
                                boolean.value = val;
                                Service.Configuration.Save();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("关键名称为：" + boolean.name);
                            }

                            //显示可以设置的案件
                            if (canAddButton)
                            {
                                ImGui.SameLine();
                                Spacing();
                                CommandHelp(boolean.name);
                            }

                        }
                        foreach (var doubles in actions.doubles)
                        {
                            Spacing();
                            float val = doubles.value;
                            if (ImGui.DragFloat($"{doubles.description}##{num}_{doubles.description}", ref val, doubles.speed, doubles.min, doubles.max))
                            {
                                doubles.value = val;
                                Service.Configuration.Save();
                            }
                        }
                        foreach (var textItem in actions.texts)
                        {
                            Spacing();
                            string val = textItem.value;
                            if (ImGui.InputText($"{textItem.description}##{num}_{textItem.description}", ref val, 15))
                            {
                                textItem.value = val;
                                Service.Configuration.Save();
                            }
                        }
                        foreach (var comboItem in actions.combos)
                        {
                            Spacing();
                            if (ImGui.BeginCombo($"{comboItem.description}##{num}_{comboItem.description}", comboItem.items[comboItem.value]))
                            {
                                for (int comboIndex = 0; comboIndex < comboItem.items.Length; comboIndex++)
                                {
                                    if (ImGui.Selectable(comboItem.items[comboIndex]))
                                    {
                                        comboItem.value = comboIndex;
                                        Service.Configuration.Save();
                                    }
                                    if (canAddButton)
                                    {
                                        ImGui.SameLine();
                                        Spacing();
                                        CommandHelp(comboItem.name + comboIndex.ToString());
                                    }
                                }
                                ImGui.EndCombo();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("关键名称为：" + comboItem.name);
                            }

                            //显示可以设置的案件
                            if (canAddButton)
                            {
                                ImGui.SameLine();
                                Spacing();
                                CommandHelp(comboItem.name);
                            }
                        }

                        if (canAddButton)
                        {
                            ImGui.NewLine();

                            foreach (var customCMD in combo.CommandShow)
                            {
                                Spacing();
                                CommandHelp(customCMD.Key, customCMD.Value);
                            }
                        }

                    }, combo.JobIDs[0], combos[i].combos.Select(c => c.Author).ToArray());

                    num++;
                }
            }
            else
            {
                if (ImGui.IsItemHovered() && _roleDescriptionValue.TryGetValue(key, out string roleDesc))
                {
                    ImGui.SetTooltip(roleDesc);
                }
                num += combos.Length;
            }
        }

        ImGui.PopStyleVar();
        ImGui.EndChild();
    }
}
