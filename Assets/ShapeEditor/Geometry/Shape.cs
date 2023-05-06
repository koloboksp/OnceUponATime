using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.ShapeEditor.Geometry
{
    [System.Serializable]
    public class Shape 
    {
        [FormerlySerializedAs("mPoints")] [SerializeField] private List<Point> _points = new List<Point>();

        public Point this[int index]
        {
            get { return _points[index]; }
            set { _points[index] = value; }
        }

        public int Count => _points.Count;
        public IEnumerable<Point> Points => _points;

        public void RemoveAt(int index)
        {
            _points.RemoveAt(index);
        }

        public void Insert(int index, Point point)
        {
            _points.Insert(index, point);
        }

        public void Add(Point point)
        {
            _points.Add(point);
        }
    }
    [Serializable]
    public struct Point
    {
        public Vector2 Position; 
        public Vector2 DepthOffset;
        
        public Point(Point src)
        {
            Position = src.Position;
            DepthOffset = src.DepthOffset;
        }

        public Point(Vector2 position)
        {
            Position = position;
            DepthOffset = Vector2.zero;
        }

        public Point(Vector2 position, Vector2 depthOffset) : this()
        {
            Position = position;
            DepthOffset = depthOffset;
        }
        public Point(Vector2 position, Side side, float depthOffset) : this()
        {
            Position = position;
            if(side == Side.Front)
                DepthOffset.x = depthOffset;
            else
                DepthOffset.y = depthOffset;
        }

      
        public void SetPosition(Vector3 position, Side side)
        {
            Position = position;

            if (side == Side.Front)
                DepthOffset.x = position.z;
            else
                DepthOffset.y = position.z;
        }
        public Vector3 GetPosition(float depth, Side side)
        {
            return new Vector3(Position.x, Position.y, depth + ((side == Side.Front) ? DepthOffset.x : DepthOffset.y));
        }

        public float GetDepthOffset(Side side)
        {
            if (side == Side.Front)
                return DepthOffset.x;

            return DepthOffset.y;
        }
    }

    public enum Side
    {
        Front,
        Back
    }
}