using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    public class EditingTool
    {
        public readonly ShapeEditor Owner;

        public EditingTool(ShapeEditor owner)
        {
            Owner = owner;
        }

        public virtual void OnEnable()
        {

        }
        public virtual void Repaint()
        {
          
        }

        public virtual void HandleInput(Event guiEvent)
        {
            
        }
    }
}