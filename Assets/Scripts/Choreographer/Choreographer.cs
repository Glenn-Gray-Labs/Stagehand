using System;
using System.Collections.Generic;
using Bolt;
using ImGuiNET;
using UnityEngine;

namespace Stagehand {
    public class Choreographer : MonoBehaviour {
        // Unique ID Generator
        private static int _nextId;

        // Read/Write Feature
        public const bool ReadOnly = true;

        // Auto-Layout Feature
        public const float Padding = 40f;

        private class NodeSize {
            public float CumulativeSize;
            public float OriginalSize;
        }
        private readonly List<NodeSize> _rowSizes = new List<NodeSize>(new[] { new NodeSize() });
        private readonly List<NodeSize> _columnSizes = new List<NodeSize>(new[] { new NodeSize() });

        // Node Inputs/Outputs
        [Serializable] public class NodeIO {
            public readonly string Id;
            public readonly Type Type;

            public NodeIO(Type type) {
                Id = (++_nextId).ToString();
                Type = type;
            }
        }

        // Node Feature
        [Serializable] public class Node {
            // Low Level Data
            public readonly string Id;
            public readonly Type Type;
            public readonly NodeIO[] Inputs;
            public readonly NodeIO[] Outputs;

            // GUI Data
            public string Title;
            public readonly string Name;
            public IStyle Style;
            public Vector2 LeftSize;
            public Vector2 RightSize;
            public readonly float[] RowHeights;

            // Auto-Layout Feature
            public readonly int Row;
            public readonly int Column;
            public Vector2 Size;
            public Vector2 Pos;

            public Node(Type type, NodeIO[] inputs, NodeIO[] outputs, int row, int column, Type parentType = null) {
                Id = (++_nextId).ToString();
                Type = type;
                Inputs = inputs;
                Outputs = outputs;

                Title = parentType == null ? Type.Name : $"{parentType.Name}->{Type.Name}";
                Name = $"{Title}##{Id}";

                var rowCount = Inputs.Length > Outputs.Length ? Inputs.Length : Outputs.Length;
                RowHeights = new float[rowCount];

                Row = row;
                Column = column;
            }
        }

        // TODO: How should we pass nodes into Choreographer?
        public static Node[] Nodes = {};

        public interface IStyle {
            void Push();
            void Pop();
        }

        public class DefaultStyle : IStyle {
            public virtual void Push() {
                //
            }

            public virtual void Pop() {
                //
            }
        }
        private static readonly IStyle _defaultStyle = new DefaultStyle();

        public class CustomStyle : DefaultStyle {
            public override void Push() {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 20f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign, new Vector2(0f, 0.3333333f));
                ImGui.PushStyleColor(ImGuiCol.Text, ColorWhite);
                ImGui.PushStyleColor(ImGuiCol.Border, ColorWhite);
            }

            public override void Pop() {
                ImGui.PopStyleColor(2);
                ImGui.PopStyleVar(2);
            }
        }
        private static readonly IStyle _customStyle = new CustomStyle();

        private readonly Dictionary<Type, IStyle> _styles = new Dictionary<Type, IStyle> {
            { typeof(bool), _customStyle },
        };

