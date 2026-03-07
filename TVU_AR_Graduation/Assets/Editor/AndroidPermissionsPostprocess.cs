#if UNITY_ANDROID
using UnityEditor.Android;
using UnityEngine;

public class AndroidPermissionsPostprocess : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 0; // Run before WebView postprocess
    
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var manifestPath = path + "/src/main/AndroidManifest.xml";
        var manifest = new AndroidManifest(manifestPath);
        
        // Add camera permissions
        manifest.AddPermission("android.permission.CAMERA");
        manifest.AddPermission("android.permission.FLASHLIGHT");
        
        // Add storage permissions
        manifest.AddPermission("android.permission.WRITE_EXTERNAL_STORAGE");
        manifest.AddPermission("android.permission.READ_EXTERNAL_STORAGE");
        
        // Add audio permission
        manifest.AddPermission("android.permission.RECORD_AUDIO");
        
        manifest.Save();
        Debug.Log("Added camera/video/flash permissions to AndroidManifest");
    }
}

// Helper class to manipulate AndroidManifest
public class AndroidManifest
{
    private readonly string path;
    private System.Xml.XmlDocument doc;
    private System.Xml.XmlElement manifestElement;
    
    public AndroidManifest(string path)
    {
        this.path = path;
        doc = new System.Xml.XmlDocument();
        doc.Load(path);
        manifestElement = doc.DocumentElement;
    }
    
    public void AddPermission(string permission)
    {
        // Check if permission already exists
        var nodes = doc.SelectNodes($"//uses-permission[@android:name='{permission}']",
            GetNamespaceManager());
        
        if (nodes.Count == 0)
        {
            var element = doc.CreateElement("uses-permission");
            element.SetAttribute("name", "http://schemas.android.com/apk/res/android", permission);
            manifestElement.AppendChild(element);
        }
    }
    
    public void Save()
    {
        doc.Save(path);
    }
    
    private System.Xml.XmlNamespaceManager GetNamespaceManager()
    {
        var nsMgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");
        return nsMgr;
    }
}
#endif
