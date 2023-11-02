using System;
using System.Collections.Generic;
using Warudo.Core.Graphs;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Nodes;
using Warudo.Plugins.VMC.Assets;
using Warudo.Plugins.VMC.Nodes;
using Warudo.Bootstrap.Templates;

namespace Warudo.Plugins.VMC {
    public class VMCFaceTrackingTemplate : FaceTrackingTemplate {
        
        public override Guid Id => Guid.Parse("7d732ed8-4967-4889-9c2b-8b3599e623c8");
        
        public override CharacterTrackingTemplateType Type => CharacterTrackingTemplateType.Face;

        public override string Name => "VMC";

        public override string AutoCompleteName => "VMC_(EXTERNAL_APPLICATION)";

        protected override IBlendShapeMapper CreateBlendShapeMapper() => new IdentityBlendShapeMapper();
        
        protected override List<string> BodyPartsToOffsetRotations => new() {
            nameof(MergeCharacterBoneRotationListNode.Face),
            //nameof(MergeCharacterBoneRotationListNode.Head),
            //nameof(MergeCharacterBoneRotationListNode.Pelvis)
        };

        public override List<Type> AssetDependencyTypes => new() { typeof(VMCReceiverAsset) };
        
        protected override CreateReceiverResult CreateReceiver(Scene scene, CharacterAsset character, Graph graph) {
            var receiverAsset = GetAssetDependency<VMCReceiverAsset>(scene);
            receiverAsset.SetDataInput(nameof(receiverAsset.Character), character);
            var getReceiverDataNode = graph.AddNode<GetVMCReceiverDataNode>();
            getReceiverDataNode.SetDataInput(nameof(getReceiverDataNode.Receiver), receiverAsset);
            
            return new CreateReceiverResult {
                ReceiverAssets = new List<Asset> {
                    receiverAsset
                },
                ReceiverNode = getReceiverDataNode,
                IsTrackedPort = nameof(getReceiverDataNode.IsTracked),
                BlendShapesPort = nameof(getReceiverDataNode.BlendShapes),
                RootPositionPort = null,
                BoneRotationOffsetsPort = nameof(getReceiverDataNode.BoneRotations)
            };
        }

    }
}
