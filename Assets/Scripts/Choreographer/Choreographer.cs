using System;
using System.Collections.Generic;
using System.Text;
using Bolt;
using ImGuiNET;
using UnityEngine;

namespace Stagehand {
    public class Choreographer : MonoBehaviour {
        // Unique ID Generator
        private static int _nextId;

        // Read/Write Feature
        public bool ReadOnly = true;

        // Auto-Layout Feature
        public const float Padding = 80f;

        private class NodeSize {
            public float CumulativeSize;
            public float OriginalSize;
        }
        private readonly List<NodeSize> _rowSizes = new List<NodeSize>(new[] { new NodeSize() });
        private readonly List<NodeSize> _columnSizes = new List<NodeSize>(new[] { new NodeSize() });

        // Helpers
        public static string _friendlyTypeName(Type type) {
            var argName = type.ToString();
            // Sandbox+<<-cctor>g___deserializeInto|3_11>d`1[Sandbox+Config] => _deserializeInto
            int start, end = argName.LastIndexOf('|');
            if (end > -1) {
                start = argName.LastIndexOf('>', end);
                argName = argName.Substring(start + 4, end - start - 4);
            }
            // Sandbox+Config => Config
            start = argName.LastIndexOf('+');
            if (start > -1) argName = argName.Substring(start + 1);
            // System.Collections.Generic.Queue`1[System.Int64] => Queue
            end = argName.IndexOf('`');
            if (end == -1) end = argName.Length;
            // System.String => String
            start = argName.LastIndexOf('.', end - 1) + 1;
            return argName.Substring(start, end - start);
        }

        public static string _friendlyTypeName(Type type, string name) {
            return $"{name} ({_friendlyTypeName(type)})";
        }

        // Node Inputs/Outputs
        [Serializable] public class NodeIO {
            public readonly string Id;
            public readonly Type Type;
            public readonly string Name;
            public readonly List<NodeIO> Connections = new List<NodeIO>();

            public NodeIO(Type type, string name) {
                Id = (++_nextId).ToString();
                Type = type;
                Name = _friendlyTypeName(type, name);
            }
        }

        // Node Connections
        [Serializable] public class Connection {
            public ConnectionType Type;
            public Node Parent;

            public enum ConnectionType {
                Inherited,
                Recursive,
            }

            public Connection(ConnectionType type, Node parent) {
                Type = type;
                Parent = parent;
            }
        }

        // Node Feature
        [Serializable] public class Node {
            // Low Level Data
            public readonly string Id;
            public readonly Type Type;
            public readonly NodeIO[] Inputs;
            public readonly NodeIO[] Outputs;
            public readonly List<Connection> Connections = new List<Connection>();

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
            public Vector2 Pos;
            public Vector2 Size;

            public Node(Type type, NodeIO[] inputs, NodeIO[] outputs, int row, int column) {
                Id = (++_nextId).ToString();
                Type = type;
                Inputs = inputs;
                Outputs = outputs;

                Title = _friendlyTypeName(Type);
                Name = $"{Title}##{Id}";

                var rowCount = Inputs.Length > Outputs.Length ? Inputs.Length : Outputs.Length;
                RowHeights = new float[rowCount];

                Row = row;
                Column = column;
            }
        }

