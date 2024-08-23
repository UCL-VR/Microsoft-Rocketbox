using UnityEngine;

namespace Ubiq.MotionMatching
{
    public class UbiqMotionMatching : MonoBehaviour, IHipSpace
    {
        public bool UpdateRootTransform = true;
        public bool UpdateLegTransforms = true;

        public LowerBodyParametrisation LowerBodyParameters;

        public LegPose LeftPose;
        public LegPose RightPose;

        private Animator animator;

        private Transform hips;
        private Leg left;
        private Leg right;

        private Quaternion HipsToLocal => Quaternion.Euler(-90, 0, 90);
        private Quaternion LocalToHips => Quaternion.Inverse(HipsToLocal);
        private Quaternion LegToWorld => Quaternion.Euler(0, 90, 0);

        // Start is called before the first frame update
        void Start()
        {
            InitialiseBindPose();
        }

        public Vector3 InverseTransformPoint(Vector3 world)
        {
            return LocalToHips * hips.InverseTransformPoint(world);
        }

        void InitialiseBindPose()
        {
            var skm = GetComponentInChildren<SkinnedMeshRenderer>();
            animator = GetComponent<Animator>();
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);

            left = new Leg(
                this,
                animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg),
                animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg),
                animator.GetBoneTransform(HumanBodyBones.LeftFoot),
                animator.GetBoneTransform(HumanBodyBones.LeftToes)
            );

            right = new Leg(
                this,
                animator.GetBoneTransform(HumanBodyBones.RightUpperLeg),
                animator.GetBoneTransform(HumanBodyBones.RightLowerLeg),
                animator.GetBoneTransform(HumanBodyBones.RightFoot),
                animator.GetBoneTransform(HumanBodyBones.RightToes)
            );
        }

        // Update is called once per frame
        void Update()
        {
            if (UpdateRootTransform)
            {
                transform.position = LowerBodyParameters.transform.position;
                transform.rotation = LowerBodyParameters.transform.rotation;
            }

            if (UpdateLegTransforms)
            {
                LeftPose = LowerBodyParameters.LeftPose;
                RightPose = LowerBodyParameters.RightPose;
            }

            ApplyTransforms(right, RightPose);
            ApplyTransforms(left, LeftPose);
        }

        void ApplyTransforms(Leg leg, LegPose pose)
        {
            var ankle = GetAnklePosition(leg, pose.ankle);

            // Transforms are always applied as rotations

            // This rotation goes between the hips rotation frame, and the pivot
            // rotation frame.

            // Get the direction of the upper leg in hip-space
            var knee = GetKneePosition(leg, pose);

            // Move relative to the pivot point
            knee = knee - leg.offset;
            ankle = ankle - leg.offset;

            // The up vector of the upper leg at the hips
            var up1 = Vector3.Cross(knee.normalized, Vector3.right);

            // The up vector of the upper leg at the knee
            var up2 = Quaternion.AngleAxis(-90, knee.normalized) * Vector3.Cross(ankle.normalized, knee.normalized).normalized;

            // Depending on the skinnng, we can apply the roll to the upper leg
            // or at the knee.
            // (It should be applied at the knee if a linear blend is performed
            // in the shader. If not, it should be applied at the pivot.)

            // This version applies it to the pivot.

            ApplyRotation(Quaternion.LookRotation(knee, up2), leg.pivot, LegToWorld);

            // Next, update the knee rotation. Get the position of the ankle relative to the knee.

            var ak = ankle - knee;

            // Even when there is a linear blend along the upper leg, the up
            // vector describing the start of the lower leg is always the same.
            // This is the up vector at the end of the upper leg, rotated by
            // the actual knee joint rotation.

            // We can use the dot product here because the knee should never rotate the other way

            var kneeHingeAxis = Quaternion.AngleAxis(-90, knee.normalized) * up2;
            var kneeAngle = Mathf.Acos(Vector3.Dot(knee.normalized, ak.normalized));
            var kneeUp = Quaternion.AngleAxis(kneeAngle * Mathf.Rad2Deg, kneeHingeAxis) * up2;

            ApplyRotation(Quaternion.LookRotation(ak, kneeUp), leg.knee, LegToWorld);
        }

        /// <summary>
        /// Applies a rotation defined in hip-space to the skeletal rig in world
        /// space, considering any rotational offsets that should be applied
        /// due to the rigging.
        /// </summary>
        private void ApplyRotation(Quaternion rotation, Transform bone, Quaternion hipsToRigWorld)
        {
            // Apply any corrective rotation as per the original rig - this is
            // done in hip space.
            rotation = rotation * hipsToRigWorld;

            // Counteract the rotation that will be applied by the hips
            rotation = HipsToLocal * rotation;

            // Apply the hips rotation to get in world space
            rotation = hips.rotation * rotation;

            // Finally apply the rotation to the scene graph
            bone.rotation = rotation;
        }

        private Quaternion GetOrientation(PolarCoordinate coord)
        {
            return Quaternion.AngleAxis(coord.position, Vector3.right) * Quaternion.AngleAxis(coord.spread, Vector3.forward);
        }

        /// <summary>
        /// Resolves a Polar Coordinate to a Position in Hip Space
        /// </summary>
        private Vector3 GetAnklePosition(Leg leg, PolarCoordinate coord)
        {
            return leg.offset + GetOrientation(coord) * Vector3.down * coord.radius * leg.length;
        }

        private struct KneePlane
        {
            public Plane p;
            public Vector3 o;
            public Vector3 normal;
            public Vector3 up; // The reference vector
        }

        private KneePlane GetKneePlane(Leg leg, LegPose pose)
        {
            var ankle = GetAnklePosition(leg, pose.ankle);

            // Get the cirlcle describing the possible positions of the knee
            var kp = KneeIntersection(leg, ankle);

            // knee plane origin & plane
            var o = leg.offset + (kp.normal * kp.d);
            var p = new Plane(kp.normal, o);

            // The hips forward vector transformed by the ankle orientation -
            // this will provide the reference vector for the knee rotation
            var forward = GetOrientation(pose.ankle) * Vector3.forward;

            // Get the reference vector in the plane
            forward = (p.ClosestPointOnPlane(o + forward) - o).normalized * kp.radius;

            return new KneePlane()
            {
                o = o,
                normal = kp.normal,
                up = forward,
                p = p
            };
        }

        private Vector3 GetKneePosition(Leg leg, LegPose pose)
        {
            var s = GetKneePlane(leg, pose);
            return s.o + Quaternion.AngleAxis(-pose.knee, s.normal) * s.up;
        }

        private struct Circle
        {
            public Vector3 normal;
            public float d;
            public float radius;
        }

        private Circle KneeIntersection(Leg leg, Vector3 ankle)
        {
            return SphereSphereIntersection(leg.offset, ankle, leg.upperLength, leg.lowerLength);
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            if (!Application.isPlaying)
            {
                InitialiseBindPose();
            }

            Gizmos.matrix = hips.localToWorldMatrix * Matrix4x4.Rotate(HipsToLocal);
            
            //Gizmos.matrix = Matrix4x4.TRS(hips.position, Quaternion.identity, Vector3.one);

            var ankle = GetAnklePosition(right, RightPose.ankle);

            Gizmos.DrawWireSphere(Vector3.zero, 0.01f);
            Gizmos.DrawWireSphere(right.offset, 0.01f);
            Gizmos.DrawWireSphere(ankle, 0.01f);

            // Draw the knee intersection circle

            var c = KneeIntersection(right, ankle);
            var q = Quaternion.FromToRotation(Vector3.forward, c.normal);
            var v0 = Vector3.zero;

            for(var i = 0f; i <= Mathf.PI * 2; i += (Mathf.PI / 10f))
            {
                var x = Mathf.Cos(i) * c.radius;
                var y = Mathf.Sin(i) * c.radius;
                var z = c.d;

                var v = new Vector3(x, y, z);

                v = right.offset + (q * v);

                if (i > 0)
                {
                    Gizmos.DrawLine(v0, v);
                }
                v0 = v;
            }

            var s = GetKneePlane(right, RightPose);
            Gizmos.DrawLine(s.o, s.o + s.up);

            var kp = GetKneePosition(right, RightPose);
            Gizmos.DrawLine(right.offset, kp);
        }
    }
}