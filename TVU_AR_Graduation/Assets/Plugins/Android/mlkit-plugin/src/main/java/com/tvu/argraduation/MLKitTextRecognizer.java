package com.tvu.argraduation;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import com.google.mlkit.vision.common.InputImage;
import com.google.mlkit.vision.text.Text;
import com.google.mlkit.vision.text.TextRecognition;
import com.google.mlkit.vision.text.TextRecognizer;
import com.google.mlkit.vision.text.latin.TextRecognizerOptions;
import com.unity3d.player.UnityPlayer;

public class MLKitTextRecognizer {
    
    private static TextRecognizer recognizer;
    
    public static void Initialize() {
        if (recognizer == null) {
            recognizer = TextRecognition.getClient(TextRecognizerOptions.DEFAULT_OPTIONS);
        }
    }
    
    public static void RecognizeText(byte[] imageBytes) {
        try {
            Bitmap bitmap = BitmapFactory.decodeByteArray(imageBytes, 0, imageBytes.length);
            
            if (bitmap == null) {
                SendResultToUnity("ERROR: Failed to decode image");
                return;
            }
            
            InputImage image = InputImage.fromBitmap(bitmap, 0);
            
            if (recognizer == null) {
                Initialize();
            }
            
            recognizer.process(image)
                .addOnSuccessListener(visionText -> {
                    String resultText = visionText.getText();
                    
                    if (resultText.isEmpty()) {
                        SendResultToUnity("ERROR: No text detected");
                    } else {
                        SendResultToUnity(resultText);
                    }
                })
                .addOnFailureListener(e -> {
                    SendResultToUnity("ERROR: " + e.getMessage());
                });
                
        } catch (Exception e) {
            SendResultToUnity("ERROR: " + e.getMessage());
        }
    }
    
    private static void SendResultToUnity(String text) {
        UnityPlayer.UnitySendMessage("XR Origin", "OnTextRecognitionComplete", text);
    }
}