using System.Runtime.InteropServices;
using UnityEngine;
using ImGuiNET;

public class Choreographer : MonoBehaviour {
    [DllImport("Choreographer")] private static extern void choreographer_load();
    [DllImport("Choreographer")] private static extern void choreographer_update();
    [DllImport("Choreographer")] private static extern void choreographer_unload();

    private void OnEnable() {
        choreographer_load();
        ImGuiUn.Layout += OnLayout;
    }

    private void OnDisable() {
        ImGuiUn.Layout -= OnLayout;
        choreographer_unload();
    }

    private float _float = 1f;
    private bool[] _windows = new bool[3];
    private void OnLayout() {
        ImGui.Text($"Hello, world {123}");
        var buf = new byte[1024];
        ImGui.SliderFloat("float", ref _float, 0.0f, 1.0f);
        ImGui.InputText($"Slider: {_float}", buf, (uint) buf.Length);
        if (ImGui.Button("Demo")) _windows[0] = !_windows[0];
        if (_windows[0]) ImGui.ShowDemoWindow();
        if (ImGui.Button("About")) _windows[1] = !_windows[1];
        if (_windows[1]) ImGui.ShowAboutWindow();
        if (ImGui.Button("Metrics")) _windows[2] = !_windows[2];
        if (_windows[2]) ImGui.ShowMetricsWindow();

        // Choreographer's UI
        choreographer_update();
    }
}