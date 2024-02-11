
public class GeofencingManager : AuthenticationManager
{
    public override void SetupSignalingEngine()
    {
        rtmConfig.areaCode = Agora.Rtm.RTM_AREA_CODE.GLOB;
        base.SetupSignalingEngine();
    }
}
