using System.IO;
using UnityEngine;

public class QRCodeFromPhoto : MonoBehaviour {

	private const string qrCodeImageName = "QRcodeImage.jpg";
	private byte[] fileData;
	private bool scanningQRCode;

	private void OnApplicationPause(bool pause) {
		if (!pause && scanningQRCode) {
			PhotoRequestEnded();
		}
	}

	// You can call this from the UI
	public void RequestPhoto() {
		AndroidJavaClass mediaStoreClass = new AndroidJavaClass("android.provider.MediaStore");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
		intentObject.Call<AndroidJavaObject>("setAction", mediaStoreClass.GetStatic<string>("ACTION_IMAGE_CAPTURE"));

		//define the path and filename to save photo taken by Camera activity
		string filePath = Application.persistentDataPath + "/" + qrCodeImageName;
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", filePath);
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);

		intentObject.Call<AndroidJavaObject>("putExtra", mediaStoreClass.GetStatic<string>("EXTRA_OUTPUT"), uriObject);

		// Start the activity
		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		currentActivity.Call("startActivity", intentObject);

		scanningQRCode = true;
	}

	private void PhotoRequestEnded() {
		string qrCodeImagePath = Application.persistentDataPath + "/" + qrCodeImageName;
		if (File.Exists(qrCodeImagePath)) {
			// Load the image into a Texture2D
			fileData = File.ReadAllBytes(qrCodeImagePath);
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(fileData);
			
			var result = QRCodeUtilities.DecodeQRCode(texture2D);
			string scannedText = result == null ? "" : result.Text;
			OnQRCodeScanned(scannedText);

			File.Delete(qrCodeImagePath);
		}
		scanningQRCode = false;
	}

	private void OnQRCodeScanned(string scannedText) {
		// Here you do whatever you want to do with the scanned text
		// scannedText will be empty when the scan fails
	}
}
