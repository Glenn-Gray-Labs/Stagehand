using System;
using System.Collections.Generic;
using Bolt;
using ImGuiNET;
using Plugins.Backstage;
using UnityEngine;

public class Choreographer : MonoBehaviour {
    public interface IFlow { }
    public interface IFlowIn : IFlow { }
    public interface IFlowOut : IFlow { }

    [Serializable] public class Node {
        public Type Type;
        public Type[] Inputs;
        public Type[] Outputs;
    }
    private static readonly Node[] _nodes = {
        new Node {
            Type = typeof(Stage),
            Inputs = new[]{typeof(IFlowIn), typeof(int), typeof(float), typeof(bool), typeof(string), typeof(Vector2), typeof(Node)},
            Outputs = new[]{typeof(IFlowOut), typeof(Node), typeof(Vector2), typeof(int), typeof(float), typeof(bool), typeof(string)},
        },
        new Node {
            Type = typeof(Choreographer),
            Inputs = new[]{typeof(IFlowIn), typeof(float), typeof(Node)},
            Outputs = new[]{typeof(IFlowOut), typeof(Vector2)},
        },
        new Node {
            Type = typeof(Node),
            Inputs = new[]{typeof(IFlowIn), typeof(float), typeof(Node)},
            Outputs = new[]{typeof(IFlowOut), typeof(Vector2)},
        },
        new Node {
            Type = typeof(Choreographer),
            Inputs = new[]{typeof(IFlowIn)},
            Outputs = new[]{typeof(IFlowOut)},
        },
        new Node {
            Type = typeof(Type),
            Inputs = new[]{typeof(IFlowIn)},
            Outputs = new[]{typeof(IFlowOut)},
        },
    };

    public interface IStyle {
        Vector2 WindowPadding { get; }
        Vector2 FramePadding { get; }
        Vector2 ItemSpacing { get; }
        Vector2 ItemInnerSpacing { get; }
        float IndentSpacing { get; }
        float ScrollbarSize { get; }
        float GrabMinSize { get; }
        float WindowBorderSize { get; }
        float ChildBorderSize { get; }
        float PopupBorderSize { get; }
        float FrameBorderSize { get; }
        float WindowRounding { get; }
        float ChildRounding { get; }
        float FrameRounding { get; }
        float PopupRounding { get; }
        float ScrollbarRounding { get; }
        float GrabRounding { get; }
        float TabRounding { get; }
        Vector2 WindowTitleAlign { get; }
        
        Vector4 Text { get; }
        Vector4 TextDisabled { get; }
        Vector4 WindowBg { get; }
        Vector4 ChildBg { get; }
        Vector4 PopupBg { get; }
        Vector4 Border { get; }
        Vector4 BorderShadow { get; }
        Vector4 FrameBg { get; }
        Vector4 FrameBgHovered { get; }
        Vector4 FrameBgActive { get; }
        Vector4 TitleBg { get; }
        Vector4 TitleBgActive { get; }
        Vector4 TitleBgCollapsed { get; }
        Vector4 MenuBarBg { get; }
        Vector4 ScrollbarBg { get; }
        Vector4 ScrollbarGrab { get; }
        Vector4 ScrollbarGrabHovered { get; }
        Vector4 ScrollbarGrabActive { get; }
        Vector4 CheckMark { get; }
        Vector4 SliderGrab { get; }
        Vector4 SliderGrabActive { get; }
        Vector4 Button { get; }
        Vector4 ButtonHovered { get; }
        Vector4 ButtonActive { get; }
        Vector4 Header { get; }
        Vector4 HeaderHovered { get; }
        Vector4 HeaderActive { get; }
        Vector4 Separator { get; }
        Vector4 SeparatorHovered { get; }
        Vector4 SeparatorActive { get; }
        Vector4 ResizeGrip { get; }
        Vector4 ResizeGripHovered { get; }
        Vector4 ResizeGripActive { get; }
        Vector4 Tab { get; }
        Vector4 TabHovered { get; }
        Vector4 TabActive { get; }
        Vector4 TabUnfocused { get; }
        Vector4 TabUnfocusedActive { get; }
        Vector4 PlotLines { get; }
        Vector4 PlotLinesHovered { get; }
        Vector4 PlotHistogram { get; }
        Vector4 PlotHistogramHovered { get; }
        Vector4 TextSelectedBg { get; }
        Vector4 DragDropTarget { get; }
        Vector4 NavHighlight { get; }
        Vector4 NavWindowingHighlight { get; }
        Vector4 NavWindowingDimBg { get; }
        Vector4 ModalWindowDimBg { get; }
    }

