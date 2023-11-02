using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Plugins;
using Warudo.Plugins.Core;
using Warudo.Plugins.VMC.Assets;
using Warudo.Plugins.VMC.Nodes;

namespace Warudo.Plugins.VMC {
    [PluginType(
        Id = "Warudo.VMC",
        Name = "VMC",
        Description = "VMC_DESCRIPTION",
        Author = "Hakuya Labs",
        Version = "WARUDO_VERSION",
        AssetTypes = new[] {
            typeof(VMCReceiverAsset),
            typeof(VMCSenderAsset)
        },
        NodeTypes = new[] {
            typeof(GetVMCReceiverDataNode)
        })]
    public class VMCPlugin : Plugin {

        protected override void OnCreate() {
            var corePlugin = Context.PluginManager.GetPlugin<CorePlugin>();
            corePlugin.RegisterCharacterTrackingTemplate<VMCFaceTrackingTemplate>(this);
            corePlugin.RegisterCharacterTrackingTemplate<VMCPoseTrackingTemplate>(this);
        }

    }
}
