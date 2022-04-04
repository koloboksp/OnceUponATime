using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Effects
{
    public class TrailEffect : MonoBehaviour
    {
        public Vector3 Axis = Vector3.up;
        public bool Emit = false;
        public float Offset = 0.0f;
        public float Width = 1.0f;
        public float MaxVertexDistance = 0.05f;
        public float MaxAngle = 5.0f;
        public float LifeTime = 0.3f;
        public Material Material;

        readonly List<int> mIndexesOfFreePoints = new List<int>();
        readonly List<int> mIndexesOfUsedPoints = new List<int>();
        readonly List<Point> mPoints = new List<Point>();

        readonly ReallocArray<Vector3> mMeshVertices = new ReallocArray<Vector3>();
        readonly ReallocArray<Vector2> mMeshUVs = new ReallocArray<Vector2>();
        readonly ReallocArray<int> mMeshTriangles = new ReallocArray<int>();
        readonly ReallocArray<Color> mMeshVertexColors = new ReallocArray<Color>();

        Vector3 mPreviousPosition;
        Quaternion mPreviousRotation;

        GameObject mTrailObject = null;
        Mesh mTrailMesh = null;
        Material mInstanceMaterial;

        void Start()
        {
            mPreviousPosition = transform.position;
            mPreviousRotation = transform.rotation;

            if (mTrailObject == null)
            {
                mTrailObject = new GameObject(gameObject.name + Mathf.Round(Random.Range(1, 256)));

                mTrailObject.transform.parent = null;
                mTrailObject.transform.position = Vector3.zero;
                mTrailObject.transform.rotation = Quaternion.identity;
                mTrailObject.transform.localScale = Vector3.one;


                mTrailMesh = new Mesh();
                mTrailMesh.name = "Trail";
                MeshFilter meshFilter = mTrailObject.AddComponent<MeshFilter>();
                meshFilter.mesh = mTrailMesh;
                MeshRenderer meshRenderer = mTrailObject.AddComponent<MeshRenderer>();

                mInstanceMaterial = new Material(Material);
                meshRenderer.material = mInstanceMaterial;
            }
        }
        void OnDestroy()
        {
            mPoints.Clear();
            mIndexesOfFreePoints.Clear();
            mIndexesOfUsedPoints.Clear();

            if (mTrailObject != null)
            {
                Destroy(mTrailObject);
                mTrailObject = null;
            }
            if (mInstanceMaterial != null)
            {
                Destroy(mInstanceMaterial);
                mInstanceMaterial = null;
            }
            if (mTrailMesh != null)
            {
                Destroy(mTrailMesh);
                mTrailMesh = null;
            }
        }
      
        public void OnEnable()
        {
            if (mTrailObject != null)
            {
                mTrailObject.SetActive(true);
            }
        }

        public void OnDisable()
        {
            if (mTrailObject != null)
            {
                mTrailObject.SetActive(false);
            }
        }

        internal void AddNewPoint()
        {
            if (mIndexesOfFreePoints.Count > 0)
            {
                int freePointIndex = mIndexesOfFreePoints[mIndexesOfFreePoints.Count - 1];
                mIndexesOfFreePoints.RemoveAt(mIndexesOfFreePoints.Count - 1);
                mIndexesOfUsedPoints.Add(freePointIndex);

                Point reusedPoint = mPoints[freePointIndex];
                reusedPoint.Reset(LifeTime, this.transform); 
            }
            else
            {
                mPoints.Add(new Point(LifeTime, this.transform));
                mIndexesOfUsedPoints.Add(mPoints.Count - 1);
            }
        }

        
        void Update()
        {
            if (Emit)
            {             
                if (mIndexesOfUsedPoints.Count >= 2)
                {
                    Point lastPoint = mPoints[mIndexesOfUsedPoints[mIndexesOfUsedPoints.Count - 1]];
                    Point penultPoint = mPoints[mIndexesOfUsedPoints[mIndexesOfUsedPoints.Count - 2]];

                    float distance = (penultPoint.Position - lastPoint.Position).magnitude;
                    if (distance >= MaxVertexDistance ||
                        Quaternion.Angle(penultPoint.Rotation, lastPoint.Rotation) >= MaxAngle)
                    {
                        AddNewPoint();
                    }   
                }
                else
                {
                    if (mPreviousPosition != transform.position ||
                        mPreviousRotation != transform.rotation)
                    {
                        AddNewPoint();
                        AddNewPoint();
                    }
                    else
                    {
                        mPreviousPosition = transform.position;
                        mPreviousRotation = transform.rotation;
                    }
                }
  
                if (mIndexesOfUsedPoints.Count > 0)
                {
                    Point lastPoint = mPoints[mIndexesOfUsedPoints[mIndexesOfUsedPoints.Count - 1]];
                    lastPoint.UpdatePose(this.transform);
                }  
            }

            if(mIndexesOfUsedPoints.Count > 0)
            {
                if (mIndexesOfUsedPoints.Count > 2)
                {
                    for (int iupIndex = 0; iupIndex < mIndexesOfUsedPoints.Count - 2; iupIndex++)
                    {
                        Point point = mPoints[mIndexesOfUsedPoints[iupIndex]];
                        Point previousPoint = mPoints[mIndexesOfUsedPoints[iupIndex + 1]];

                        if (point.ElapsedTime >= point.LifeTime && 
                            previousPoint.ElapsedTime >= previousPoint.LifeTime)
                            point.MarkToRemove();
                    }
                }
                else
                {
                    Point point = mPoints[mIndexesOfUsedPoints[0]];
                    Point previousPoint = mPoints[mIndexesOfUsedPoints[1]];
                    if (point.ElapsedTime >= point.LifeTime && 
                        previousPoint.ElapsedTime >= previousPoint.LifeTime)
                    {
                        point.MarkToRemove();
                        previousPoint.MarkToRemove();
                    }
                }

                for (int iupIndex = mIndexesOfUsedPoints.Count - 1; iupIndex >= 0; iupIndex--)
                {
                    var indexOfUsedPoint = mIndexesOfUsedPoints[iupIndex];
                    Point point = mPoints[indexOfUsedPoint];
                    
                    if (point.NeedToRemove)
                    {
                        mIndexesOfUsedPoints.RemoveAt(iupIndex);
                        mIndexesOfFreePoints.Add(indexOfUsedPoint);
                    }
                    else
                        point.Update(Time.deltaTime);
                }

                mMeshVertices.CheckSize(mIndexesOfUsedPoints.Count * 2);
                mMeshUVs.CheckSize(mIndexesOfUsedPoints.Count * 2);
                mMeshTriangles.CheckSize((mIndexesOfUsedPoints.Count - 1) * 6);
                mMeshVertexColors.CheckSize(mIndexesOfUsedPoints.Count * 2);

                Vector3 anchorPosition = Vector3.zero;
                for (int iupIndex = 0, pointQueueIndex = 0; iupIndex < mIndexesOfUsedPoints.Count; iupIndex++, pointQueueIndex++)
                {
                    int indexesOfUsedPoint = mIndexesOfUsedPoints[iupIndex];
                    Point point = mPoints[indexesOfUsedPoint];
                    if (pointQueueIndex == 0)
                        anchorPosition = point.Position;
                    
                    float normLifeTime = point.ElapsedTime / point.LifeTime;

                    Color color = Color.Lerp(Color.white, Color.clear, normLifeTime);

                    mMeshVertexColors[pointQueueIndex * 2] = color;
                    mMeshVertexColors[(pointQueueIndex * 2) + 1] = color;

                    float width = Width;
                 
                    mMeshVertices[pointQueueIndex * 2] = point.Position + point.Rotation * Axis * (Offset + width * 0.5f) - anchorPosition;
                    mMeshVertices[(pointQueueIndex * 2) + 1] = point.Position + point.Rotation * Axis * (Offset - width * 0.5f) - anchorPosition;

                    float uvRatio = normLifeTime;
                    mMeshUVs[pointQueueIndex * 2] = new Vector2(uvRatio, 0);
                    mMeshUVs[(pointQueueIndex * 2) + 1] = new Vector2(uvRatio, 1);

                    if (pointQueueIndex != 0)
                    {
                        int triIndex = (pointQueueIndex - 1) * 6;
                        int vertIndex = pointQueueIndex * 2;
                        mMeshTriangles[triIndex + 0] = vertIndex - 2;
                        mMeshTriangles[triIndex + 1] = vertIndex - 1;
                        mMeshTriangles[triIndex + 2] = vertIndex - 0;

                        mMeshTriangles[triIndex + 3] = vertIndex + 1;
                        mMeshTriangles[triIndex + 4] = vertIndex + 0;
                        mMeshTriangles[triIndex + 5] = vertIndex - 1;
                    }
                }

                if (mIndexesOfUsedPoints.Count > 0)
                {
                    for (int i = (mIndexesOfUsedPoints.Count - 1) * 6; i < mMeshTriangles.Length; i++)
                    {
                        mMeshTriangles[i] = 0;
                    }
                }

                if (mTrailObject != null)
                {
                    mTrailObject.transform.position = anchorPosition;
                    mTrailObject.transform.rotation = Quaternion.identity;
                }

                mTrailMesh.Clear();
                mTrailMesh.vertices = mMeshVertices.Array;
                mTrailMesh.colors = mMeshVertexColors.Array;
                mTrailMesh.uv = mMeshUVs.Array;
                mTrailMesh.triangles = mMeshTriangles.Array;
                mTrailMesh.UploadMeshData(false);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.gameObject.transform.position + this.gameObject.transform.rotation * Axis * (Offset + Width * 0.5f), 
                this.gameObject.transform.position + this.gameObject.transform.rotation * Axis * (Offset - Width * 0.5f));
        }

        public class ReallocArray<T>
        {
            T[] mArray;

            public void CheckSize(int newSize)
            {
                if (newSize > 0)
                {
                    if (mArray == null)
                    {
                        mArray = new T[newSize];
                    }
                    else if (mArray.Length < newSize)
                    {
                        mArray = new T[newSize];
                    }
                    else
                    {

                    }
                }
            }
            public T this[int key]
            {
                get { return mArray[key]; }
                set
                {
                    mArray[key] = value;
                }
            }
            public T[] Array
            {
                get { return mArray; }
            }
            public int Length
            {
                get
                {
                    if (mArray != null)
                        return mArray.Length;

                    return 0;
                }
            }
        }
        class Point
        {
            public float ElapsedTime;
            public float LifeTime;
            public Vector3 Position;
            public Quaternion Rotation;
            bool mRemove;

            public Point(float lifeTime, Transform transform)
            {
                Reset(lifeTime, transform);
            }

            public void Reset(float lifeTime, Transform transform)
            {
                Position = transform.position;
                Rotation = transform.rotation;
                LifeTime = lifeTime;
                ElapsedTime = 0.0f;

                mRemove = false;
            }

            public void Update(float deltaTime)
            {
                ElapsedTime = Mathf.Clamp(ElapsedTime + deltaTime, 0.0f, LifeTime);
            }

            public void MarkToRemove()
            {
                mRemove = true;
            }

            public bool NeedToRemove { get { return mRemove; } }

            public void UpdatePose(Transform transform)
            {
                Position = transform.position;
                Rotation = transform.rotation;
            }
        }
    }
}