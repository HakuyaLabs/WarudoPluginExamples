using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data.Models;
using Warudo.Core.Graphs;
using Warudo.Plugins.VMC.Assets;

namespace Warudo.Plugins.VMC.Nodes {
    [NodeType(Id = "5246edcb-310c-49f8-9934-a75df2078d23", Title = "GET_VMC_RECEIVER_DATA", Category = "CATEGORY_MOTION_CAPTURE")]
    public class GetVMCReceiverDataNode : Node {

        [DataInput]
        [Label("RECEIVER")]
        public VMCReceiverAsset Receiver;
        
        [DataOutput]
        [Label("IS_TRACKED")]
        public bool IsTracked() => Receiver?.IsTracked ?? false;

        [DataOutput]
        [Label("ROOT_TRANSFORM")]
        public TransformData RootTransform() => Receiver?.LatestRootTransform;
        
        [DataOutput]
        [Label("BONE_POSITIONS")]
        public Vector3[] BonePositions() => Receiver?.LatestBonePositions;
        
        [DataOutput]
        [Label("BONE_ROTATIONS")]
        public Quaternion[] BoneRotations() => Receiver?.LatestBoneRotations;

        [DataOutput]
        [Label("BLENDSHAPES")]
        public Dictionary<string, float> BlendShapes() {
            if (Receiver == null) return null;
            latestBlendShapes.Clear();
            foreach (var (key, value) in Receiver.LatestBlendShapes) {
                latestBlendShapes.Add(key, value);
            }
            // VSeeFace sends ARKit blendshapes even though they are not used; these blendshapes override the VRM ones (e.g., eyeBlinkLeft overrides Blink_L)
            // Check if all ARKit blendshapes are 0. If so, remove those from the dictionary
            if (vsfARKitBlendShapes == null) {
                vsfARKitBlendShapes = Core.Utils.BlendShapes.ARKitBlendShapeNames.Select(it => it[0].ToString().ToUpper() + it[1..]).ToArray();
            }
            if (vsfARKitBlendShapes.All(arkitBlendShape => latestBlendShapes.ContainsKey(arkitBlendShape) && latestBlendShapes[arkitBlendShape] == 0)) {
                foreach (var key in vsfARKitBlendShapes) {
                    latestBlendShapes.Remove(key);
                }
            }
            return latestBlendShapes;
        }

        private Dictionary<string, float> latestBlendShapes = new(100);
        private string[] vsfARKitBlendShapes;

    }
}
