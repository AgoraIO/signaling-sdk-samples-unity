using Agora.Rtm;
public class ProxyManager : AuthenticationManager
{
    public override void SetupSignalingEngine()
    {
        // Define proxy configuration
        RtmProxyConfig config = new RtmProxyConfig();
        config.server = "<your proxy server domain name or IP address>"; // server URL
        config.port = 8080; // server port
        config.account = "<proxy login account>"; // Account name
        config.password = "<proxy login password>"; // Account password
        rtmConfig.proxyConfig = config;
        base.SetupSignalingEngine();
    }
}