    public class DefaultStyle : IStyle {
        public virtual Vector2 WindowPadding => ImGui.GetStyle().WindowPadding;
        public virtual Vector2 FramePadding => ImGui.GetStyle().FramePadding;
        public virtual Vector2 ItemSpacing => ImGui.GetStyle().ItemSpacing;
        public virtual Vector2 ItemInnerSpacing => ImGui.GetStyle().ItemInnerSpacing;
        public virtual float IndentSpacing => ImGui.GetStyle().IndentSpacing;
        public virtual float ScrollbarSize => ImGui.GetStyle().ScrollbarSize;
        public virtual float GrabMinSize => ImGui.GetStyle().GrabMinSize;
        public virtual float WindowBorderSize => ImGui.GetStyle().WindowBorderSize;
        public virtual float ChildBorderSize => ImGui.GetStyle().ChildBorderSize;
        public virtual float PopupBorderSize => ImGui.GetStyle().PopupBorderSize;
        public virtual float FrameBorderSize => ImGui.GetStyle().FrameBorderSize;
        public virtual float WindowRounding => ImGui.GetStyle().WindowRounding;
        public virtual float ChildRounding => ImGui.GetStyle().ChildRounding;
        public virtual float FrameRounding => ImGui.GetStyle().FrameRounding;
        public virtual float PopupRounding => ImGui.GetStyle().PopupRounding;
        public virtual float ScrollbarRounding => ImGui.GetStyle().ScrollbarRounding;
        public virtual float GrabRounding => ImGui.GetStyle().GrabRounding;
        public virtual float TabRounding => ImGui.GetStyle().TabRounding;
        public virtual Vector2 WindowTitleAlign => ImGui.GetStyle().WindowTitleAlign;

