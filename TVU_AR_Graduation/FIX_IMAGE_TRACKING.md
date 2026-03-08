# Fix Image Tracking - Cải thiện độ chính xác

## VẤN ĐỀ: Tracking kém

Dancer spawn nhưng:
- Mất tracking nhanh
- Jitter (rung lắc)
- Không gắn chặt với bằng

## NGUYÊN NHÂN:

1. **Image target chất lượng thấp** → AR không nhận diện tốt
2. **Kích thước physical size sai** → Scale không đúng
3. **Lighting kém** → Camera không thấy rõ bằng

## GIẢI PHÁP:

### Bước 1: Kiểm tra Image Target Quality

```
1. Project → Assets/AR/CupImageLibrary
2. Select "Diploma_TVU" reference image
3. Inspector → Quality:
   - ⭐⭐⭐⭐⭐ (5 sao) → Tốt
   - ⭐⭐⭐ (3 sao) → Trung bình
   - ⭐ (1 sao) → Kém → CẦN THAY ẢNH KHÁC
```

### Bước 2: Set Physical Size đúng

```
1. Select "Diploma_TVU" trong CupImageLibrary
2. Inspector:
   - Specify Size: ✓ Check
   - Physical Size:
     * Width: 0.297 (A4 ngang = 29.7cm)
     * Height: 0.21 (A4 ngang = 21cm)
   - Keep Texture at Original Size: ✓ Check
   
3. Apply
```

### Bước 3: Tăng Max Number of Moving Images

```
1. Hierarchy → AR Tracked Image Manager
2. Inspector:
   - Max Number of Moving Images: 2 (đã đúng)
   - Tracking Mode: Tracking and Detection
```

### Bước 4: Cải thiện ảnh target (nếu quality thấp)

**Yêu cầu ảnh tốt:**
- Độ phân giải: Tối thiểu 512x512px, khuyến nghị 1024x1024px
- Có nhiều chi tiết (text, logo, pattern)
- Tránh ảnh mờ, blur
- Tránh ảnh đơn sắc (toàn trắng/đen)
- Có contrast cao

**Nếu ảnh hiện tại kém:**
```
1. Chụp lại bằng tốt nghiệp với:
   - Ánh sáng tốt
   - Camera focus rõ
   - Góc vuông (không xiên)
   
2. Crop ảnh chỉ lấy phần bằng (bỏ background)

3. Import vào Unity:
   - Copy ảnh mới vào Assets/Images/Targets/
   - Đặt tên: Diploma_TVU_HD.jpg
   
4. Thay trong CupImageLibrary:
   - Select CupImageLibrary
   - Remove "Diploma_TVU" cũ
   - Add "Diploma_TVU_HD" mới
```

### Bước 5: Tối ưu Prefab cho Tracking

```
1. Giảm scale nếu quá lớn:
   - DancerAR prefab → dancing_character
   - Scale: (0.2, 0.2, 0.2) thay vì 0.3
   - Character nhỏ hơn → ít bị mất tracking khi di chuyển

2. Thêm offset nếu cần:
   - dancing_character → Position Y: 0.05
   - Nâng character lên khỏi mặt bằng một chút
```

## TIPS KHI TEST:

1. **Ánh sáng tốt** → Test ở nơi sáng, tránh tối
2. **Giữ bằng phẳng** → Không cong, không nhăn
3. **Khoảng cách vừa phải** → 30-50cm từ camera đến bằng
4. **Di chuyển chậm** → Không quét nhanh

## KẾT QUẢ MONG ĐỢI:

✅ Tracking ổn định
✅ Character gắn chặt với bằng
✅ Ít jitter
✅ Không mất tracking khi di chuyển nhẹ

