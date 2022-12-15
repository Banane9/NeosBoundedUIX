using BaseX;
using FrooxEngine.UIX;

namespace BoundedUIX
{
    internal class OriginalRect
    {
        public float2 AnchorMax { get; private set; }
        public float2 AnchorMin { get; private set; }
        public bool Local { get; set; }
        public float2 OffsetMax { get; private set; }
        public float2 OffsetMin { get; private set; }
        public float2 Position { get; private set; }

        public void Update(RectTransform rectTransform)
        {
            AnchorMin = rectTransform.AnchorMin;
            AnchorMax = rectTransform.AnchorMax;
            OffsetMin = rectTransform.OffsetMin;
            OffsetMax = rectTransform.OffsetMax;
            Position = rectTransform.Slot.LocalPosition.xy;
        }
    }
}