        public virtual Vector4 Text => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));
        public virtual Vector4 TextDisabled => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TextDisabled));
        public virtual Vector4 WindowBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.WindowBg));
        public virtual Vector4 ChildBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ChildBg));
        public virtual Vector4 PopupBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.PopupBg));
        public virtual Vector4 Border => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Border));
        public virtual Vector4 BorderShadow => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.BorderShadow));
        public virtual Vector4 FrameBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.FrameBg));
        public virtual Vector4 FrameBgHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.FrameBgHovered));
        public virtual Vector4 FrameBgActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.FrameBgActive));
        public virtual Vector4 TitleBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TitleBg));
        public virtual Vector4 TitleBgActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TitleBgActive));
        public virtual Vector4 TitleBgCollapsed => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TitleBgCollapsed));
        public virtual Vector4 MenuBarBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.MenuBarBg));
        public virtual Vector4 ScrollbarBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ScrollbarBg));
        public virtual Vector4 ScrollbarGrab => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ScrollbarGrab));
        public virtual Vector4 ScrollbarGrabHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ScrollbarGrabHovered));
        public virtual Vector4 ScrollbarGrabActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ScrollbarGrabActive));
        public virtual Vector4 CheckMark => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.CheckMark));
        public virtual Vector4 SliderGrab => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.SliderGrab));
        public virtual Vector4 SliderGrabActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.SliderGrabActive));
        public virtual Vector4 Button => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Button));
        public virtual Vector4 ButtonHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ButtonHovered));
        public virtual Vector4 ButtonActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ButtonActive));
        public virtual Vector4 Header => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Header));
        public virtual Vector4 HeaderHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.HeaderHovered));
        public virtual Vector4 HeaderActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.HeaderActive));
        public virtual Vector4 Separator => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Separator));
        public virtual Vector4 SeparatorHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.SeparatorHovered));
        public virtual Vector4 SeparatorActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.SeparatorActive));
        public virtual Vector4 ResizeGrip => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ResizeGrip));
        public virtual Vector4 ResizeGripHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ResizeGripHovered));
        public virtual Vector4 ResizeGripActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ResizeGripActive));
        public virtual Vector4 Tab => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Tab));
        public virtual Vector4 TabHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TabHovered));
        public virtual Vector4 TabActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TabActive));
        public virtual Vector4 TabUnfocused => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TabUnfocused));
        public virtual Vector4 TabUnfocusedActive => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TabUnfocusedActive));
        public virtual Vector4 PlotLines => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.PlotLines));
        public virtual Vector4 PlotLinesHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.PlotLinesHovered));
        public virtual Vector4 PlotHistogram => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.PlotHistogram));
        public virtual Vector4 PlotHistogramHovered => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.PlotHistogramHovered));
        public virtual Vector4 TextSelectedBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.TextSelectedBg));
        public virtual Vector4 DragDropTarget => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.DragDropTarget));
        public virtual Vector4 NavHighlight => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.NavHighlight));
        public virtual Vector4 NavWindowingHighlight => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.NavWindowingHighlight));
        public virtual Vector4 NavWindowingDimBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.NavWindowingDimBg));
        public virtual Vector4 ModalWindowDimBg => ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ModalWindowDimBg));
    }
    private static readonly DefaultStyle _defaultStyle = new DefaultStyle();

    public class AltStyle : DefaultStyle {
        public override Vector2 WindowPadding => new Vector2(4f, 4f);
        public override Vector2 FramePadding => new Vector2(4f, 6f);
        public override Vector2 ItemSpacing => new Vector2(0f, 6f);
        public override Vector2 ItemInnerSpacing => new Vector2(6f, 0f);
        public override float IndentSpacing => 0f;
        public override float ScrollbarSize => 20f;
        public override float FrameBorderSize => 1f;
        public override float WindowRounding => 6f;
        public override float FrameRounding => 6f;
        public override float ScrollbarRounding => 3f;
        public override float GrabRounding => 2f;
        public override Vector2 WindowTitleAlign => new Vector2(0f, 0.3333333f);

        public override Vector4 TitleBg => _defaultStyle.TitleBgActive;
    }
    private static readonly AltStyle _altStyle = new AltStyle();

    private Dictionary<Type, IStyle> _styles = new Dictionary<Type, IStyle> {
        { typeof(Type), new AltStyle() },
    };

    private void _pushStyle(IStyle style) {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, style.WindowPadding);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, style.FramePadding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, style.ItemSpacing);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, style.ItemInnerSpacing);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, style.IndentSpacing);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, style.ScrollbarSize);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, style.GrabMinSize);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, style.WindowBorderSize);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, style.ChildBorderSize);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, style.PopupBorderSize);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, style.FrameBorderSize);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, style.WindowRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, style.ChildRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, style.FrameRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, style.PopupRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, style.ScrollbarRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, style.GrabRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, style.TabRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign, style.WindowTitleAlign);

        ImGui.PushStyleColor(ImGuiCol.Text, style.Text);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, style.TextDisabled);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, style.WindowBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, style.ChildBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, style.PopupBg);
        ImGui.PushStyleColor(ImGuiCol.Border, style.Border);
        ImGui.PushStyleColor(ImGuiCol.BorderShadow, style.BorderShadow);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, style.FrameBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, style.FrameBgHovered);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, style.FrameBgActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, style.TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, style.TitleBgActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, style.TitleBgCollapsed);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, style.MenuBarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, style.ScrollbarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, style.ScrollbarGrab);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, style.ScrollbarGrabHovered);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, style.ScrollbarGrabActive);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, style.CheckMark);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, style.SliderGrab);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, style.SliderGrabActive);
        ImGui.PushStyleColor(ImGuiCol.Button, style.Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, style.ButtonHovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, style.ButtonActive);
        ImGui.PushStyleColor(ImGuiCol.Header, style.Header);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, style.HeaderHovered);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, style.HeaderActive);
        ImGui.PushStyleColor(ImGuiCol.Separator, style.Separator);
        ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, style.SeparatorHovered);
        ImGui.PushStyleColor(ImGuiCol.SeparatorActive, style.SeparatorActive);
        ImGui.PushStyleColor(ImGuiCol.ResizeGrip, style.ResizeGrip);
        ImGui.PushStyleColor(ImGuiCol.ResizeGripHovered, style.ResizeGripHovered);
        ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, style.ResizeGripActive);
        ImGui.PushStyleColor(ImGuiCol.Tab, style.Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, style.TabHovered);
        ImGui.PushStyleColor(ImGuiCol.TabActive, style.TabActive);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, style.TabUnfocused);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, style.TabUnfocusedActive);
        ImGui.PushStyleColor(ImGuiCol.PlotLines, style.PlotLines);
        ImGui.PushStyleColor(ImGuiCol.PlotLinesHovered, style.PlotLinesHovered);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, style.PlotHistogram);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogramHovered, style.PlotHistogramHovered);
        ImGui.PushStyleColor(ImGuiCol.TextSelectedBg, style.TextSelectedBg);
        ImGui.PushStyleColor(ImGuiCol.DragDropTarget, style.DragDropTarget);
        ImGui.PushStyleColor(ImGuiCol.NavHighlight, style.NavHighlight);
        ImGui.PushStyleColor(ImGuiCol.NavWindowingHighlight, style.NavWindowingHighlight);
        ImGui.PushStyleColor(ImGuiCol.NavWindowingDimBg, style.NavWindowingDimBg);
        ImGui.PushStyleColor(ImGuiCol.ModalWindowDimBg, style.ModalWindowDimBg);
    }

    private void _popStyle() {
        ImGui.PopStyleColor(48);
        ImGui.PopStyleVar(19);
    }

    private void OnEnable() {
        ImGuiUn.Layout += OnLayout;

        // Set Defaults
        var imGuiStyle = ImGui.GetStyle();
        imGuiStyle.WindowPadding = _altStyle.WindowPadding;
        imGuiStyle.FramePadding = _altStyle.FramePadding;
        imGuiStyle.ItemSpacing = _altStyle.ItemSpacing;
        imGuiStyle.ItemInnerSpacing = _altStyle.ItemInnerSpacing;
        imGuiStyle.IndentSpacing = _altStyle.IndentSpacing;
        imGuiStyle.ScrollbarSize = _altStyle.ScrollbarSize;
        imGuiStyle.GrabMinSize = _altStyle.GrabMinSize;
        imGuiStyle.WindowBorderSize = _altStyle.WindowBorderSize;
        imGuiStyle.ChildBorderSize = _altStyle.ChildBorderSize;
        imGuiStyle.PopupBorderSize = _altStyle.PopupBorderSize;
        imGuiStyle.FrameBorderSize = _altStyle.FrameBorderSize;
        imGuiStyle.WindowRounding = _altStyle.WindowRounding;
        imGuiStyle.ChildRounding = _altStyle.ChildRounding;
        imGuiStyle.FrameRounding = _altStyle.FrameRounding;
        imGuiStyle.PopupRounding = _altStyle.PopupRounding;
        imGuiStyle.ScrollbarRounding = _altStyle.ScrollbarRounding;
        imGuiStyle.GrabRounding = _altStyle.GrabRounding;
        imGuiStyle.TabRounding = _altStyle.TabRounding;
        imGuiStyle.WindowTitleAlign = _altStyle.WindowTitleAlign;
    }

    private void OnDisable() {
        ImGuiUn.Layout -= OnLayout;
    }

    private void OnLayout() {
        CustomEvent.Trigger(gameObject, "OnLayout", _nodes);

        var imGuiStyle = ImGui.GetStyle();

        var counter = 0;
        foreach (var node in _nodes) {
            IStyle currentStyle;
            if (_styles.ContainsKey(node.Type)) {
                currentStyle = _styles[node.Type];
                _pushStyle(currentStyle);
            } else {
                currentStyle = _defaultStyle;
            }

            var titleSize = ImGui.CalcTextSize($"{counter}. {node.Type.Name}");

            const int columnCount = 2;
            var rowCount = node.Inputs.Length > node.Outputs.Length ? node.Inputs.Length : node.Outputs.Length;

            var rowHeights = new float[rowCount];

            var leftSize = Vector2.zero;
            for (var i = 0; i < node.Inputs.Length; ++i) {
                var inputSize = ImGui.CalcTextSize($"{node.Inputs[i].Name}");
                leftSize.x = Mathf.Max(leftSize.x, inputSize.x);
                leftSize.y += inputSize.y;
                rowHeights[i] = inputSize.y;
            }
            leftSize.x += imGuiStyle.WindowPadding.x + imGuiStyle.ItemSpacing.x * 2f + imGuiStyle.ItemInnerSpacing.x;

            var rightSize = Vector2.zero;
            for (var i = 0; i < node.Outputs.Length; ++i) {
                var outputSize = ImGui.CalcTextSize($"{node.Outputs[i].Name}");
                rightSize.x = Mathf.Max(rightSize.x, outputSize.x);
                rightSize.y += outputSize.y;
                if (outputSize.y > rowHeights[i]) rowHeights[i] = outputSize.y;
            }
            rightSize.x += imGuiStyle.WindowPadding.x + imGuiStyle.ItemSpacing.x + imGuiStyle.ItemInnerSpacing.x;

            if (leftSize.x + rightSize.x > titleSize.x) titleSize.x = leftSize.x + rightSize.x;

            var windowSize = new Vector2(titleSize.x, titleSize.y + Mathf.Max(leftSize.y, rightSize.y));
            windowSize.x += imGuiStyle.WindowPadding.x + imGuiStyle.ItemSpacing.x / 2f;
            windowSize.y += imGuiStyle.WindowPadding.y + imGuiStyle.FramePadding.y * 2f + (imGuiStyle.ItemInnerSpacing.y + imGuiStyle.ItemSpacing.y) * rowCount;

            ImGui.SetNextWindowSize(windowSize);
            ImGui.Begin($"{counter++}. {node.Type.Name}", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            ImGui.Columns(columnCount, "", false);
            ImGui.SetColumnWidth(0, leftSize.x);
            ImGui.SetColumnOffset(1, leftSize.x);
            ImGui.SetColumnWidth(1, rightSize.x);
            for (var i = 0; i < rowCount; ++i) {
                if (i < node.Inputs.Length) ImGui.Selectable($"{node.Inputs[i].Name}", false, ImGuiSelectableFlags.None, new Vector2(leftSize.x, rowHeights[i]));
                ImGui.NextColumn();
                if (i < node.Outputs.Length) {
                    ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(1.0f, 0f));
                    ImGui.Unindent(imGuiStyle.ItemSpacing.x);
                    ImGui.Selectable($"{node.Outputs[i].Name}", false, ImGuiSelectableFlags.None, new Vector2(rightSize.x, rowHeights[i]));
                    ImGui.Indent(imGuiStyle.ItemSpacing.x);
                    ImGui.PopStyleVar();
                }
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
            ImGui.End();

            if (currentStyle != _defaultStyle) _popStyle();
        }
    }
}