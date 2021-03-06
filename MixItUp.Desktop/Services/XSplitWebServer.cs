﻿using Mixer.Base.Model.Client;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MixItUp.XSplit
{
    public class XSplitPacket : WebSocketPacket
    {
        public JObject data;

        public XSplitPacket(string type, JObject data)
        {
            this.type = type;
            this.data = data;
        }
    }

    public class XSplitWebServer : WebSocketServerBase, IXSplitService
    {
        public XSplitWebServer(string address) : base(address) { }

        public async Task SetCurrentScene(XSplitScene scene) { await this.Send(new XSplitPacket("sceneTransition", JObject.FromObject(scene))); }

        public async Task SetSourceVisibility(XSplitSource source) { await this.Send(new XSplitPacket("sourceUpdate", JObject.FromObject(source))); }

        public async Task SetWebBrowserSource(XSplitWebBrowserSource source) { await this.Send(new XSplitPacket("sourceUpdate", JObject.FromObject(source))); }

        protected override async Task PacketReceived(string packet)
        {
            await base.PacketReceived(packet);
        }
    }
}
