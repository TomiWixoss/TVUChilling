# FIX GRADLE ML KIT DEPENDENCIES

## Vấn đề
Build lỗi: `package com.google.mlkit.vision.common does not exist`

**Root cause:** Unity không tự động merge file `build.gradle` thông thường. Cần enable Custom Gradle Template.

---

## Solution: Enable Custom Main Gradle Template

### Bước 1: Enable Custom Gradle Template

1. Unity Editor → **Edit** → **Project Settings**
2. Chọn **Player** (icon Android)
3. Tab **Publishing Settings** (scroll xuống)
4. Tìm section **Build**
5. Tick checkbox: ☑ **Custom Main Gradle Template**

**Kết quả:** Unity sẽ generate file `Assets/Plugins/Android/mainTemplate.gradle`

### Bước 2: Verify mainTemplate.gradle

File `Assets/Plugins/Android/mainTemplate.gradle` phải có:

```gradle
dependencies {
    implementation fileTree(dir: 'libs', include: ['*.jar'])
    implementation 'com.google.mlkit:text-recognition:16.0.0'
**DEPS**}
```

**Lưu ý:** Dòng `**DEPS**` là placeholder của Unity, KHÔNG XÓA!

### Bước 3: Clean Build

1. Unity → **File** → **Build Settings**
2. Click **Build** (không phải Build and Run)
3. Chọn nơi lưu APK
4. Đợi build xong

**Expected:** Build thành công, không còn lỗi ML Kit

---

## Alternative: Dùng External Dependency Manager (EDM4U)

Nếu approach trên không work, dùng EDM4U:

### Install EDM4U

1. Download: https://github.com/googlesamples/unity-jar-resolver/releases
2. Import `.unitypackage` vào project
3. Tạo file `Assets/Plugins/Android/Dependencies.xml`:

```xml
<dependencies>
  <androidPackages>
    <androidPackage spec="com.google.mlkit:text-recognition:16.0.0" />
  </androidPackages>
</dependencies>
```

4. Unity → **Assets** → **External Dependency Manager** → **Android Resolver** → **Resolve**

---

## Troubleshooting

### Lỗi: "Custom Main Gradle Template" không có trong Publishing Settings
**Fix:** Update Unity version hoặc dùng EDM4U approach

### Build vẫn lỗi sau khi enable template
**Fix:** 
1. Delete folder `Library/Bee`
2. Unity → **Assets** → **Reimport All**
3. Build lại

### Lỗi: "Gradle sync failed"
**Fix:**
1. Check internet connection (Gradle cần download ML Kit)
2. Unity → **Preferences** → **External Tools** → Verify Android SDK path
3. Restart Unity Editor

---

## Verify ML Kit đã được add

Sau khi build thành công, check log:

```
> Task :unityLibrary:compileReleaseJavaWithJavac
BUILD SUCCESSFUL
```

Không còn lỗi `package com.google.mlkit.vision.common does not exist`

---

## Next Steps

Sau khi fix Gradle:
1. Build and Run APK
2. Test AR tracking
3. Quét cúp → Check Console log
4. Verify OCR hoạt động

**Status:** Ready to build after enabling Custom Gradle Template
