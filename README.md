# AttendAR
ARCore (Android)/Desktop app which detects and recognizes student faces to record their "class attendance".
#
Built in Unity3D, Utilizing [Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/directory/). Project based on demos from Unity assets [Cloud Face Detection](https://assetstore.unity.com/packages/tools/ai/cloud-face-detection-54489) and [Cloud User Manager](https://assetstore.unity.com/packages/tools/ai/cloud-user-manager-61391) by [Innovative Smart Systems](https://assetstore.unity.com/publishers/18913).
#
You will need an [Azure Portal](https://portal.azure.com/) subscription (free) to use the [Face](https://azure.microsoft.com/en-us/services/cognitive-services/face/) API.

## Usage
<pre>
Open project and import necessary assets/libraries if prompted by Unity
</pre>

<pre>
In project explorer, navigate to 'CloudFaceDetection' > 'DemoScenes' and open 'UserRecognitionScene0' scene object
</pre>

<pre>
Click on 'CloudFaceController' GameObject
</pre>

<pre>
On 'Cloud Face Manager (Script)' component, enter your selected 'Face Service Location' and 'Face Subscription Key' provided by Azure Portal
</pre>

<pre>
Run (play) scene (or build to ARCore compatible device running Android >= 8.0)
</pre>

<pre>
To build app, just select appropriate platform under 'File > Build Settings'
</pre>

## Screenshots

<h5>General Interface</h5>

![Interface](./Screenshots/Screenshot_20190122-154856_AttendAR.jpg?raw=true "Interface")

<h5>User Creation</h5>

![User Creation](./Screenshots/Screenshot_20190122-155004_AttendAR.jpg?raw=true "User Creation")

![User Creation Confirmation](./Screenshots/Screenshot_20190122-155049_AttendAR.jpg?raw=true "User Creation Confirmation")

<h5>User Recognition</h5>

![User Recognition](./Screenshots/Screenshot_20190122-155138_AttendAR.jpg?raw=true "User Recognition")

<h5>Resize Image Viewport</h5>

![Resize Image Viewport](./Screenshots/Screenshot_20190122-155229_AttendAR.jpg?raw=true "Resize Image Viewport")

<h5>User Login via Interactable Cards</h5>

![User Login](./Screenshots/Screenshot_20190122-155302_AttendAR.jpg?raw=true "User Login")

<h5>Built-In User Manager</h5>

![User Manager](./Screenshots/Screenshot_20190122-155332_AttendAR.jpg?raw=true "User Manager")

<h5>View and Modify User Details</h5>

![User Details](./Screenshots/Screenshot_20190122-155458_AttendAR.jpg?raw=true "User Details")

<h5>Delete Users</h5>

![Delete User](./Screenshots/Screenshot_20190122-155531_AttendAR.jpg?raw=true "Delete User")
