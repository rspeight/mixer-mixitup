﻿using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MixItUp.Base.Commands
{
    public enum CommandTypeEnum
    {
        Chat,
        Interactive,
        Event,
        Timer,
        Custom,
        ActionGroup,
        Game,
    }

    [DataContract]
    public abstract class CommandBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public List<string> Commands { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsBasic { get; set; }

        [XmlIgnore]
        public string TypeName { get { return EnumHelper.GetEnumName(this.Type); } }

        [XmlIgnore]
        private Task currentTaskRun;
        [XmlIgnore]
        private CancellationTokenSource currentCancellationTokenSource;

        public CommandBase()
        {
            this.ID = Guid.NewGuid();
            this.Commands = new List<string>();
            this.Actions = new List<ActionBase>();
            this.IsEnabled = true;
        }

        public CommandBase(string name, CommandTypeEnum type, string command) : this(name, type, new List<string>() { command }) { }

        public CommandBase(string name, CommandTypeEnum type, IEnumerable<string> commands)
            : this()
        {
            this.Name = name;
            this.Type = type;
            this.Commands.AddRange(commands);
        }

        public string CommandsString { get { return string.Join(" ", this.Commands); } }

        public bool ContainsCommand(string command) { return this.Commands.Contains(command); }

        public async Task Perform() { await this.Perform(null); }

        public async Task Perform(IEnumerable<string> arguments) { await this.Perform(ChannelSession.GetCurrentUser(), arguments); }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (this.IsEnabled)
            {
                await this.PerformInternal(user, arguments);
            }
        }

        public async Task PerformAndWait(UserViewModel user, IEnumerable<string> arguments = null)
        {
            await this.Perform(user, arguments);
            if (this.currentTaskRun != null && !this.currentTaskRun.IsCompleted)
            {
                await this.currentTaskRun;
            }
        }

        public virtual Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (arguments == null)
            {
                arguments = new List<string>();
            }

            this.currentCancellationTokenSource = new CancellationTokenSource();
            this.currentTaskRun = Task.Run(async () =>
            {
                CancellationToken token = this.currentCancellationTokenSource.Token;
                try
                {
                    await this.AsyncSemaphore.WaitAsync();

                    for (int i = 0; i < this.Actions.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        if (this.Actions[i] is OverlayAction && ChannelSession.Services.OverlayServer != null)
                        {
                            ChannelSession.Services.OverlayServer.StartBatching();
                        }

                        await this.Actions[i].Perform(user, arguments);

                        if (this.Actions[i] is OverlayAction && ChannelSession.Services.OverlayServer != null)
                        {
                            if (i == (this.Actions.Count - 1) || !(this.Actions[i + 1] is OverlayAction))
                            {
                                await ChannelSession.Services.OverlayServer.EndBatching();
                            }
                        }
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex) { Logger.Log(ex); }
                finally { this.AsyncSemaphore.Release(); }
            }, this.currentCancellationTokenSource.Token);
            return Task.FromResult(0);
        }

        public void StopCurrentRun()
        {
            if (this.currentCancellationTokenSource != null)
            {
                this.currentCancellationTokenSource.Cancel();
            }
        }

        public void AddSpecialIdentifier(string specialIdentifier, string replacement)
        {
            foreach (ActionBase action in this.Actions)
            {
                action.AddSpecialIdentifier(specialIdentifier, replacement);
            }
        }

        protected abstract SemaphoreSlim AsyncSemaphore { get; }
    }
}
