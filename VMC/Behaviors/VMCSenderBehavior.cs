using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using uOSC;
using Warudo.Core.Utils;

namespace Warudo.Plugins.VMC.Behaviors {
    public class VMCSenderBehavior : MonoBehaviour {

        private static readonly Dictionary<HumanBodyBones, string> VMCTrackerBones = new() {
            { HumanBodyBones.Head, "Head" },
            { HumanBodyBones.Spine, "Spine" },
            { HumanBodyBones.Hips, "Waist" },
            { HumanBodyBones.LeftHand, "Left hand" },
            { HumanBodyBones.RightHand, "Right hand" },
            { HumanBodyBones.LeftFoot, "Left foot" },
            { HumanBodyBones.RightFoot, "Right foot" },
            { HumanBodyBones.LeftLowerArm, "Left elbow" },
            { HumanBodyBones.RightLowerArm, "Right elbow" },
            { HumanBodyBones.LeftLowerLeg, "Left knee" },
            { HumanBodyBones.RightLowerLeg, "Right knee" }
        };

        public Vector3 RootPosition { get; set; }
        public Quaternion RootRotation { get; set; }
        public Vector3[] BonePositions { get; } = new Vector3[(int)HumanBodyBones.LastBone];
        public Quaternion[] BoneRotations { get; } = new Quaternion[(int)HumanBodyBones.LastBone].FillWithIdentity();
        public Vector3[] BoneWorldPositions { get; } = new Vector3[(int)HumanBodyBones.LastBone];
        public Quaternion[] BoneWorldRotations { get; } = new Quaternion[(int)HumanBodyBones.LastBone].FillWithIdentity();
        public Dictionary<string, float> VRMBlendShapeProxyWeights { get; } = new(100);
        public bool SendingToVMC { get; set; } = false;

        private uOscClient client;

        private void Awake() {
            client = gameObject.GetOrAddComponent<uOscClient>();
        }

        public bool IsClientRunning() {
            return client.isRunning;
        }

        public void StartClient(string address, int port) {
            client.address = address;
            client.port = port;
            client.StartClient();
        }

        public void StopClient() {
            client.StopClient();
        }

        public void OnDestroy() {
            StopClient();
            Destroy(client);
        }

        private async void Update() {
            await UniTask.WaitForEndOfFrame(this);
            if (!client.isRunning) return;
            var bundle = new Bundle();
            bundle.Add(new Message("/VMC/Ext/T", Time.realtimeSinceStartup));
            bundle.Add(new Message("/VMC/Ext/Root/Pos", "root", RootPosition.x, RootPosition.y, RootPosition.z, RootRotation.x, RootRotation.y, RootRotation.z, RootRotation.w));
            for (var bone = HumanBodyBones.Hips; bone < HumanBodyBones.LastBone; bone++) {
                var i = (int)bone;

                var rot = BoneRotations[i];
                if (SendingToVMC && (bone == HumanBodyBones.LeftHand || bone == HumanBodyBones.RightHand)) {
                    // VMC bugfix
                    rot = BoneWorldRotations[i];
                }
                bundle.Add(new Message("/VMC/Ext/Bone/Pos", bone.ToString(), BonePositions[i].x, BonePositions[i].y, BonePositions[i].z, rot.x, rot.y, rot.z, rot.w));

                if (SendingToVMC) {
                    // VMC (the software, not protocol) compatibility: send tracker data, so VMC handles IK
                    if (VMCTrackerBones.ContainsKey(bone)) {
                        bundle.Add(new Message("/VMC/Ext/Tra/Pos", bone.ToString(), BoneWorldPositions[i].x, BoneWorldPositions[i].y, BoneWorldPositions[i].z, rot.x, rot.y, rot.z, rot.w));
                    }
                }
            }
            foreach (var (key, value) in VRMBlendShapeProxyWeights) {
                bundle.Add(new Message("/VMC/Ext/Blend/Val", key, value));
            }
            bundle.Add(new Message("/VMC/Ext/Blend/Apply"));
            bundle.Add(new Message("/VMC/Ext/OK", 1, 3, 0)); // Loaded, Calibrated, Normal
            client.Send(bundle);
        }
    }
}
