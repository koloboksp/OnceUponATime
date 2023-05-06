using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.ShapeEditor
{

    public class ShapeCreator : MonoBehaviour, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("MeshFilter")] [SerializeField] private MeshFilter _meshFilter;
        [FormerlySerializedAs("Depth")] [SerializeField] private float _depth = 1.0f;
        [FormerlySerializedAs("mShapes")]
        [HideInInspector]
        [SerializeField]
        private List<Shape> _shapes = new List<Shape>();

        public IEnumerable<Shape> Shapes => _shapes;

        public float Depth => _depth;
        public MeshFilter MeshFilter => _meshFilter;

        public virtual void UpdateMeshDisplay()
        {
            CompositeShape compShape = new CompositeShape(_shapes);

            _meshFilter.mesh = compShape.GetMesh(this);        
        }

        public Shape this[int index]
        {
            get { return _shapes[index]; }
            set { _shapes[index] = value; }
        }

        public int Count
        {
            get { return _shapes.Count; }
        }

        public void Add(Shape shape)
        {
            _shapes.Add(shape);
        }

        public void RemoveAt(int index)
        {
            _shapes.RemoveAt(index);
        }

        public bool IsEmpty
        {
            get
            {
                for (var sIndex = 0; sIndex < _shapes.Count; sIndex++)
                {
                    var shape = _shapes[sIndex];
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