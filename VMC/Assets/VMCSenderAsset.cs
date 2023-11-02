using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Localization;
using Warudo.Core.Scenes;
using Warudo.Core.Utils;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.VMC.Behaviors;

namespace Warudo.Plugins.VMC.Assets {
    [AssetType(Id = "cd797a17-4ed1-4ed0-b483-9069e3d1a942", Title = "VMC_SENDER", Category = "CATEGORY_EXTERNAL_INTEGRATION")]
    public class VMCSenderAsset : Asset {
        
        [Markdown]
        public string Status = "NOT_SENDING_DATA".Localized();
        
        [DataInput]
        [Label("IP_ADDRESS")]
        public string IPAddress = "127.0.0.1";

        [DataInput]
        [Label("PORT")]
        public int Port = 39540;

        [DataInput]
        [Label("CHARACTER")]
        public CharacterAsset Character;

        [DataInput]
        [Label("SENDING_TO_VIRTUALMOTIONCAPTURE")]
        public bool SendingToVMC = false;
        
        [DataInput]
        [HiddenIf(nameof(HideCalibratingVMC))]
        [Label("CALIBRATING_(VIRTUALMOTIONCAPTURE)")]
        public bool CalibratingVMC = false;

        protected bool HideCalibratingVMC() => !SendingToVMC;

        public VMCSenderBehavior Sender { get; private set; }

        protected override void OnCreate() {
            var gameObject = new GameObject("VMC Sender");
            Sender = gameObject.AddComponent<VMCSenderBehavior>();
            WatchAll(new[]{nameof(IPAddress), nameof(Port)}, ResetSender);
            ResetSender();
        }
        
        protected override void OnDestroy() {
            if (Sender != null) {
                Object.Destroy(Sender.gameObject);
                Sender = null;
            }
        }

        public void ResetSender() {
            SetActive(false);
            Sender.StopClient();
            Sender.StartClient(IPAddress, Port);
            Status = "SENDER_STARTED".Localized(IPAddress, Port);
            SetActive(true);
            BroadcastDataInput(nameof(Status));
        }

        public override void OnPostLateUpdate() {
            if (!Active) return;
            if (Character.IsNullOrInactive()) return;

            Sender.SendingToVMC = SendingToVMC;
            if (CalibratingVMC) {
                Sender.RootPosition = Vector3.zero;
                Sender.RootRotation = Quaternion.identity;
                for (var i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                    Sender.BonePositions[i] = Character.InitialBoneLocalPositions[i];
                    Sender.BoneWorldPositions[i] = Character.InitialBoneWorldPositions[i];
                    Sender.BoneRotations[i] = Quaternion.identity;
                    Sender.BoneWorldRotations[i] = Quaternion.identity;
                    if (i == (int)HumanBodyBones.LeftHand) {
                        Sender.BoneRotations[i] *= Quaternion.AngleAxis(-90f, Vector3.right);
                        Sender.BoneWorldRotations[i] *= Quaternion.AngleAxis(-90f, Vector3.right);
                    } else if (i == (int)HumanBodyBones.RightHand) {
                        Sender.BoneRotations[i] *= Quaternion.AngleAxis(-90f, Vector3.right);
                        Sender.BoneWorldRotations[i] *= Quaternion.AngleAxis(-90f, Vector3.right);
                    }
                }
                Sender.VRMBlendShapeProxyWeights.Clear();
                for (var i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                    var bt = Character.HumanBodyBoneToBodyTransforms[(HumanBodyBones)i];
                    if (bt == null) continue;
                    bt.localRotation = Sender.BoneRotations[i];
                    bt.localPosition = Sender.BonePositions[i];
                }
            } else {
                Sender.RootPosition = Character.MainTransform.position;
                Sender.RootRotation = Character.MainTransform.rotation;
                for (var i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                    Sender.BonePositions[i] = Character.EndOfFrameBonePositions[i];
                    Sender.BoneRotations[i] = Character.EndOfFrameBoneRotations[i];
                    Sender.BoneWorldPositions[i] = Character.AnimationJob.FinalBoneWorldPositions[i];
                    Sender.BoneWorldRotations[i] = Character.AnimationJob.FinalBoneWorldRotations[i];
                }
                Sender.VRMBlendShapeProxyWeights.Clear();
                foreach (var (key, value) in Character.LastVRMBlendShapeProxyWeights) {
                    Sender.VRMBlendShapeProxyWeights[key] = value;
                }
                foreach (var expression in Character.Expressions) {
                    Sender.VRMBlendShapeProxyWeights[expression.Name] = 0;
                }
                foreach (var expressionLayer in Character.ActiveExpressionLayers) {
                    foreach (var (_, state) in expressionLayer.states) {
                        if (state.isValid) Sender.VRMBlendShapeProxyWeights[state.expression.Name] = 1;
                    }
                }
            }
        }

    }
}
