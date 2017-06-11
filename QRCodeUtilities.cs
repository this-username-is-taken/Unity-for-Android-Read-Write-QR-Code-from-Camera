using System.Collections.Generic;
using UnityEngine;
using ZXing; // You need to have the zxing.unity.dll in the project
using ZXing.Common;
using ZXing.QrCode.Internal;

public static class QRCodeUtilities {
	
	private static readonly Color32 white = new Color32(255, 255, 255, 255);
	private static readonly Color32 black = new Color32(0, 0, 0, 255);

	// Generates a Texture2D that can be displayed in a RawImage
	// Works better (for me) on Android than the built in methods of ZXing
	public static Texture2D GenerateQRCode(string text, int quietZoneSize) {
		// The white area around the QR Code. Usually 4 units is the minimum
		quietZoneSize = Mathf.Max(0, quietZoneSize);
		
		// Step 1 - generate the QRCode dot array
		Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
		hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
		QRCode qrCode = Encoder.encode(text, ErrorCorrectionLevel.L, hints);

		// Step 2 - create a Texture2D out of this array
		int width = qrCode.Matrix.Width + 2 * quietZoneSize;
		int height = qrCode.Matrix.Height + 2 * quietZoneSize;
		int qrCodeStartX = quietZoneSize;
		int qrCodeStartY = quietZoneSize;
		int qrCodeEndX = width - 1 - quietZoneSize;
		int qrCodeEndY = height - 1 - quietZoneSize;
		Texture2D encoded = new Texture2D(width, height, TextureFormat.RGBA32, false);

		Color32[] colors = new Color32[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				if (x < qrCodeStartX || y < qrCodeStartY || x > qrCodeEndX || y > qrCodeEndY) {
					colors[x + (height - 1 - y) * width] = white;
				} else {
					colors[x + (height - 1 - y) * width] = qrCode.Matrix.Array[x - quietZoneSize][y - quietZoneSize] == 1 ? black : white;
				}
			}
		}
		encoded.filterMode = FilterMode.Point;
		encoded.SetPixels32(colors);
		encoded.Apply();

		return encoded;
	}

	public static Result DecodeQRCode(Texture2D texture2D) {
		// Define the reader
		BarcodeReader barcodeReader = new BarcodeReader();
		List<BarcodeFormat> possibleFormats = new List<BarcodeFormat>();
		possibleFormats.Add(BarcodeFormat.QR_CODE);
		barcodeReader.Options = new DecodingOptions() {
			PossibleFormats = possibleFormats,
			CharacterSet = "UTF-8",
			TryHarder = true
		};

		Result result = barcodeReader.Decode(texture2D.GetPixels32(), texture2D.width, texture2D.height);
		if (result == null) {
			// Try rotating the image. I know that QR codes don't care about orientation, but without this the scan fails often
			texture2D = RotateTexture(texture2D);
			result = barcodeReader.Decode(texture2D.GetPixels32(), texture2D.width, texture2D.height);
		}

		return result;
	}

	private static Texture2D RotateTexture(Texture2D image) {
		//flip image width<>height, as we rotated the image, it might be a rect. not a square image
		Texture2D target = new Texture2D(image.height, image.width, image.format, false);    
		Color32[] pixels = image.GetPixels32(0);
		pixels = RotateTextureGrid(pixels, image.width, image.height);
		target.SetPixels32(pixels);
		target.Apply();
		return target;
	}


	private static Color32[] RotateTextureGrid(Color32[] texture, int width, int height) {
		//reminder we are flipping these in the target
		Color32[] rotatedTexture = new Color32[width * height];      

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				//juggle the pixels around
				rotatedTexture[(height - 1) - y + x * height] = texture[x + y * width];         
			}
		}

		return rotatedTexture;
	}
}
