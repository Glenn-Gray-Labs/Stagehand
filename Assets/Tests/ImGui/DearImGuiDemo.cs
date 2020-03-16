using UnityEngine;
using ImGuiNET;

public class DearImGuiDemo : MonoBehaviour {
    private void OnEnable() {
        ImGuiUn.Layout += OnLayout;
    }

    private void OnDisable() {
        ImGuiUn.Layout -= OnLayout;
    }

    private float _float = 1f;
    private bool _showDemo;
    private void OnLayout() {
        ImGui.Text($"Hello, world {123}");
        var buf = new byte[1024];
        ImGui.SliderFloat("float", ref _float, 0.0f, 1.0f);
        ImGui.InputText($"Slider: {_float}", buf, (uint) buf.Length);
        if (ImGui.Button("Demo Window")) _showDemo = !_showDemo;
        if (_showDemo) ImGui.ShowDemoWindow();
    }
}