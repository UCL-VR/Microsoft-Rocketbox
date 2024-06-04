using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;

public class ThreePointRig : MonoBehaviour
{
    private Ubiq.Avatars.Avatar avatar;

    private struct State
    {
        public Vector3 headPosition;
        public Vector3 leftHandPosition;
        public Vector3 rightHandPosition;
        public Quaternion headRotation;
        public Quaternion leftHandRotation;
        public Quaternion rightHandRotation;
    }

    public Transform left;
    public Transform right;
    public Transform head;

    private State state;

    private void Awake()
    {
        avatar = GetComponent<Ubiq.Avatars.Avatar>();
    }

    // Start is called before the first frame update
    void Start()
    {
        var avatarManager = FindAnyObjectByType<AvatarManager>();
        avatar.SetHints(avatarManager.hints);
    }

    // Update is called once per frame
    void Update()
    {
        if (avatar.hints != null)
        {
            avatar.hints.TryGetVector3("RightHandPosition", out state.rightHandPosition);
            avatar.hints.TryGetVector3("LeftHandPosition", out state.leftHandPosition);
            avatar.hints.TryGetVector3("HeadPosition", out state.headPosition);
            avatar.hints.TryGetQuaternion("RightHandRotation", out state.rightHandRotation);
            avatar.hints.TryGetQuaternion("LeftHandRotation", out state.leftHandRotation);
            avatar.hints.TryGetQuaternion("HeadRotation", out state.headRotation);
        }

        left.position = state.leftHandPosition;
        left.rotation = state.leftHandRotation;
        right.position = state.rightHandPosition;
        right.rotation = state.rightHandRotation;
        head.position = state.headPosition;
        head.rotation = state.headRotation;
    }
}
