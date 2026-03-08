# Fix Dancer Animation - Hướng dẫn

## VẤN ĐỀ: Animation không chạy

FBX có animation clip `mixamo.com` nhưng không play vì:
- Animator Controller chưa có
- Animation clip chưa được assign

## GIẢI PHÁP:

### Bước 1: Tạo Animator Controller

```
1. Project → Assets/Models/Dancer/
2. Right click → Create → Animator Controller
3. Đặt tên: "DancerAnimator"
```

### Bước 2: Setup Animation State

```
1. Double click "DancerAnimator" (mở Animator window)
2. Trong Animator window:
   a. Right click → Create State → Empty
   b. Đặt tên state: "Dancing"
   c. Right click state "Dancing" → Set as Layer Default State (màu cam)
   
3. Select state "Dancing"
4. Inspector → Motion:
   - Click vòng tròn
   - Chọn animation clip "mixamo.com" (từ dancing_character.fbx)
   
5. File → Save (Ctrl+S)
```

### Bước 3: Assign Animator Controller vào Prefab

```
1. Project → Assets/Prefabs/ImageTracking/DancerAR.prefab
2. Double click (mở Prefab mode)
3. Hierarchy → Select "dancing_character" (child)
4. Inspector → Animator component:
   - Controller: Kéo "DancerAnimator" vào đây
   - Avatar: "dancing_characterAvatar" (auto-generated)
   - Apply Root Motion: ✓ Check
   - Update Mode: Normal
   - Culling Mode: Always Animate
   
5. File → Save (Ctrl+S)
6. Exit Prefab mode
```

### Bước 4: Test

```
1. Hierarchy → Kéo DancerAR.prefab vào scene
2. Play mode
3. Kiểm tra:
   ✅ Character nhảy
   ✅ Animation loop liên tục
4. Stop play mode
5. Xóa test GameObject
```

## KẾT QUẢ:

✅ Animation sẽ tự động play khi prefab được spawn
✅ Loop liên tục (không dừng)

