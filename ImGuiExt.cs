using System.Drawing;
using ImGuiNET;
using StrongboxHelper.Extensions;

namespace StrongboxHelper
{
    public static class ImGuiExt
    {
        public static bool? DrawDeleteConfirmationPopup(string popupId, string? itemName)
        {
            bool? result = null;
            bool isOpen = true;
            
            if (ImGui.BeginPopupModal(popupId, ref isOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text($"Are you sure you want to delete {itemName ?? "this item"}?");
                ImGui.Separator();
                
                ImGui.PushStyleColor(ImGuiCol.Button, Color.Red.ToVector4());
                if (ImGui.Button("Yes, Delete"))
                {
                    result = true;
                }
                ImGui.PopStyleColor();
                
                ImGui.SameLine();
                
                if (ImGui.Button("Cancel"))
                {
                    result = false;
                }
                
                if (result.HasValue)
                {
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }
            
            return result;
        }
    }
}
