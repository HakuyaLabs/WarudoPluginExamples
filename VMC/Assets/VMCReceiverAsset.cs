using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Localization;
using Warudo.Core.Utils;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets.MotionCapture;
using Warudo.Plugins.Core.Utils;
using Warudo.Plugins.VMC.Behaviors;
using Object = UnityEngine.Object;
// using System.Collections.Generic;
// using System.IO;
// using Warudo.Core.Graphs;
// using uOSC;
// using Newtonsoft.Json;
// using Playground;

namespace Warudo.Plugins.VMC.Assets {
    [AssetType(Id = "78fac3fd-88f9-492d-96de-4bd6605cdf43", Title = "VMC_RECEIVER", Category = "CATEGORY_MOTION_CAPTURE")]
    public class VMCReceiverAsset : GenericTrackerAsset {

        protected override bool UseHeadIK => false;

        protected override bool UseCharacterDaemon => false;
        
        protected override bool CanCalibrate => false;
        
        public override List<string> InputBlendShapes => BlendShapes.ARKitBlendShapeNames.ToList().Also(it => {
            it.AddRange(new [] {
                "A",
                "I",
                "U",
                "E",
                "O",
                "Blink_L",
                "Blink_R",
                "Blink",
                "LookUp",
                "LookDown",
                "LookLeft",
                "LookRight",
            });
        });

        [Markdown(order: -5000)]
        public string Status = "RECEIVER_NOT_STARTED".Localized();

        [DataInput(order: -999)]
        [Label("PORT")]
        public int Port = 39539;

        private VMCReceiverBehavior receiver;

        protected override void OnCreate() {
            base.OnCreate();
            var gameObject = new GameObject("VMC Receiver");
            receiver = gameObject.AddComponent<VMCReceiverBehavior>();
            Watch(nameof(Port), ResetReceiver);
            ResetReceiver();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (receiver != null) {
                Object.Destroy(receiver.gameObject);
                receiver = null;
            }
        }

        public async void ResetReceiver() {
            IsStarted = false;
            SetActive(false);
            
            receiver.StopServer();
            try {
                await Context.PluginManager.GetPlugin<CorePlugin>().BeforeListenToPort();
                receiver.StartServer(Port);
                Context.PluginManager.GetPlugin<CorePlugin>().AfterListenToPort();
                
                IsStarted = true;
                Status = "RECEIVER_STARTED_ON_PORT".Localized(Port, string.Join(", ", Networking.GetLocalIPAddresses().Select(it => it.ToString())));
                SetActive(true);
            } catch (Exception e) {
                Log.UserError("Failed to start VMC receiver on port " + Port, e);
                Status = "FAILED_TO_START_RECEIVER_ANOTHER_PROCESS_IS_ALREADY_LISTENING_ON_THIS_PORT".Localized(0) + "\n\n" + e.PrettyPrint();  
            }
            BroadcastDataInput(nameof(Status));
        }

        protected override bool UpdateRawData() {
            if (receiver == null || Time.realtimeSinceStartup - receiver.LastReceivedTime > 0.5f) return false;
            
            RawBlendShapes.Clear();
            RawBlendShapes.CopyFrom(receiver.LatestBlendShapes);

            for (var i = 0; i < (int)HumanBodyBones.LastBone; i++) {
                RawBoneRotations[i] = receiver.LatestBoneRotations[i];
                RawBonePositions[i] = receiver.LatestBonePositions[i];
            }

            RawRootTransform.CopyFrom(receiver.LatestRootTransform);

            return true;
        }
        
        // [Trigger]
        // public void StartTesting() {
        //     testData = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("C:\\temp\\bones\\ExpressionIssue.json"));
        //     testPointer = 0;
        // }
        //
        // [Trigger]
        // public void StopTesting() {
        //     testData = null;
        // }
        //
        // public override void OnUpdate() {
        //     base.OnUpdate();
        //     if (testData == null) return;
        //     for (var i = 0; i < 300; i++) {
        //         testPointer = (testPointer + 1) % testData.Count;
        //         var data = testData[testPointer];
        //         var arr = data.Split("\t");
        //         var address = arr[0];
        //         var args = arr[1].Split(" ")[0..^2];
        //         object[] o;
        //         if (address == "/VMC/Ext/Root/Pos") {
        //             o = new object[] {
        //                 "root", float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]), float.Parse(args[7]),
        //             };
        //         } else if (address == "/VMC/Ext/Bone/Pos") {
        //             o = new object[] {
        //                 args[0], float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]), float.Parse(args[7]),
        //             };
        //         } else if (address == "/VMC/Ext/Blend/Val") {
        //             o = new object[] {
        //                 args[0], float.Parse(args[1]),
        //             };
        //         } else {
        //             Debug.Log("Unknown address: " + address);
        //             continue;
        //         }
        //         receiver.OnDataReceived(new Message(address, o));
        //     }
        // }
        //
        // private List<string> testData;
        // private int testPointer = 0;

    }
}
