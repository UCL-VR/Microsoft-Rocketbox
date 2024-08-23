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

        float GetTPoseHeight(string name, Skeleton skeleton)
        {
            var height = 0f;
            Skeleton.Joint j;
            skeleton.Find(name, out j);
            do
            {
                height += j.LocalOffset.y;
                j = skeleton.Joints[j.ParentIndex];
            } while (j.ParentIndex != j.Index);
            return -height;
        }

        /// <summary>
        /// Updates parms with the position (in local hip space) as a polar coordinate
        /// </summary>
        void GetPolarPosition(Leg leg, Vector3 position, ref PolarCoordinate parms)
        {
            parms.radius = (leg.offset - position).magnitude / leg.length;
            position.x = 0;
            position.Normalize();
            parms.position = Mathf.Atan2(position.z, -position.y) * Mathf.Rad2Deg;
        }

        void GetAnklePosition(Leg leg, ref PolarCoordinate parms)
        {
            var local = hips.InverseTransformPoint(leg.ankle.position);
            GetPolarPosition(leg, local, ref parms);
            parms.position += angleOffset;
        }

        void Update()
        {
            GetAnklePosition(left, ref LeftPose.ankle);
            GetAnklePosition(right, ref RightPose.ankle);
        }

        private void OnDrawGizmos()
        {

        }
    }
}