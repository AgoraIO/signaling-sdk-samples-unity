using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

// Data structure for holding the RTM token
public class TokenStruct
{
    public string rtmToken;
}

// AuthenticationManager class extends SignalingManager
public class AuthenticationManager : SignalingManager
{
    // Asynchronously fetches an RTM token from the server
    public async Task FetchRTMToken()
    {
        // Check if required parameters are provided in the configuration
        if (string.IsNullOrEmpty(configData.uid) || string.IsNullOrEmpty(configData.serverUrl) || configData.tokenExpiryTime == null)
        {
            LogInfo("Please specify all required parameters in the config.json file to fetch a token from the server");
            return;
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
        TokenStruct tokenInfo = JsonUtility.FromJson<TokenStruct>(request.downloadHandler.text);

        // Log the retrieved token
        LogInfo($"Retrieved token:`1` {tokenInfo.rtmToken}");

        // Update the configuration with the fetched token`1
        configData.token = tokenInfo.rtmToken;
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
        await FetchToken();

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