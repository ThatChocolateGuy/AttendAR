# AttendAR
ARCore (Android) app which detects and recognizes student faces, and records their "class attendance".
#
Built in Unity3D, Utilizing [Microsoft's Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/directory/).
#
You will need an [Azure Portal](https://portal.azure.com/) subscription (free) to use the [Face](https://azure.microsoft.com/en-us/services/cognitive-services/face/) API.

## Usage
```bash
Open Project and import necessary assets/libraries if prompted by Unity
```
```bash
Navigate to 'CloudFaceDetection' > 'DemoScenes' and open 'UserRecognitionScene0' scene object
```
```bash
Click on 'CloudFaceController' GameObject
```
```bash
On 'Cloud Face Manager (Script)' component, enter your selected 'Face Service Location' and 'Face Subscription Key' provided by Azure Portal
```
```bash
Run (play) scene (or build to ARCore compatible device running Android >= 8.0)
```
