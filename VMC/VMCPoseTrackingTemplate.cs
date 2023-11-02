using System;
using System.Collections.Generic;
using Warudo.Core.Graphs;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.VMC.Assets;
using Warudo.Plugins.VMC.Nodes;
using Warudo.Bootstrap.Templates;

namespace Warudo.Plugins.VMC {
    public class VMCPoseTrackingTemplate : GenericPoseTrackingTemplate {
        
        public override Guid Id => Guid.Parse("cd55a71a-f107-4931-9802-11a0df660ef8");

        public override CharacterTrackingTemplateType Type => CharacterTrackingTemplateType.Pose;
        
        public override string Name => "VMC";

        public override string AutoCompleteName => "VMC_(EXTERNAL_APPLICATION)";
        
        public override List<Type> AssetDependencyTypes => new() { typeof(VMCReceiverAsset) };
        
        protected override float SmoothTime => 0.2f;

        protected override CreateReceiverResult CreateReceiver(Scene scene, CharacterAsset character, Graph graph) {
            var receiverAsset = GetAssetDependency<VMCReceiverAsset>(scene);
            receiverAsset.SetDataInput(nameof(receiverAsset.Character), character);
            var getVMCReceiverDataNode = graph.AddNode<GetVMCReceiverDataNode>();
            getVMCReceiverDataNode.SetDataInput(nameof(getVMCReceiverDataNode.Receiver), receiverAsset);
            return new CreateReceiverResult {
                ReceiverAssets = new List<Asset> {
                    receiverAsset
                },
                ReceiverNode = getVMCReceiverDataNode,
                IsTrackedPort = nameof(getVMCReceiverDataNode.IsTracked),
                BoneRotationsPort = nameof(getVMCReceiverDataNode.BoneRotations),
                BonePositionsPort = nameof(getVMCReceiverDataNode.BonePositions),
                OffsetTransformPort = nameof(getVMCReceiverDataNode.RootTransform)
            };
        }

    }
}
