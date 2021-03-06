﻿using Mixer.Base;
using Mixer.Base.Interactive;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class MixerConnectionWrapper : MixerRequestWrapperBase
    {
        public MixerConnection Connection { get; private set; }

        public MixerConnectionWrapper(MixerConnection connection)
        {
            this.Connection = connection;
        }

        public void Initialize()
        {
            if (ChannelSession.Settings.DiagnosticLogging)
            {
                this.Connection.Channels.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Channels.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Channels.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Chats.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Chats.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Chats.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Costream.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Costream.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Costream.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.GameTypes.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.GameTypes.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.GameTypes.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Interactive.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Interactive.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Interactive.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.OAuth.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.OAuth.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.OAuth.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Teams.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Teams.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Teams.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;

                this.Connection.Users.OnRequestSent += RestAPIService_OnRequestSent;
                this.Connection.Users.OnSuccessResponseReceived += RestAPIService_OnSuccessResponseReceived;
                this.Connection.Users.OnFailureResponseReceived += RestAPIServices_OnFailureResponseReceived;
            }
        }

        public async Task<UserModel> GetUser(string username) { return await this.RunAsync(this.Connection.Users.GetUser(username)); }

        public async Task<UserModel> GetUser(uint userID) { return await this.RunAsync(this.Connection.Users.GetUser(userID)); }

        public async Task<UserWithChannelModel> GetUser(UserModel user) { return await this.RunAsync(this.Connection.Users.GetUser(user)); }

        public async Task<UserWithGroupsModel> GetUserInChannel(ChannelModel channel, uint userID) { return await this.RunAsync(this.Connection.Channels.GetUser(channel, userID)); }

        public async Task<IEnumerable<UserWithGroupsModel>> GetUsersWithRoles(ChannelModel channel, UserRole role) { return await this.RunAsync(this.Connection.Channels.GetUsersWithRoles(channel, role.ToString())); }

        public async Task<PrivatePopulatedUserModel> GetCurrentUser() { return await this.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<ChatUserModel> GetChatUser(ChannelModel channel, uint userID) { return await this.RunAsync(this.Connection.Chats.GetUser(channel, userID)); }

        public async Task<IEnumerable<ChatUserModel>> GetChatUsers(ChannelModel channel, uint maxResults = 1) { return await this.RunAsync(this.Connection.Chats.GetUsers(channel, maxResults)); }

        public async Task<ExpandedChannelModel> GetChannel(string name) { return await this.RunAsync(this.Connection.Channels.GetChannel(name)); }

        public async Task<IEnumerable<ExpandedChannelModel>> GetChannels(uint maxResults = 1) { return await this.RunAsync(this.Connection.Channels.GetChannels(maxResults)); }

        public async Task<ChannelModel> UpdateChannel(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.UpdateChannel(channel)); }

        public async Task<GameTypeModel> GetGameType(uint id) { return await this.RunAsync(this.Connection.GameTypes.GetGameType(id)); }

        public async Task<IEnumerable<GameTypeModel>> GetGameTypes(string name, uint maxResults = 1) { return await this.RunAsync(this.Connection.GameTypes.GetGameTypes(name, maxResults)); }

        public async Task<IEnumerable<ChannelModel>> GetChannelsByGameTypes(GameTypeSimpleModel gameType, uint maxResults = 1) { return await this.RunAsync(this.Connection.GameTypes.GetChannelsByGameType(gameType, maxResults)); }

        public async Task<DateTimeOffset?> CheckIfFollows(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.CheckIfFollows(ChannelSession.Channel, user)); }

        public async Task<Dictionary<UserModel, DateTimeOffset?>> CheckIfFollows(ChannelModel channel, IEnumerable<UserModel> users) { return await this.RunAsync(this.Connection.Channels.CheckIfFollows(ChannelSession.Channel, users)); }

        public async Task<IEnumerable<StreamSessionsAnalyticModel>> GetStreamSessions(ChannelModel channel, DateTimeOffset startTime) { return await this.RunAsync(this.Connection.Channels.GetStreamSessions(ChannelSession.Channel, startTime)); }

        public async Task AddUserRoles(ChannelModel channel, UserModel user, IEnumerable<UserRole> roles) { await this.RunAsync(this.Connection.Channels.UpdateUserRoles(ChannelSession.Channel, user, roles.Select(r => EnumHelper.GetEnumName(r)), null)); }

        public async Task RemoveUserRoles(ChannelModel channel, UserModel user, IEnumerable<UserRole> roles) { await this.RunAsync(this.Connection.Channels.UpdateUserRoles(ChannelSession.Channel, user, null, roles.Select(r => EnumHelper.GetEnumName(r)))); } 

        public async Task<IEnumerable<InteractiveGameListingModel>> GetOwnedInteractiveGames(ChannelModel channel) { return await this.RunAsync(this.Connection.Interactive.GetOwnedInteractiveGames(channel)); }

        public async Task<InteractiveGameListingModel> CreateInteractiveGame(ChannelModel channel, UserModel user, string name, InteractiveSceneModel defaultScene) { return await this.RunAsync(InteractiveGameHelper.CreateInteractive2Game(this.Connection, channel, user, name, defaultScene)); }

        public async Task<InteractiveGameVersionModel> GetInteractiveGameVersion(InteractiveGameVersionModel version) { return await this.RunAsync(this.Connection.Interactive.GetInteractiveGameVersion(version)); }

        public async Task UpdateInteractiveGameVersion(InteractiveGameVersionModel version) { await this.RunAsync(this.Connection.Interactive.UpdateInteractiveGameVersion(version)); }

        private void RestAPIService_OnRequestSent(object sender, Tuple<string, HttpContent> e) { Logger.Log(string.Format("Rest API Request: {0} - {1}", e.Item1, e.Item2)); }

        private void RestAPIService_OnSuccessResponseReceived(object sender, string e) { Logger.Log(string.Format("Rest API Success Response: {0}", e)); }

        private void RestAPIServices_OnFailureResponseReceived(object sender, RestServiceRequestException e) { Logger.Log(string.Format("Rest API Failure Response: {0}", e.ToString())); }
    }
}