        // TODO: How should we pass nodes into Choreographer?
        public static Node[] Nodes = {};
        private static Dictionary<Node, Node> _nodes = new Dictionary<Node, Node>();

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
        public static readonly uint ColorGridLineHorizontal = ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.1f, 1f));
        public static readonly uint ColorGridLineVertical = ColorGridLineHorizontal;

        public static readonly Vector2 DefaultLineCurveStrength = new Vector2(40f, 40f);

        // Main Window
        public static readonly Vector2 ContentSize = Vector2.one * 20000f;
        public static readonly Vector2 CenterPosition = Vector2.one * 200f; //ContentSize / 2f;
        public static Vector2 ScrollPosition = Vector2.zero;

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
                    var inputSize = ImGui.CalcTextSize(node.Inputs[i].Name);
                    node.LeftSize.x = Mathf.Max(node.LeftSize.x, inputSize.x);
                    node.LeftSize.y += inputSize.y;
                    node.RowHeights[i] = inputSize.y;
                }
                node.LeftSize.x += imGuiStyle.WindowPadding.x + imGuiStyle.ItemSpacing.x * 2f + imGuiStyle.ItemInnerSpacing.x;

                for (var i = 0; i < node.Outputs.Length; ++i) {
                    var outputSize = ImGui.CalcTextSize(node.Outputs[i].Name);
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

            // TODO: HACK: ImGui shortcoming? We build a custom INI here so that we can avoid a bunch of OTHER hacks involving window pos/size...
            var iniData = new StringBuilder();
            foreach (var node in Nodes) {
                node.Pos = new Vector2(
                    node.Column == 0 ? (_columnSizes[node.Column].CumulativeSize - node.Size.x) / 2f : Padding * node.Column + _columnSizes[node.Column - 1].CumulativeSize + (_columnSizes[node.Column].CumulativeSize - _columnSizes[node.Column - 1].CumulativeSize - node.Size.x) / 2f, 
                    node.Row == 0 ? (_rowSizes[node.Row].CumulativeSize - node.Size.y) / 2f : Padding * node.Row + _rowSizes[node.Row - 1].CumulativeSize + (_rowSizes[node.Row].CumulativeSize - _rowSizes[node.Row - 1].CumulativeSize - node.Size.y) / 2f
                ) + CenterPosition;

                iniData.Append($"[Window][{node.Name}]\nPos={(int)node.Pos.x},{(int)node.Pos.y}\nSize={(int)node.Size.x},{(int)node.Size.y}\n");

                _nodes.Add(node, node);
            }
            ImGui.LoadIniSettingsFromMemory(iniData.ToString(), (uint) iniData.Length);
        }

        public static Vector2 Scale(Vector2 value) {
            return value * ImGui.GetIO().FontGlobalScale;
        }

        public static float Scale(float value) {
            return value * ImGui.GetIO().FontGlobalScale;
        }

        public static void DrawCurvedLine(ImDrawListPtr drawList, Vector2 start, Vector2 end, uint color, Vector2 curveStrength, float thickness) {
            var direction = (end - start).normalized;
            direction *= direction;
            drawList.AddBezierCurve(
            Scale(start - ScrollPosition),
            Scale(start + direction * curveStrength - ScrollPosition),
            Scale(end - direction * curveStrength - ScrollPosition),
            Scale(end - ScrollPosition),
                color,
                thickness
            );
        }

        public static void DrawCurvedLine(ImDrawListPtr drawList, Vector2 start, Vector2 end) {
            DrawCurvedLine(drawList, start, end, ColorWhite, DefaultLineCurveStrength, 2f);
        }

        private void OnLayout() {
            ImGui.SetNextWindowPos(Vector2.zero);
            ImGui.SetNextWindowSize(new Vector2(Screen.width, Screen.height));
            ImGui.SetNextWindowContentSize(Vector2.one * 20000f);
            ImGui.SetNextWindowFocus();
            if (ImGui.Begin("Choreographer", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoScrollbar)) {
                // TODO: HACK: ImGui shortcoming? Can't "SetNextScrollX" so have to this (nasty hack) here. >_<
                if (ScrollPosition.x == 0f) {
                    ImGui.SetScrollX(ScrollPosition.x = CenterPosition.x - Padding);
                    ImGui.SetScrollY(ScrollPosition.y = CenterPosition.y - Padding);
                } else {
                    ScrollPosition.x = ImGui.GetScrollX();
                    ScrollPosition.y = ImGui.GetScrollY();
                    if (ImGui.IsMouseDragging(ImGuiMouseButton.Right)) {
                        ScrollPosition -= ImGui.GetMouseDragDelta(ImGuiMouseButton.Right);
                        ImGui.ResetMouseDragDelta(ImGuiMouseButton.Right);
                        ImGui.SetScrollX(ScrollPosition.x);
                        ImGui.SetScrollY(ScrollPosition.y);
                    }
                    if (ScrollPosition.x < 1f) ImGui.SetScrollX(ScrollPosition.x = 1f);
                    if (ScrollPosition.y < 1f) ImGui.SetScrollY(ScrollPosition.y = 1f);
                }

                ImDrawListPtr bgDrawList = ImGui.GetBackgroundDrawList(), fgDrawList = ImGui.GetForegroundDrawList();
                bgDrawList.AddRectFilled(Vector2.zero, new Vector2(2000f, 2000f), ColorBackground);
                for (var i = 1; i < 19; ++i) {
                    bgDrawList.AddLine(new Vector2(0f, i * 100f), new Vector2(2000f, i * 100f), ColorGridLineHorizontal, 1f);
                    bgDrawList.AddLine(new Vector2(i * 100f, 0f), new Vector2(i * 100f, 2000f), ColorGridLineVertical, 1f);
                }

                var imGuiStyle = ImGui.GetStyle();
                foreach (var node in Nodes) {
                    node.Style.Push();
                    var flags = ImGuiWindowFlags.NoResize;
                    if (ReadOnly) flags |= ImGuiWindowFlags.NoMove;
                    if (ImGui.GetIO().FontGlobalScale != 1f) {
                        ImGui.SetNextWindowPos(Scale(node.Pos - ScrollPosition));
                        ImGui.SetNextWindowSize(Scale(node.Size));
                    }
                    if (ImGui.Begin(node.Name, flags)) {
                        var drawList = ImGui.IsWindowFocused() | ImGui.IsWindowHovered() ? ImGui.GetForegroundDrawList() : ImGui.GetBackgroundDrawList();
                        CustomEvent.Trigger(gameObject, "OnNode", drawList, node);
                        ImGui.Columns(2, "Column", false);
                        ImGui.SetColumnWidth(0, Scale(node.LeftSize.x));
                        ImGui.SetColumnOffset(1, Scale(node.LeftSize.x));
                        ImGui.SetColumnWidth(1, Scale(node.RightSize.x));
                        for (var i = 0; i < node.RowHeights.Length; ++i) {
                            if (i < node.Inputs.Length) {
                                ImGui.PushID(node.Inputs[i].Id);
                                if (ImGui.Selectable(node.Inputs[i].Name, false, ReadOnly ? ImGuiSelectableFlags.Disabled : ImGuiSelectableFlags.None, Scale(new Vector2(node.LeftSize.x, node.RowHeights[i])))) {
                                    CustomEvent.Trigger(gameObject, "OnInput", drawList, node.Inputs[i].Id);
                                }
                                CustomEvent.Trigger(gameObject, "OnConnection", drawList, new Vector2(ImGui.GetItemRectMin().x, (ImGui.GetItemRectMin().y + ImGui.GetItemRectMax().y) / 2f), -1, node.Inputs[i].Id, ReadOnly);
                                ImGui.PopID();
                            }

                            ImGui.NextColumn();
                            if (i < node.Outputs.Length) {
                                ImGui.PushID(node.Outputs[i].Id);
                                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(1.0f, 0f));
                                ImGui.Unindent(imGuiStyle.ItemSpacing.x);
                                if (ImGui.Selectable(node.Outputs[i].Name, false, ReadOnly ? ImGuiSelectableFlags.Disabled : ImGuiSelectableFlags.None, Scale(new Vector2(node.RightSize.x, node.RowHeights[i])))) {
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
                    //node.Pos = ImGui.GetWindowPos() + ScrollPosition;
                    ImGui.End();
                    node.Style.Pop();

                    // TODO: Don't draw connections if neither node is visible? Edge Case: If the viewport is in-between the two nodes, the line should be visible.

                    foreach (var connection in node.Connections) {
                        var parentNode = _nodes[connection.Parent];
                        switch (connection.Type) {
                        case Connection.ConnectionType.Inherited:
                            DrawCurvedLine(fgDrawList, new Vector2(parentNode.Pos.x + parentNode.Size.x, parentNode.Pos.y), node.Pos);
                            break;
                        case Connection.ConnectionType.Recursive:
                            DrawCurvedLine(fgDrawList, new Vector2(parentNode.Pos.x + parentNode.Size.x, parentNode.Pos.y + parentNode.Size.y), node.Pos + new Vector2(0f, node.Size.y));
                            break;
                        }
                    }
                }
            }
            ImGui.End();

            if (ImGui.BeginMainMenuBar()) {
                ImGui.Checkbox("Read Only", ref ReadOnly);
                if (ImGui.SliderFloat("Zoom", ref ImGui.GetIO().FontGlobalScale, 0.1f, 2f, "%.2f")) {
                    //_refresh();
                }
                ImGui.EndMainMenuBar();
            }

            CustomEvent.Trigger(gameObject, "OnLayout");
        }
    }
}