using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Effects
{
    public class TrailEffect : MonoBehaviour
    {
        private readonly List<int> _indexesOfFreePoints = new List<int>();
        private readonly List<int> _indexesOfUsedPoints = new List<int>();
        private readonly List<Point> _points = new List<Point>();

        private readonly ReallocArray<Vector3> _meshVertices = new ReallocArray<Vector3>();
        private readonly ReallocArray<Vector2> _meshUVs = new ReallocArray<Vector2>();
        private readonly ReallocArray<int> _meshTriangles = new ReallocArray<int>();
        private readonly ReallocArray<Color> _meshVertexColors = new ReallocArray<Color>();

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;

        private GameObject _trailObject = null;
        private Mesh _trailMesh = null;
        private Material _instanceMaterial;

        [FormerlySerializedAs("Axis")] [SerializeField] private Vector3 _axis = Vector3.up;
        [FormerlySerializedAs("Emit")] [SerializeField] private bool _emit = false;
        [FormerlySerializedAs("Offset")] [SerializeField] private float _offset = 0.0f;
        [FormerlySerializedAs("Width")] [SerializeField] private float _width = 1.0f;
        [FormerlySerializedAs("MaxVertexDistance")] [SerializeField] private float _maxVertexDistance = 0.05f;
        [FormerlySerializedAs("MaxAngle")] [SerializeField] private float _maxAngle = 5.0f;
        [FormerlySerializedAs("LifeTime")] [SerializeField] private float _lifeTime = 0.3f;
        [FormerlySerializedAs("Material")] [SerializeField] private Material _material;
        
        public bool Emit
        {
            get => _emit;
            set => _emit = value;
        }

        private void Start()
        {
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;

            if (_trailObject == null)
            {
                _trailObject = new GameObject(gameObject.name + Mathf.Round(Random.Range(1, 256)));

                _trailObject.transform.parent = null;
                _trailObject.transform.position = Vector3.zero;
                _trailObject.transform.rotation = Quaternion.identity;
                _trailObject.transform.localScale = Vector3.one;


                _trailMesh = new Mesh();
                _trailMesh.name = "Trail";
                MeshFilter meshFilter = _trailObject.AddComponent<MeshFilter>();
                meshFilter.mesh = _trailMesh;
                MeshRenderer meshRenderer = _trailObject.AddComponent<MeshRenderer>();

                _instanceMaterial = new Material(_material);
                meshRenderer.material = _instanceMaterial;
            }
        }

        private void OnDestroy()
        {
            _points.Clear();
            _indexesOfFreePoints.Clear();
            _indexesOfUsedPoints.Clear();

            if (_trailObject != null)
            {
                Destroy(_trailObject);
                _trailObject = null;
            }
            if (_instanceMaterial != null)
            {
                Destroy(_instanceMaterial);
                _instanceMaterial = null;
            }
            if (_trailMesh != null)
            {
                Destroy(_trailMesh);
                _trailMesh = null;
            }
        }
      
        public void OnEnable()
        {
            if (_trailObject != null)
            {
                _trailObject.SetActive(true);
            }
        }

        public void OnDisable()
        {
            if (_trailObject != null)
            {
                _trailObject.SetActive(false);
            }
        }

        internal void AddNewPoint()
        {
            if (_indexesOfFreePoints.Count > 0)
            {
                int freePointIndex = _indexesOfFreePoints[_indexesOfFreePoints.Count - 1];
                _indexesOfFreePoints.RemoveAt(_indexesOfFreePoints.Count - 1);
                _indexesOfUsedPoints.Add(freePointIndex);

                Point reusedPoint = _points[freePointIndex];
                reusedPoint.Reset(_lifeTime, this.transform); 
            }
            else
            {
                _points.Add(new Point(_lifeTime, this.transform));
                _indexesOfUsedPoints.Add(_points.Count - 1);
            }
        }


        private void Update()
        {
            if (_emit)
            {             
                if (_indexesOfUsedPoints.Count >= 2)
                {
                    Point lastPoint = _points[_indexesOfUsedPoints[_indexesOfUsedPoints.Count - 1]];
                    Point penultPoint = _points[_indexesOfUsedPoints[_indexesOfUsedPoints.Count - 2]];

                    float distance = (penultPoint.Position - lastPoint.Position).magnitude;
                    if (distance >= _maxVertexDistance ||
                        Quaternion.Angle(penultPoint.Rotation, lastPoint.Rotation) >= _maxAngle)
                    {
                        AddNewPoint();
                    }   
                }
                else
                {
                    if (_previousPosition != transform.position ||
                        _previousRotation != transform.rotation)
                    {
                        AddNewPoint();
                        AddNewPoint();
                    }
                    else
                    {
                        _previousPosition = transform.position;
                        _previousRotation = transform.rotation;
                    }
                }
  
                if (_indexesOfUsedPoints.Count > 0)
                {
                    Point lastPoint = _points[_indexesOfUsedPoints[_indexesOfUsedPoints.Count - 1]];
                    lastPoint.UpdatePose(this.transform);
                }  
            }

            if(_indexesOfUsedPoints.Count > 0)
            {
                if (_indexesOfUsedPoints.Count > 2)
                {
                    for (int iupIndex = 0; iupIndex < _indexesOfUsedPoints.Count - 2; iupIndex++)
                    {
                        Point point = _points[_indexesOfUsedPoints[iupIndex]];
                        Point previousPoint = _points[_indexesOfUsedPoints[iupIndex + 1]];

                        if (point.ElapsedTime >= point.LifeTime && 
                            previousPoint.ElapsedTime >= previousPoint.LifeTime)
                            point.MarkToRemove();
                    }
                }
                else
                {
                    Point point = _points[_indexesOfUsedPoints[0]];
                    Point previousPoint = _points[_indexesOfUsedPoints[1]];
                    if (point.ElapsedTime >= point.LifeTime && 
                        previousPoint.ElapsedTime >= previousPoint.LifeTime)
                    {
                        point.MarkToRemove();
                        previousPoint.MarkToRemove();
                    }
                }

                for (int iupIndex = _indexesOfUsedPoints.Count - 1; iupIndex >= 0; iupIndex--)
                {
                    var indexOfUsedPoint = _indexesOfUsedPoints[iupIndex];
                    Point point = _points[indexOfUsedPoint];
                    
                    if (point.NeedToRemove)
                    {
                        _indexesOfUsedPoints.RemoveAt(iupIndex);
                        _indexesOfFreePoints.Add(indexOfUsedPoint);
                    }
                    else
                        point.Update(Time.deltaTime);
                }

                _meshVertices.CheckSize(_indexesOfUsedPoints.Count * 2);
                _meshUVs.CheckSize(_indexesOfUsedPoints.Count * 2);
                _meshTriangles.CheckSize((_indexesOfUsedPoints.Count - 1) * 6);
                _meshVertexColors.CheckSize(_indexesOfUsedPoints.Count * 2);

                Vector3 anchorPosition = Vector3.zero;
                for (int iupIndex = 0, pointQueueIndex = 0; iupIndex < _indexesOfUsedPoints.Count; iupIndex++, pointQueueIndex++)
                {
                    int indexesOfUsedPoint = _indexesOfUsedPoints[iupIndex];
                    Point point = _points[indexesOfUsedPoint];
                    if (pointQueueIndex == 0)
                        anchorPosition = point.Position;
                    
                    float normLifeTime = point.ElapsedTime / point.LifeTime;

                    Color color = Color.Lerp(Color.white, Color.clear, normLifeTime);

                    _meshVertexColors[pointQueueIndex * 2] = color;
                    _meshVertexColors[(pointQueueIndex * 2) + 1] = color;

                    float width = _width;
                 
                    _meshVertices[pointQueueIndex * 2] = point.Position + point.Rotation * _axis * (_offset + width * 0.5f) - anchorPosition;
                    _meshVertices[(pointQueueIndex * 2) + 1] = point.Position + point.Rotation * _axis * (_offset - width * 0.5f) - anchorPosition;

                    float uvRatio = normLifeTime;
                    _meshUVs[pointQueueIndex * 2] = new Vector2(uvRatio, 0);
                    _meshUVs[(pointQueueIndex * 2) + 1] = new Vector2(uvRatio, 1);

                    if (pointQueueIndex != 0)
                    {
                        int triIndex = (pointQueueIndex - 1) * 6;
                        int vertIndex = pointQueueIndex * 2;
                        _meshTriangles[triIndex + 0] = vertIndex - 2;
                        _meshTriangles[triIndex + 1] = vertIndex - 1;
                        _meshTriangles[triIndex + 2] = vertIndex - 0;

                        _meshTriangles[triIndex + 3] = vertIndex + 1;
                        _meshTriangles[triIndex + 4] = vertIndex + 0;
                        _meshTriangles[triIndex + 5] = vertIndex - 1;
                    }
                }

                if (_indexesOfUsedPoints.Count > 0)
                {
                    for (int i = (_indexesOfUsedPoints.Count - 1) * 6; i < _meshTriangles.Length; i++)
                    {
                        _meshTriangles[i] = 0;
                    }
                }

                if (_trailObject != null)
                {
                    _trailObject.transform.position = anchorPosition;
                    _trailObject.transform.rotation = Quaternion.identity;
                }

                _trailMesh.Clear();
                _trailMesh.vertices = _meshVertices.Array;
                _trailMesh.colors = _meshVertexColors.Array;
                _trailMesh.uv = _meshUVs.Array;
                _trailMesh.triangles = _meshTriangles.Array;
                _trailMesh.UploadMeshData(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.gameObject.transform.position + this.gameObject.transform.rotation * _axis * (_offset + _width * 0.5f), 
                this.gameObject.transform.position + this.gameObject.transform.rotation * _axis * (_offset - _width * 0.5f));
        }

        public class ReallocArray<T>
        {
            private T[] _mArray;

            public void CheckSize(int newSize)
            {
                if (newSize > 0)
                {
                    if (_mArray == null)
                    {
                        _mArray = new T[newSize];
                    }
                    else if (_mArray.Length < newSize)
                    {
                        _mArray = new T[newSize];
                    }
                    else
                    {

                    }
                }
            }
            public T this[int key]
            {
                get { return _mArray[key]; }
                set
                {
                    _mArray[key] = value;
                }
            }
            public T[] Array
            {
                get { return _mArray; }
            }
            public int Length
            {
                get
                {
                    if (_mArray != null)
                        return _mArray.Length;

                    return 0;
                }
            }
        }

        private class Point
        {
            private bool _remove;
            public float ElapsedTime;
            public float LifeTime;
            public Vector3 Position;
            public Quaternion Rotation;
            
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

                _remove = false;
            }

            public void Update(float deltaTime)
            {
                ElapsedTime = Mathf.Clamp(ElapsedTime + deltaTime, 0.0f, LifeTime);
            }

            public void MarkToRemove()
            {
                _remove = true;
            }

            public bool NeedToRemove { get { return _remove; } }

            public void UpdatePose(Transform transform)
            {
                Position = transform.position;
                Rotation = transform.rotation;
            }
        }
    }
}