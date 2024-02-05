# Unity reference app for Agora Signaling SDK

This repository holds the code examples used for the [Agora Signaling SDK for Unity](https://docs-beta.agora.io/en/signaling/overview/product-overview?platform=android) documentation. It is a robust and comprehensive documentation reference app for Unity, designed to enhance your productivity and understanding. It's built to be flexible, easily extensible, and beginner-friendly.

Clone the repo, run, and test the samples, and use the code in your own project. Enjoy.

- [Samples](#samples)
- [Prerequisites](#prerequisites)
- [Run the app](#run-the-app)
- [Contact](#contact)

## Samples

This reference app includes several samples that illustrate the functionality and features of Agora Signaling SDK. Each sample is self-contained and the relevant code can be found in its own folder in the root directory. For more information about each sample, see:

- [SDK quickstart](/Assets/signaling-manager/) - the minimum code you need to integrate low-latency, high-concurrency
  signaling features into your app using Signaling SDK.
- [Secure authentication with tokens](/Assets/authentication-workflow) - quickly set up an authentication token server, retrieve a token from the server, and use it to connect securely to Signaling as a specific user.
- [Stream channels](/Assets/stream-channel/) - communicate to other users in topics.
- [Store channel and user data](/Assets/storage) - easily store data for users and channels without the need to
  set up your own databases. 
- [Connect through restricted networks with Cloud Proxy](/Assets/cloud-proxy/) - ensure reliable connectivity for your users when they connect from an
  environment with a restricted network.
- [Data encryption](/Assets/data-encryption/) - integrate built-in data encryption into your app using Signaling.
- [Geofencing](geofencing) - only connect to Signaling within the specified region.

To view the UI implementation, see [SignalingUI](/Assets/signaling-manager/SignalingUI.cs).

## Prerequisites

Before getting started with this reference app, ensure you have the following set up:

- [Unity Hub](https://unity.com/download)
- [Unity Editor 2017.X LTS or higher](https://unity.com/releases/editor/archive)
- Microsoft Visual Studio 2017 or higher

## Run the app

1. **Clone the repository**

    To clone the repository to your local machine, open Terminal and navigate to the directory where you want to clone the repository. Then, use the following command:

    ```sh
    git clone https://github.com/AgoraIO/signaling-sdk-samples-unity.git
    ```

1. **Open the project**

    1. In Unity Hub, Open `signaling-sdk-samples-unity`, Unity Editor opens the project.
       
       Unity Editor warns of compile errors. Don't worry, you fix them when you import Video SDK for Unity. 

    1. Go `Assets\Scenes`, and open `SampleScene.unity`. The sample scene opens.
         
    1. Unzip [the latest version of the Agora Video SDK](https://download.agora.io/sdk/release/Agora_Unity_RTM_SDK_v2.1.9.zip?_gl=1*y2lyxl*_ga*MjA2MzYxMjY4Mi4xNzAzMDczMjA1*_ga_BFVGG7E02W*MTcwNzEyMDUzMC4xMi4xLjE3MDcxMjEwODAuMC4wLjA.) to a local folder.

   1. In **Unity**, click **Assets** > **Import Package** > **Custom Package**.

   1. Navigate to the Video SDK package and click **Open**.

   1. In **Import Unity Package**, click **Import**.
   
      Unity recompiles the Video SDK samples for Unity and the warnings disappear. 

1. **Modify the project configuration**

   The app loads connection parameters from the [`config.json`](/Assets/utils/Config.json) file. Ensure that the file is populated with the required parameter values before running the application.

    - `uid`: The user ID associated with the application.
    - `appId`: (Required) The unique ID for the application obtained from [Agora Console](https://console.agora.io). 
    - `channelName`: The default name of the channel to join.
    - `token`: A token generated for `uid`. You generate a temporary token using the [Agora token builder](https://agora-token-generator-demo.vercel.app/).
    - `serverUrl`: The URL for the token generator. See [Secure authentication with tokens](https://docs-beta.agora.io/en/signaling/get-started/authentication-workflow) for information on how to set up a token server.
    - `tokenExpiryTime`: The time in seconds after which a token expires.

    If a valid `serverUrl` is provided, all samples use the token server to obtain a token except the **SDK quickstart** project that uses the `token`. If a `serverUrl` is not specified, all samples except **Secure authentication with tokens** use the `token` from `config.json`.

1. **Build and run the project**

    In **Unity Editor**, click **Play**. A moment later you see the game running on your development device.

1. **Run the samples in the reference app**

    From the main app screen, choose and launch a sample.

## Contact

If you have any questions, issues, or suggestions, please file an issue in our [GitHub Issue Tracker](https://github.com/AgoraIO/signaling-sdk-samples-android/issues).
