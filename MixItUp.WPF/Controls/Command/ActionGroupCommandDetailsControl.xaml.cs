﻿using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for ActionGroupCommandDetailsControl.xaml
    /// </summary>
    public partial class ActionGroupCommandDetailsControl : CommandDetailsControlBase
    {
        private ActionGroupCommand command;

        public ActionGroupCommandDetailsControl(ActionGroupCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public ActionGroupCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
            }

            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("Name is missing");
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                if (this.command == null)
                {
                    this.command = new ActionGroupCommand(this.NameTextBox.Text);
                    ChannelSession.Settings.ActionGroupCommands.Add(this.command);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                }
                return this.command;
            }
            return null;
        }
    }
}
