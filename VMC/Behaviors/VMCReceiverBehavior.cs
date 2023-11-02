using System;
using System.Collections.Generic;
using UnityEngine;
using uOSC;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Utils;

namespace Warudo.Plugins.VMC.Behaviors {
    // Credits go to: https://github.com/gpsnmeajp/EasyVirtualMotionCaptureForUnity/blob/master/EVMC4U/ExternalReceiver.cs
    public class VMCReceiverBehavior : MonoBehaviour {

        public TransformData LatestRootTransform { get; } = StructuredData.Create<TransformData>();
        public Vector3[] LatestBonePositions { get; } = new Vector3[(int)HumanBodyBones.LastBone];
        public Quaternion[] LatestBoneRotations { get; } = new Quaternion[(int)HumanBodyBones.LastBone].FillWithIdentity();
        public Dictionary<string, float> LatestBlendShapes { get; } = new(100);

        public float LastReceivedTime { get; private set; }

        private uOscServer server;

        // Temporary variables to prevent GC.Alloc
        private Vector3 pos;
        private Quaternion rot;
        private Vector3 scale;
        private Vector3 offset;

        // Generate enum string to int map
        private static Dictionary<string, int> boneNameToIndex;

        private void Awake() {
            if (boneNameToIndex == null) {
                boneNameToIndex = new Dictionary<string, int>();
                foreach (var bone in Enum.GetValues(typeof(HumanBodyBones))) {
                    boneNameToIndex.Add(bone.ToString(), (int)bone);
                }
            }
            server = gameObject.GetOrAddComponent<uOscServer>();
            server.autoStart = false;
        }

        public bool IsServerRunning() {
            return server.isRunning;
        }

        public void StartServer(int port) {
            server.port = port;
            server.StartServer();
            server.onDataReceived.AddListener(OnDataReceived);
        }

        public void StopServer() {
            server.StopServer();
        }

        public void OnDestroy() {
            server.StopServer();
            Destroy(server);
        }

        public void OnDataReceived(Message message) {
            ProcessMessage(ref message);
            LastReceivedTime = Time.realtimeSinceStartup;
        }

        public void ProcessMessage(ref Message message) {
            if (message.address == null || message.values == null) {
                return;
            }

            if (message.address == "/VMC/Ext/Root/Pos"
                && message.values.Length >= 8
                && message.values[0] is string
                && message.values[1] is float
                && message.values[2] is float
                && message.values[3] is float
                && message.values[4] is float
                && message.values[5] is float
                && message.values[6] is float
                && message.values[7] is float
               ) {
                pos.x = (float)message.values[1];
                pos.y = (float)message.values[2];
                pos.z = (float)message.values[3];
                rot.x = (float)message.values[4];
                rot.y = (float)message.values[5];
                rot.z = (float)message.values[6];
                rot.w = (float)message.values[7];

                LatestRootTransform.Position = pos;
                LatestRootTransform.Rotation = rot.eulerAngles;

                if (message.values.Length >= 14
                    && message.values[8] is float
                    && message.values[9] is float
                    && message.values[10] is float
                    && message.values[11] is float
                    && message.values[12] is float
                    && message.values[13] is float
                   ) {
                    scale.x = 1.0f / (float)message.values[8];
                    scale.y = 1.0f / (float)message.values[9];
                    scale.z = 1.0f / (float)message.values[10];
                    offset.x = (float)message.values[11];
                    offset.y = (float)message.values[12];
                    offset.z = (float)message.values[13];

                    LatestRootTransform.Scale = scale;
                    LatestRootTransform.Position = Vector3.Scale(pos, scale);
                    offset = Vector3.Scale(offset, scale);
                    LatestRootTransform.Position -= offset;
                } else {
                    LatestRootTransform.Scale = Vector3.one;
                }
            } else if (message.address == "/VMC/Ext/Bone/Pos"
                       && message.values.Length >= 8
                       && message.values[0] is string
                       && message.values[1] is float
                       && message.values[2] is float
                       && message.values[3] is float
                       && message.values[4] is float
                       && message.values[5] is float
                       && message.values[6] is float
                       && message.values[7] is float
                      ) {
                var boneName = (string)message.values[0];
                pos.x = (float)message.values[1];
                pos.y = (float)message.values[2];
                pos.z = (float)message.values[3];
                rot.x = (float)message.values[4];
                rot.y = (float)message.values[5];
                rot.z = (float)message.values[6];
                rot.w = (float)message.values[7];

                if (!boneNameToIndex.ContainsKey(boneName)) {
                    return;
                }
                var index = boneNameToIndex[boneName];
                if (index < 0 || index >= LatestBonePositions.Length) {
                    return;
                }
                
                LatestBonePositions[index] = pos;
                LatestBoneRotations[index] = rot;

                LatestBoneRotations[index] = Quaternion.Euler(rot.WrappedEulerAngles());
            } else if (message.address == "/VMC/Ext/Blend/Val"
                       && message.values.Length >= 2
                       && message.values[0] is string
                       && message.values[1] is float
                      ) {
                var key = (string)message.values[0];
                var value = (float)message.values[1];
                LatestBlendShapes[key] = value;
            } else if (message.address == "/VMC/Ext/Blend/Apply") {
                // By VMC protocol, BlendShapes should only be applied after /Apply is sent.
                // But is this really necessary? I don't know.
            }
        }
    }
}
