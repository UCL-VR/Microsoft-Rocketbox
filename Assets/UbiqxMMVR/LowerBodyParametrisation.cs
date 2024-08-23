using MotionMatching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Ubiq.MotionMatching
{
    [Serializable]
    public struct PolarCoordinate
    {
        /// <summary>
        /// Angular component of the coordinate, relative to the T Pose
        /// </summary>
        public float position;

        /// <summary>
        /// Distance from the Hips to the Position, in Percent, relative to the T Pose
        /// </summary>
        public float radius;

        /// <summary>
        /// Lateral distance from the Hips to the Position, relative to the T Pose
        /// </summary>
        public float spread;
    }

    [Serializable]
    public struct LegPose
    {
        /// <summary>
        /// The position relative to the hip pivot point in polar coordinates
        /// </summary>
        public PolarCoordinate ankle;

        /// <summary>
        /// The orientation of the Knee relative to the pivot point, expressed
        /// as an Euler angle to the desired position from the reference vector
        /// </summary>
        public float knee;
    }


    [Serializable]
    public struct LowerBodyParams
    {
        public PolarCoordinate left;
        public PolarCoordinate right;
    }

    [Serializable]
    public class Leg
    {
        public Transform pivot;
        public Transform knee;
        public Transform ankle;
        public Transform toes;

        public float upperLength;
        public float lowerLength;

        public float length;
        public Vector3 offset; // From the avatar hips to the root of the leg

        public Leg(Transform hips, Transform pivot, Transform knee, Transform ankle, Transform toes)
        {
            this.pivot = pivot;
            this.knee = knee;
            this.ankle = ankle;
            this.toes = toes;
            this.length = (pivot.position - ankle.position).magnitude;
            this.offset = hips.InverseTransformPoint(pivot.position);
            this.upperLength = (pivot.position - knee.position).magnitude;
            this.lowerLength = (knee.position - ankle.position).magnitude;
        }
    }

    public class LowerBodyParametrisation : MonoBehaviour
    {
        public float angleOffset = 0f;
        public float inclinationOffset = 0f;

        public List<Transform> bones = new List<Transform>();

        private MotionMatchingController controller;

        public Transform hips;

        private Leg left;
        private Leg right;

        public LegPose LeftPose;
        public LegPose RightPose;

        private void Awake()
        {
            controller = GetComponent<MotionMatchingController>();
        }

        // Start is called before the first frame update
        void Start()
        {
            var transforms = new Dictionary<string, Transform>();

            if (controller)
            {

                var data = controller.MMData;
                var animationdata = data.AnimationDataTPose;
                var animation = animationdata.GetAnimation();
                var skeleton = animation.Skeleton;

                foreach (var transform in controller.GetSkeletonTransforms())
                {
                    transforms.Add(transform.name, transform);
                }
            }
            else
            {
                foreach (var item in bones)
                {
                    transforms.Add(item.name, item);
                }
            }

            if (!hips)
            {
                hips = transforms["Hips"];
            }

            left = new Leg(
                hips,
                transforms["LeftHip"],
                transforms["LeftKnee"],
                transforms["LeftAnkle"],
                transforms["LeftToe"]
            );

            right = new Leg(
                hips,
                transforms["RightHip"],
                transforms["RightKnee"],
                transforms["RightAnkle"],
                transforms["RightToe"]
            );
        }

        void Update()
        {
            UpdatePose(left, ref LeftPose);
            UpdatePose(right, ref RightPose);
        }

        private void UpdatePose(Leg leg, ref LegPose pose)
        {
            GetAnklePose(leg, ref pose.ankle); // Do this before getting the knee pose
            GetKneePose(leg, ref pose);
        }

        void GetAnklePose(Leg leg, ref PolarCoordinate parms)
        {
            var p = hips.InverseTransformPoint(leg.ankle.position) - leg.offset;

            parms.radius = p.magnitude / leg.length;

            Vector3 yz = new Vector3(0, p.y, p.z);
            yz.Normalize();
            parms.position = -Mathf.Atan2(yz.z, -yz.y) * Mathf.Rad2Deg;

            Vector3 xy = new Vector3(p.x, p.y, 0);
            xy.Normalize();
            parms.spread = Mathf.Atan2(xy.x, -xy.y) * Mathf.Rad2Deg;

            parms.position += angleOffset;
        }

        void GetKneePose(Leg leg, ref LegPose pose)
        {
            // Get the parameters for the knee in the same way as they'd be defined
            // when applying the transform at the other end.

            var ankle = hips.InverseTransformPoint(leg.ankle.position) - leg.offset;

            // Get the cirlcle describing the possible positions of the knee
            var kp = SphereSphereIntersection(Vector3.zero, ankle, leg.upperLength, leg.lowerLength);

            // knee plane origin & plane
            var o = (kp.normal * kp.d);
            var p = new Plane(kp.normal, o);

            // The hips forward vector transformed by the ankle orientation -
            // this will provide the reference vector for the knee rotation
            var forward = GetOrientation(pose.ankle) * Vector3.forward;

            // Get the reference vector in the plane
            forward = (p.ClosestPointOnPlane(o + forward) - o).normalized;

            // Get the knee position in the plane
            var knee = (hips.InverseTransformPoint(leg.knee.position) - leg.offset - o);

            DebugDraw(o + leg.offset, o + leg.offset + knee, Color.red);
            DebugDraw(o + leg.offset, o + leg.offset + forward * knee.magnitude, Color.green);

            pose.knee = 0;

            if (knee.magnitude > 0.001f)
            {
                var angle = Vector3.Dot(forward, knee.normalized);
                if (angle < 1 - Mathf.Epsilon)
                {
                    pose.knee = Mathf.Acos(angle) * Mathf.Rad2Deg * Mathf.Sign(Vector3.Dot(Vector3.Cross(forward, knee.normalized), -kp.normal));
                }
            }
        }

        private Quaternion GetOrientation(PolarCoordinate coord)
        {
            return Quaternion.AngleAxis(coord.position, Vector3.right) * Quaternion.AngleAxis(coord.spread, Vector3.forward);
        }

        private struct Circle
        {
            public Vector3 normal;
            public float d;
            public float radius;
        }

        private Circle SphereSphereIntersection(Vector3 A, Vector3 B, float R, float r)
        {
            // This is the problem of a sphere sphere intersection, which we solve
            // as a distance along the vector b-a, and radius of a circle normal
            // to the vector at that point.

            Circle circle;

            var AB = B - A;
            var d = AB.magnitude;
            var x = ((d * d) - (r * r) + (R * R)) / (2 * d);

            circle.normal = AB.normalized;
            circle.d = x;

            circle.radius = 0;

            var b = 4 * d * d * R * R - Mathf.Pow(d * d - r * r + R * R, 2);
            if (b > 0)
            {
                var a = (1 / (2 * d)) * Mathf.Sqrt(b);
                circle.radius = a;
            }

            return circle;
        }

        private void DebugDraw(Vector3 start, Vector3 end, Color color)
        {
            Debug.DrawLine(hips.TransformPoint(start), hips.TransformPoint(end), color);
        }

        private void OnDrawGizmos()
        {

        }
    }
}