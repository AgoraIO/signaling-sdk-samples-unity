using UnityEngine;
using Agora.Rtm;
using TMPro;
using System.Text;

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

        SubscribeChannel();
        JoinChannelOptions options = new JoinChannelOptions();
        options.token = configData.rtcToken;
        options.withMetadata = false;
        options.withPresence = true;
        options.withLock = false;
     
        var result = await signalingChannel.JoinAsync(options);
        if (result.Status.Error)
        {
            isChannelJoined = false;
            LogError(string.Format("Join Status.Reason:{0} ", result.Status.Reason));
        }
        else
        {
            string str = string.Format("Join Response: channelName:{0} userId:{1}",
                result.Response.ChannelName, result.Response.UserId);
            isChannelJoined = true;
            LogInfo(str);
        }
    }

    // Subscribe/unsubscribe from the channel
    public void SubscribeChannel()
    {
        if (isSubscribed)
        {
            Unsubscribe();
        }
        else
        {
            Subscribe();
        }
    }

    public async void LeaveStreamChannel()
    {
        if (signalingChannel == null)
        {
            LogError("Rtm StreamChannel is null");
            return;
        }

        var result = await signalingChannel.LeaveAsync();

        if (result.Status.Error)
        {
            LogError(string.Format("StreamChannel.Leave Status.ErrorCode:{0} ", result.Status.ErrorCode));
        }
        else
        {
            string str = string.Format("StreamChannel.Leave Response: channelName:{0} userId:{1}",
                result.Response.ChannelName, result.Response.UserId);
            isChannelJoined = false;
            LogInfo(str);
        }

    }

    // Subscribe to a topic to receive messages.
    public async void SubscribeTopic(string topic)
    {
        TopicOptions options = new TopicOptions();
        var result = await signalingChannel.SubscribeTopicAsync(topic, options);
        if (result.Status.Error)
        {
            Debug.LogError(string.Format("signalingChannel.SubscribeTopicAsync Status.Reason:{0} ", result.Status.Reason));
        }
        else
        {
            string str = string.Format("signalingChannel.SubscribeTopicAsync Response: channelName:{0} userId:{1} topic:{2}",
              result.Response.ChannelName, result.Response.UserId, result.Response.Topic);
            Debug.Log(str);
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
        var result = await signalingChannel.JoinTopicAsync(topic, options);
        if (result.Status.Error)
        {
            LogError(string.Format("signalingChannel.JoinTopic Status.Reason:{0} ", result.Status.Reason));
        }
        else
        {
            string str = string.Format("signalingChannel.JoinTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3}",
              result.Response.ChannelName, result.Response.UserId, result.Response.Topic, result.Response.Meta);
            isTopicJoined = false;
            LogInfo(str);
            SubscribeTopic(topic);
        }
    }

    public async void LeaveTopic(string topic)
    {
        if (signalingChannel == null)
        {
            LogError("StreamChannel not created!");
            return;
        }

        var result = await signalingChannel.LeaveTopicAsync(topic);

        if (result.Status.Error)
        {
            LogError(string.Format("signalingChannel.LeaveTopic Status.Reason:{0} ", result.Status.Reason));
        }
        else
        {
            string str = string.Format("signalingChannel.LeaveTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3}",
              result.Response.ChannelName, result.Response.UserId, result.Response.Topic, result.Response.Meta);
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

        TopicMessageOptions options = new TopicMessageOptions();
        options.customType = "byte";
        var result = await signalingChannel.PublishTopicMessageAsync(topic, Encoding.UTF8.GetBytes(msg), options);
        if(result.Status.Error)
        {
            LogError(string.Format("signalingChannel.PublishTopicMessageAsync Status.Reason:{0} ", result.Status.Reason));
        }
        else
        {
            msg = "Topic name:" + topic + "message: " + msg;
            signalingUI.AddTextToDisplay(msg, Color.blue, TextAlignmentOptions.BaselineRight);
            LogInfo("StreamChannel.PublishTopicMessage  ret:" + result.Status.ErrorCode);
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

