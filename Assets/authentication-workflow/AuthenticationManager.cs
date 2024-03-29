﻿using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Agora.Rtm;

// Data structure for holding the RTM token
public class RtmTokenStruct
{
    public string rtmToken;
}

// Data structure for holding the RTM token
public class RtcTokenStruct
{
    public string rtcToken;
}

// AuthenticationManager class extends SignalingManager
public class AuthenticationManager : SignalingManager
{
    internal bool isStreamChannelJoined = false;

    // Asynchronously fetches an RTM token from the server
    public async Task FetchRtmToken(string userName)
    {
        // Check if required parameters are provided in the configuration
        if (string.IsNullOrEmpty(configData.uid) || string.IsNullOrEmpty(configData.serverUrl) || configData.tokenExpiryTime == null)
        {
            LogInfo("Please specify all required parameters in the config.json file to fetch a token from the server");
            return;
        }

        // Use user name as UID
        if (userName != "")
        {
            configData.uid = userName;
        }

        // Construct the URL to request the RTM token
        string url = $"{configData.serverUrl}/rtm/{configData.uid}/?expiry={configData.tokenExpiryTime}";

        // Use UnityWebRequest to send a GET request to the server
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Asynchronously send the request
        var operation = request.SendWebRequest();

        // Wait until the operation is done
        while (!operation.isDone)
        {
            await Task.Yield();
        }
         
        // Check for network or HTTP errors
        if (request.isNetworkError || request.isHttpError)
        {
            LogError($"Failed to fetch token. Error: {request.error}");
            return;
        }

        // Deserialize the response JSON into TokenStruct
        RtmTokenStruct tokenInfo = JsonUtility.FromJson<RtmTokenStruct>(request.downloadHandler.text);

        // Log the retrieved token
        LogInfo($"Retrieved rtm token: {tokenInfo.rtmToken}");

        // Update the configuration with the fetched token`1
        configData.token = tokenInfo.rtmToken;
    }

    // Fetch a rtc token
    public async Task FetchRtcToken(string channelName, string uid)
    {

        string url = string.Format("{0}/rtc/{1}/{2}/uid/{3}/?expiry={4}", configData.serverUrl, channelName , 1 , uid , configData.tokenExpiryTime);
        UnityWebRequest request = UnityWebRequest.Get(url);
        Debug.Log(url);
        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.isNetworkError || request.isHttpError)
        {
            LogInfo(request.error);
            return;
        }

        RtcTokenStruct tokenInfo = JsonUtility.FromJson<RtcTokenStruct>(request.downloadHandler.text);
        Debug.Log("Retrieved rtc token : " + tokenInfo.rtcToken);
        configData.rtcToken = tokenInfo.rtcToken;
    }

    public async void JoinAndLeaveStreamChannel(string channelName)
    {
        if(!isLogin)
        {
            await FetchRtmToken(configData.uid);
            Login(configData.uid, configData.token);
        }
        if(!isStreamChannelJoined)
        {
            if (signalingChannel == null)
            {
                CreateChannel(channelName);
            }

            // Fetch a rtc token for the stream channel
            await FetchRtcToken(channelName, configData.uid);

            if (configData.rtcToken == "")
            {
                LogInfo("Token was not fetched from the server");
                return;
            }

            // Configure the channel options
            JoinChannelOptions options = new JoinChannelOptions();
            options.token = configData.rtcToken;
            options.withMetadata = false;
            options.withPresence = true;
            options.withLock = false;

            // Join the stream channel
            var result = await signalingChannel.JoinAsync(options);
            if (result.Status.Error)
            {
                LogError(string.Format("Join Status.Reason:{0} ", result.Status.Reason));
            }
            else
            {
                string str = string.Format("Join Response: channelName:{0} userId:{1}",
                    result.Response.ChannelName, result.Response.UserId);
                isStreamChannelJoined = true;
                LogInfo(str);
            }
        }
        else
        {
            var result = await signalingChannel.LeaveAsync();
            if (result.Status.Error)
            {
                LogError(string.Format("StreamChannel.Leave Status.ErrorCode:{0} ", result.Status.ErrorCode));
            }
            else
            {
                string str = string.Format("StreamChannel.Leave Response: channelName:{0} userId:{1}",
                    result.Response.ChannelName, result.Response.UserId);
                isStreamChannelJoined = false;
                LogInfo(str);
            }

        }

    }

    // Renew the RTM token
    public void RenewToken()
    {
        if (configData.token == "")
        {
            Debug.Log("Token was not retrieved");
            return;
        }

        // Update RTC Engine with new token
        signalingEngine.RenewTokenAsync(configData.token);
    }

    // Handle token expiration event
    public async override void OnTokenPrivilegeWillExpire(string channelName)
    {
        // Log an informational message
        LogInfo($"OnTokenPrivilegeWillExpire channelName {channelName}");

        // Asynchronously fetch a new token
        await FetchRtmToken(configData.uid);

        // Check if a valid token is retrieved
        if (!string.IsNullOrEmpty(configData.token))
        {
            // Asynchronously renew the RTM token
            var result = await signalingEngine.RenewTokenAsync(configData.token);

            // Log an error if the token renewal fails
            if (result.Status.Error)
            {
                LogError($"Failed to renew token. Error: {result.Status.Reason}");
            }
        }
        else
        {
            // Log an error if the token was not retrieved
            LogError("Token was not retrieved");
        }
    }

}