        public static readonly uint ColorWhite = ImGui.ColorConvertFloat4ToU32(Vector4.one);
        public static readonly uint ColorBackground = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f));
        public static readonly uint ColorGridLineHorizontal = ImGui.ColorConvertFloat4ToU32(new Vector4(0.3333333f, 0.3333333f, 0.3333333f, 1f));
        public static readonly uint ColorGridLineVertical = ColorGridLineHorizontal;

        private void OnEnable() {
            void _beforeLayout() {
                ImGuiUn.Layout -= _beforeLayout;
                _refresh();
            }
            ImGuiUn.Layout += _beforeLayout;
            ImGuiUn.Layout += OnLayout;
        }

        private void OnDisable() {
            ImGuiUn.Layout -= OnLayout;
        }

        private void _refresh() {
            var imGuiStyle = ImGui.GetStyle();
            foreach (var node in Nodes) {
                node.Style = _styles.ContainsKey(node.Type) ? _styles[node.Type] : _defaultStyle;

                for (var i = 0; i < node.Inputs.Length; ++i) {
                    var inputSize = ImGui.CalcTextSize($"{node.Inputs[i].Type.Name}");
                    node.LeftSize.x = Mathf.Max(node.LeftSize.x, inputSize.x);
                    node.LeftSize.y += inputSize.y;
                    node.RowHeights[i] = inputSize.y;
                }
                node.LeftSize.x += imGuiStyle.WindowPadding.x + imGuiStyle.ItemSpacing.x * 2f + imGuiStyle.ItemInnerSpacing.x;

                for (var i = 0; i < node.Outputs.Length; ++i) {
                    var outputSize = ImGui.CalcTextSize($"{node.Outputs[i].Type.Name}");
                    node.RightSize.x = Mathf.Max(node.RightSize.x, outputSize.x);
                    node.RightSize.y += outputSize.y;
                    if (outputSize.y > node.RowHeights[i]) node.RowHeights[i] = outputSize.y;
                }
                node.RightSize.x += imGuiStyle.WindowPadding.x + imGuiStyle.ItemSpacing.x + imGuiStyle.ItemInnerSpacing.x;

                var titleSize = ImGui.CalcTextSize(node.Title);
                if (node.LeftSize.x + node.RightSize.x > titleSize.x) titleSize.x = node.LeftSize.x + node.RightSize.x;

                node.Size = new Vector2(
                titleSize.x + imGuiStyle.WindowPadding.x + imGuiStyle.ItemSpacing.x / 2f, 
                titleSize.y + imGuiStyle.WindowPadding.y + imGuiStyle.FramePadding.y * 2f + (imGuiStyle.ItemInnerSpacing.y + imGuiStyle.ItemSpacing.y) * node.RowHeights.Length + Mathf.Max(node.LeftSize.y, node.RightSize.y)
                );

                void _adjustCumulativeSize(int current, IList<NodeSize> sizes, float size) {
                    if (current >= sizes.Count) {
                        var prevNodeSize = sizes[sizes.Count - 1];
                        for (var i = sizes.Count; i < current + 1; ++i) {
                            sizes.Add(new NodeSize {
                                CumulativeSize = prevNodeSize.CumulativeSize,
                                OriginalSize = 0,
                            });
                        }
                    }

                    if (sizes[current].OriginalSize >= size) return;
                    var delta = size - sizes[current].OriginalSize;
                    for (var i = current; i < sizes.Count; ++i) {
                        sizes[i].CumulativeSize += delta;
                        sizes[i].OriginalSize = size;
                    }
                }
                _adjustCumulativeSize(node.Column, _columnSizes, node.Size.x);
                _adjustCumulativeSize(node.Row, _rowSizes, node.Size.y);
            }

            foreach (var node in Nodes) {
                node.Pos = new Vector2(
                    node.Column == 0 ? (_columnSizes[node.Column].CumulativeSize - node.Size.x) / 2f : Padding * node.Column + _columnSizes[node.Column - 1].CumulativeSize + (_columnSizes[node.Column].CumulativeSize - _columnSizes[node.Column - 1].CumulativeSize - node.Size.x) / 2f, 
                    node.Row == 0 ? (_rowSizes[node.Row].CumulativeSize - node.Size.y) / 2f : Padding * node.Row + _rowSizes[node.Row - 1].CumulativeSize + (_rowSizes[node.Row].CumulativeSize - _rowSizes[node.Row - 1].CumulativeSize - node.Size.y) / 2f
                );
            }
        }

        private void OnLayout() {
            var bgDrawList = ImGui.GetBackgroundDrawList();
            bgDrawList.AddRectFilled(Vector2.zero, new Vector2(2000f, 2000f), ColorBackground);
            for (var i = 1; i < 19; ++i) {
                bgDrawList.AddLine(new Vector2(0f, i * 100f), new Vector2(2000f,i*100f), ColorGridLineHorizontal, 1f);
                bgDrawList.AddLine(new Vector2(i * 100f, 0f), new Vector2(i*100f, 2000f), ColorGridLineVertical, 1f);
            }

            var imGuiStyle = ImGui.GetStyle();
            foreach (var node in Nodes) {
                ImGui.SetNextWindowSize(node.Size);
                ImGui.SetNextWindowPos(node.Pos);
                node.Style.Push();
                if (ImGui.Begin(node.Name, ReadOnly ? ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings : ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse)) {
                    var drawList = ImGui.IsWindowFocused() | ImGui.IsWindowHovered() ? ImGui.GetForegroundDrawList() : ImGui.GetBackgroundDrawList();
                    CustomEvent.Trigger(gameObject, "OnNode", drawList, node);
                    ImGui.Columns(2, "Column", false);
                    ImGui.SetColumnWidth(0, node.LeftSize.x);
                    ImGui.SetColumnOffset(1, node.LeftSize.x);
                    ImGui.SetColumnWidth(1, node.RightSize.x);
                    for (var i = 0; i < node.RowHeights.Length; ++i) {
                        if (i < node.Inputs.Length) {
                            ImGui.PushID(node.Inputs[i].Id);
                            if (ImGui.Selectable(node.Inputs[i].Type.Name, false, ReadOnly ? ImGuiSelectableFlags.Disabled : ImGuiSelectableFlags.None, new Vector2(node.LeftSize.x, node.RowHeights[i]))) {
                                CustomEvent.Trigger(gameObject, "OnInput", drawList, node.Inputs[i].Id);
                            }
                            CustomEvent.Trigger(gameObject, "OnConnection", drawList, new Vector2(ImGui.GetItemRectMin().x, (ImGui.GetItemRectMin().y + ImGui.GetItemRectMax().y) / 2f), -1, node.Inputs[i].Id, ReadOnly);
                            ImGui.PopID();
                        }
                        ImGui.NextColumn();
                        if (i < node.Outputs.Length) {
                            ImGui.PushID(node.Inputs[i].Id);
                            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(1.0f, 0f));
                            ImGui.Unindent(imGuiStyle.ItemSpacing.x);
                            if (ImGui.Selectable(node.Outputs[i].Type.Name, false, ReadOnly ? ImGuiSelectableFlags.Disabled : ImGuiSelectableFlags.None, new Vector2(node.RightSize.x, node.RowHeights[i]))) {
                                CustomEvent.Trigger(gameObject, "OnOutput", drawList, node.Outputs[i].Id);
                            }
                            CustomEvent.Trigger(gameObject, "OnConnection", drawList, new Vector2(ImGui.GetItemRectMax().x, (ImGui.GetItemRectMin().y + ImGui.GetItemRectMax().y) / 2f), 1, node.Outputs[i].Id, ReadOnly);
                            ImGui.Indent(imGuiStyle.ItemSpacing.x);
                            ImGui.PopStyleVar();
                            ImGui.PopID();
                        }
                        ImGui.NextColumn();
                    }
                    ImGui.Columns();
                }
                ImGui.End();
                node.Style.Pop();
            }
        }
    }
}