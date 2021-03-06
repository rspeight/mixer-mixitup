﻿using AutoUpdaterDotNET;
using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using MixItUp.WPF.Windows.Wizard;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MixItUp.WPF
{
    public class StreamerLoginItem
    {
        public IChannelSettings Setting;

        public StreamerLoginItem(IChannelSettings setting)
        {
            this.Setting = setting;
        }

        public string Name { get { return this.Setting.Channel.user.username; } }
    }

    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : LoadingWindowBase
    {
        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            List<IChannelSettings> settings = new List<IChannelSettings>(await ChannelSession.Services.Settings.GetAllSettings());
            settings = settings.Where(s => s.IsStreamer).ToList();
            if (settings.Count() > 0)
            {
                this.ExistingStreamerComboBox.Visibility = Visibility.Visible;
                settings.Add(new DesktopChannelSettings() { Channel = new ExpandedChannelModel() { id = 0, user = new UserModel() { username = "NEW STREAMER" } } });
                this.ExistingStreamerComboBox.ItemsSource = settings.Select(cs => new StreamerLoginItem(cs));
                if (settings.Count() == 2)
                {
                    this.ExistingStreamerComboBox.SelectedIndex = 0;
                }
            }

            await base.OnLoaded();

            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
            AutoUpdater.Start("https://updates.mixitupapp.com/AutoUpdater.xml");
        }

        private async void StreamerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;

            await this.RunAsyncOperation(async () =>
            {
                if (this.ExistingStreamerComboBox.Visibility == Visibility.Visible)
                {
                    if (this.ExistingStreamerComboBox.SelectedIndex >= 0)
                    {
                        StreamerLoginItem loginItem = (StreamerLoginItem)this.ExistingStreamerComboBox.SelectedItem;
                        IChannelSettings setting = loginItem.Setting;
                        if (setting.Channel.id == 0)
                        {
                            result = await this.NewStreamerLogin();
                        }
                        else
                        {
                            result = await ChannelSession.ConnectUser(setting);
                            if (result)
                            {
                                if (!await ChannelSession.ConnectBot(setting))
                                {
                                    await MessageBoxHelper.ShowMessageDialog("Bot Account failed to authenticate, please re-connect it from the Services section.");
                                }

                                MainWindow window = new MainWindow();
                                this.Hide();
                                window.Show();
                                this.Close();
                            }
                        }
                    }
                    else
                    {
                        await MessageBoxHelper.ShowMessageDialog("You must select a Streamer account to log in to");
                    }
                }
                else
                {
                    result = await this.NewStreamerLogin();
                }
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Mixer, please try again");
            }
        }

        private void ModeratorChannelTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ModeratorLoginButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void ModeratorLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ModeratorChannelTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A channel name must be entered");
                return;
            }

            if (await this.EstablishConnection(ChannelSession.ModeratorScopes, this.ModeratorChannelTextBox.Text))
            {
                IEnumerable<UserWithGroupsModel> users = await ChannelSession.Connection.GetUsersWithRoles(ChannelSession.Channel, UserRole.Mod);
                if (users.Any(uwg => uwg.id.Equals(ChannelSession.User.id)))
                {
                    MainWindow window = new MainWindow();
                    this.Hide();
                    window.Show();
                    this.Close();
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("You are not a moderator for this channel.");
                }
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Mixer, please try again");
            }
        }

        private async Task<bool> NewStreamerLogin()
        {
            if (await this.EstablishConnection(ChannelSession.StreamerScopes, channelName: null))
            {
                NewUserWizardWindow window = new NewUserWizardWindow();
                window.Show();
                this.Close();
                return true;
            }
            return false;
        }

        private async Task<bool> EstablishConnection(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            return await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConnectUser(scopes, channelName);
            });
        }

        private void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null && args.IsUpdateAvailable)
            {
                UpdateWindow window = new UpdateWindow(args);
                window.Show();
            }
        }
    }
}
