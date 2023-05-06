using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{

    public class ShapeCreator : MonoBehaviour, ISerializationCallbackReceiver
    {
        public MeshFilter MeshFilter;

       // public List<Shape> shapes = new List<Shape>();
        public float Depth = 1.0f;

        [HideInInspector]
        [SerializeField]
        private List<Shape> mShapes = new List<Shape>();

        public IEnumerable<Shape> Shapes => mShapes;

        public virtual void UpdateMeshDisplay()
        {
            CompositeShape compShape = new CompositeShape(mShapes);

            MeshFilter.mesh = compShape.GetMesh(this);        
        }

        public Shape this[int index]
        {
            get { return mShapes[index]; }
            set { mShapes[index] = value; }
        }

        public int Count
        {
            get { return mShapes.Count; }
        }

        public void Add(Shape shape)
        {
            mShapes.Add(shape);
        }

        public void RemoveAt(int index)
        {
            mShapes.RemoveAt(index);
        }

        public bool IsEmpty
        {
            get
            {
                for (var sIndex = 0; sIndex < mShapes.Count; sIndex++)
                {
                    var shape = mShapes[sIndex];
                    if (shape.Count != 0)
                        return false;
                }
                return true;
            }
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
          //  if (mShapes == null || mShapes.Count == 0)
          //  {
          //      if (shapes != null && shapes.Count > 0)
          //      {
          //          mShapes = new List<Shape>();
          //          foreach (var shape in shapes)
          //          {
          //              Shape newShape = new Shape();
          //              var oldPoints = shape.points.Select(v => new Point(v));
          //              foreach (var point in oldPoints)
          //                  newShape.Add(point);
          //
          //              mShapes.Add(newShape);
          //          }
          //      }
          //  }     
        }
    }
}