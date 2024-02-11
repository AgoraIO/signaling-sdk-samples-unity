using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtm;
using TMPro;

public class StreamChannelManager : AuthenticationManager
{
    internal bool isChannelJoined = false;
    internal bool isTopicJoined = false;
    public override void SetupSignalingEngine()
    {
        base.SetupSignalingEngine();
    }


    public async void JoinStreamChannel()
    {
        if (signalingChannel == null)
        {
            LogError("Rtm StreamChannel is null");
            return;
        }

        JoinChannelOptions options = new JoinChannelOptions();
        options.token = configData.rtcToken;
        options.withMetadata = false;
        options.withPresence = true;
        options.withLock = false;

        var (status, response) = await signalingChannel.JoinAsync(options);
        if (status.Error)
        {
            isChannelJoined = false;
            LogError(string.Format("Join Status.Reason:{0} ", status.ErrorCode));
        }
        else
        {
            string str = string.Format("Join Response: channelName:{0} userId:{1}",
                response.ChannelName, response.UserId);
            isChannelJoined = true;
            LogInfo(str);
        }
    }

    public async void LeaveStreamChannel()
    {
        if (signalingChannel == null)
        {
            LogError("Rtm StreamChannel is null");
            return;
        }

        var (status, response) = await signalingChannel.LeaveAsync();

        if (status.Error)
        {
            LogError(string.Format("StreamChannel.Leave Status.ErrorCode:{0} ", status.ErrorCode));
        }
        else
        {
            string str = string.Format("StreamChannel.Leave Response: channelName:{0} userId:{1}",
                response.ChannelName, response.UserId);
            isChannelJoined = false;
            LogInfo(str);
        }

    }
    public async void JoinTopic(string topic)
    {
        JoinTopicOptions options = new JoinTopicOptions()
        {
            qos = RTM_MESSAGE_QOS.ORDERED,
            priority = RTM_MESSAGE_PRIORITY.NORMAL,
            meta = "My topic",
            syncWithMedia = true
        };
        var (status, response) = await signalingChannel.JoinTopicAsync(topic, options);
        if (status.Error)
        {
            LogError(string.Format("signalingChannel.JoinTopic Status.Reason:{0} ", status.Reason));
        }
        else
        {
            string str = string.Format("signalingChannel.JoinTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3}",
              response.ChannelName, response.UserId, response.Topic, response.Meta);
            isTopicJoined = false;
            LogInfo(str);
        }
    }

    public async void LeaveTopic(string topic)
    {
        if (signalingChannel == null)
        {
            LogError("StreamChannel not created!");
            return;
        }

        var (status, response) = await signalingChannel.LeaveTopicAsync(topic);

        if (status.Error)
        {
            LogError(string.Format("signalingChannel.LeaveTopic Status.Reason:{0} ", status.Reason));
        }
        else
        {
            string str = string.Format("signalingChannel.LeaveTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3}",
              response.ChannelName, response.UserId, response.Topic, response.Meta);
            isTopicJoined = false;
            LogInfo(str);
        }
    }

    public async void SendTopicMessage(string msg, string topic)
    {
        if (signalingChannel == null)
        {
            LogError("StreamChannel is null");
            return;
        }

        if (topic == "" || msg == "")
        {
            LogError("topic or message is empty");
            return;
        }

        PublishOptions options = new PublishOptions();

        var (status, response) = await signalingChannel.PublishTopicMessageAsync(topic, msg, options);
        if(status.Error)
        {
            LogError(string.Format("signalingChannel.PublishTopicMessageAsync Status.Reason:{0} ", status.Reason));
        }
        else
        {
            msg = "Topic name:" + topic + "message: " + msg;
            signalingUI.AddTextToDisplay(msg, Color.blue, TextAlignmentOptions.BaselineRight);
            LogInfo("StreamChannel.PublishTopicMessage  ret:" + status.ErrorCode);
        }
    }

    public void ReleaseStreamChannel()
    {
        if (signalingChannel == null)
        {
            LogInfo("rtm stream channel is empty");
            return;
        }

        signalingChannel.Dispose();
        signalingChannel = null;
    }
}

