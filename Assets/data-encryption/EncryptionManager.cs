using Agora.Rtm;

public class EncryptionManager : AuthenticationManager
{

    public override void SetupSignalingEngine()
    {
        RtmEncryptionConfig encryptionConfig = new RtmEncryptionConfig();
        encryptionConfig.encryptionKey = configData.cipherKey;
        encryptionConfig.encryptionSalt = System.Text.Encoding.UTF8.GetBytes(configData.salt);
        encryptionConfig.encryptionMode = RTM_ENCRYPTION_MODE.AES_256_GCM;
        rtmConfig.encryptionConfig = encryptionConfig;
        base.SetupSignalingEngine();
    }
}
