using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenMusicPlayer.Samples.HUD
{
    /// <summary>
    /// Helper class to prevent progress updates while dragging. 
    /// </summary>
    public class SliderInteractController : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public bool IsDragging { get; private set; }
        
        public void OnDrag(PointerEventData eventData) => IsDragging = true;
        public void OnEndDrag(PointerEventData eventData) => IsDragging = false;
    }
}