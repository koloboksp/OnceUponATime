using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ShapeEditor.Geometry
{
    [System.Serializable]
    public class Shape 
    {
        public List<Vector2> points = new List<Vector2>();

        [SerializeField]
        List<Point> mPoints = new List<Point>();

        public Point this[int index]
        {
            get { return mPoints[index]; }
            set { mPoints[index] = value; }
        }

        public int Count => mPoints.Count;
        public IEnumerable<Point> Points => mPoints;

        public void RemoveAt(int index)
        {
            mPoints.RemoveAt(index);
        }

        public void Insert(int index, Point point)
        {
            mPoints.Insert(index, point);
        }

        public void Add(Point point)
        {
            mPoints.Add(point);
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