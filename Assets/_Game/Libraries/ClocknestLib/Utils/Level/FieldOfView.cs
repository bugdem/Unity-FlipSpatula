// Extension of Sebastian Lague's FieldOfView. 
// Credits to Sebastian Lague.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ClocknestGames.Library.Utils
{
	public class FieldOfView : MonoBehaviour 
	{
#pragma warning disable 0649
		[SerializeField] private float _viewRadius;
		[Range(0, 360)]
		[SerializeField] private float _viewAngle;

		[SerializeField] private LayerMask _targetMask;
		[SerializeField] private LayerMask _obstacleMask;

		[SerializeField] private float _meshResolution;
		[SerializeField] private int _edgeResolveIterations;
		[SerializeField] private float _edgeDstThreshold;
		[SerializeField] private float _maskCutawayDst = .1f;
		[SerializeField] private MeshFilter _viewMeshFilter;
#pragma warning restore 0649

		public float ViewRadius => _viewRadius;
		public float ViewAngle => ViewAngle;
		public List<Transform> VisibleTargets => _visibleTargets;

		private List<Transform> _visibleTargets = new List<Transform>();
		private Mesh _viewMesh;

		void Start()
		{
			_viewMesh = new Mesh();
			_viewMesh.name = "View Mesh";
			_viewMeshFilter.mesh = _viewMesh;

			StartCoroutine("FindTargetsWithDelay", .2f);
		}


		IEnumerator FindTargetsWithDelay(float delay)
		{
			while (true)
			{
				yield return new WaitForSeconds(delay);
				FindVisibleTargets();
			}
		}

		void LateUpdate()
		{
			DrawFieldOfView();
		}

		void FindVisibleTargets()
		{
			_visibleTargets.Clear();
			Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _viewRadius, _targetMask);

			for (int i = 0; i < targetsInViewRadius.Length; i++)
			{
				Transform target = targetsInViewRadius[i].transform;
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				if (Vector3.Angle(transform.forward, dirToTarget) < _viewAngle / 2)
				{
					float dstToTarget = Vector3.Distance(transform.position, target.position);
					if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, _obstacleMask))
					{
						_visibleTargets.Add(target);
					}
				}
			}
		}

		void DrawFieldOfView()
		{
			int stepCount = Mathf.RoundToInt(_viewAngle * _meshResolution);
			float stepAngleSize = _viewAngle / stepCount;
			List<Vector3> viewPoints = new List<Vector3>();
			ViewCastInfo oldViewCast = new ViewCastInfo();
			for (int i = 0; i <= stepCount; i++)
			{
				float angle = transform.eulerAngles.y - _viewAngle / 2 + stepAngleSize * i;
				ViewCastInfo newViewCast = ViewCast(angle);

				if (i > 0)
				{
					bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > _edgeDstThreshold;
					if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
					{
						EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
						if (edge.pointA != Vector3.zero)
						{
							viewPoints.Add(edge.pointA);
						}
						if (edge.pointB != Vector3.zero)
						{
							viewPoints.Add(edge.pointB);
						}
					}

				}


				viewPoints.Add(newViewCast.point);
				oldViewCast = newViewCast;
			}

			int vertexCount = viewPoints.Count + 1;
			Vector3[] vertices = new Vector3[vertexCount];
			int[] triangles = new int[(vertexCount - 2) * 3];

			vertices[0] = Vector3.zero;
			for (int i = 0; i < vertexCount - 1; i++)
			{
				vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * _maskCutawayDst;

				if (i < vertexCount - 2)
				{
					triangles[i * 3] = 0;
					triangles[i * 3 + 1] = i + 1;
					triangles[i * 3 + 2] = i + 2;
				}
			}

			_viewMesh.Clear();

			_viewMesh.vertices = vertices;
			_viewMesh.triangles = triangles;
			_viewMesh.RecalculateNormals();
		}


		EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
		{
			float minAngle = minViewCast.angle;
			float maxAngle = maxViewCast.angle;
			Vector3 minPoint = Vector3.zero;
			Vector3 maxPoint = Vector3.zero;

			for (int i = 0; i < _edgeResolveIterations; i++)
			{
				float angle = (minAngle + maxAngle) / 2;
				ViewCastInfo newViewCast = ViewCast(angle);

				bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > _edgeDstThreshold;
				if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
				{
					minAngle = angle;
					minPoint = newViewCast.point;
				}
				else
				{
					maxAngle = angle;
					maxPoint = newViewCast.point;
				}
			}

			return new EdgeInfo(minPoint, maxPoint);
		}


		ViewCastInfo ViewCast(float globalAngle)
		{
			Vector3 dir = DirFromAngle(globalAngle, true);
			RaycastHit hit;

			if (Physics.Raycast(transform.position, dir, out hit, _viewRadius, _obstacleMask))
			{
				return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
			}
			else
			{
				return new ViewCastInfo(false, transform.position + dir * _viewRadius, _viewRadius, globalAngle);
			}
		}

		public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
		{
			if (!angleIsGlobal)
			{
				angleInDegrees += transform.eulerAngles.y;
			}
			return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		}

		public struct ViewCastInfo
		{
			public bool hit;
			public Vector3 point;
			public float dst;
			public float angle;

			public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
			{
				hit = _hit;
				point = _point;
				dst = _dst;
				angle = _angle;
			}
		}

		public struct EdgeInfo
		{
			public Vector3 pointA;
			public Vector3 pointB;

			public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
			{
				pointA = _pointA;
				pointB = _pointB;
			}
		}

	}
}