using MotionMatching;
using System.Collections.Generic;
using UnityEngine;

namespace Ubiq.MotionMatching
{
    public class LowerBodyParametrisation : MonoBehaviour, IHipSpace
    {
        public float angleOffset = 0f;
        public float inclinationOffset = 0f;

        public List<Transform> bones = new List<Transform>();

        private MotionMatchingController controller;

        private Transform hips;
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

            hips = transforms["Hips"];

            left = new Leg(
                this,
                transforms["LeftHip"],
                transforms["LeftKnee"],
                transforms["LeftAnkle"],
                transforms["LeftToe"]
            );

            right = new Leg(
                this,
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

        /// <summary>
        /// Transforms from World Space into local Hip space, including any
        /// corrective transforms.
        /// </summary>
        public Vector3 InverseTransformPoint(Vector3 world)
        {
            return Quaternion.Euler(0,180,0) * hips.InverseTransformPoint(world);
        }

        public Vector3 TransformPoint(Vector3 local)
        {
            return  hips.TransformPoint(Quaternion.Inverse(Quaternion.Euler(0, 180, 0)) * local);
        }

        void GetAnklePose(Leg leg, ref PolarCoordinate parms)
        {
            var p = InverseTransformPoint(leg.ankle.position) - leg.offset;

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

            var ankle = InverseTransformPoint(leg.ankle.position) - leg.offset;

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
            var knee = InverseTransformPoint(leg.knee.position) - leg.offset - o;

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
            Debug.DrawLine(TransformPoint(start), TransformPoint(end), color);
        }

        private void OnDrawGizmos()
        {

        }
    }
}