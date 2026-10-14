# AppHider

**AppHider** là tiện ích portable nhỏ gọn, hiện đại và mạnh mẽ trên Windows giúp bạn quản lý hiển thị, ẩn ứng dụng và bảo vệ sự riêng tư tức thì chỉ với một thao tác.

---

## ⚡ Phím tắt nhanh (Boss Key)

- **Phím tắt mặc định:** `Ctrl + F12`
- **Chức năng:** Khi nhấn tổ hợp phím này ở bất cứ đâu trong hệ thống:
  - Ngay lập tức **khôi phục tất cả ứng dụng đang bị ẩn** (chế độ ẩn ứng dụng, ẩn taskbar hoặc đang thu nhỏ).
  - Tự động đưa các cửa sổ ứng dụng lên vị trí tiền cảnh (Foreground).
  - **Bật lại âm thanh** ngay lập tức cho các ứng dụng đã bị tắt tiếng.
- **Tùy biến:** Bạn có thể bấm vào ô phím tắt trong phần **Cài đặt chung** và nhấn bất kỳ tổ hợp phím nào mong muốn (ví dụ: `Ctrl + Alt + H`, `Alt + Q`,...).

---

## 🚀 Hướng dẫn sử dụng

1. **Khởi động**: Chạy file `AppHider.exe` (ứng dụng chạy trực tiếp không cần cài đặt). Biểu tượng AppHider sẽ xuất hiện trên khay hệ thống (System Tray).
2. **Chọn chế độ cho từng ứng dụng**: Danh sách tất cả các ứng dụng đang mở sẽ được hiển thị. Nhấp vào menu chọn cạnh mỗi ứng dụng:
   - **Chế độ bình thường**: Ứng dụng hoạt động bình thường, không can thiệp.
   - **Chế độ: Ẩn ứng dụng**: Khi bạn chuyển sang cửa sổ khác (mất tiêu điểm/blur), ứng dụng này sẽ **hoàn toàn tàng hình** khỏi màn hình chính, thanh Taskbar và danh sách Alt+Tab. Một icon đại diện sẽ xuất hiện ở khay hệ thống để bạn có thể mở lại khi cần.
   - **Chế độ: Ẩn taskbar (không tự ẩn cửa sổ ứng dụng)**: Chỉ xóa biểu tượng của ứng dụng khỏi thanh Taskbar để thanh tác vụ luôn gọn gàng, nhưng cửa sổ vẫn hiển thị bình thường trên màn hình.
3. **Tìm kiếm & Lọc**: Sử dụng thanh tìm kiếm phía trên danh sách để lọc nhanh ứng dụng cần cài đặt.
4. **Cài đặt nâng cao (Cài đặt chung)**:
   - **Tắt tiếng khi thu nhỏ / ẩn ứng dụng**: Tự động ngắt tiếng ứng dụng khi bị thu nhỏ hoặc bị ẩn và tự động khôi phục âm thanh khi mở lại.
   - **Khung giờ hoạt động**: Đặt thời gian (Từ ... Đến ...) để AppHider chỉ áp dụng tự động ẩn và tắt tiếng trong khung giờ quy định (ví dụ: giờ làm việc).
   - **Khởi động cùng Windows**: Tự động chạy AppHider ngầm khi bật máy.
   - **Hiện khi khởi động**: Quyết định hiển thị bảng điều khiển khi mở máy hay tự động thu xuống khay hệ thống.
5. **Lưu cấu hình**: Bấm nút **Lưu và thu xuống khay hệ thống**. AppHider sẽ ghi nhớ cài đặt vào `config.ini` và tiếp tục hoạt động ngầm.

---

## 📌 Khay hệ thống (System Tray)

- **Nhấp đúp vào icon AppHider**: Mở lại bảng điều khiển chính.
- **Nhấp chuột phải vào icon AppHider**:
  - *Mở cửa sổ cài đặt*
  - *Khôi phục tất cả ứng dụng (`Ctrl + F12`)*
  - *Thoát hoàn toàn*
- **Các icon ứng dụng con bị ẩn**: Nhấp chuột trái vào icon ứng dụng tương ứng ở khay hệ thống để khôi phục nhanh cửa sổ đó.

---

## ✨ Tính năng nổi bật

- **Boss Key siêu tốc**: Phím tắt toàn hệ thống `Ctrl + F12` mở lại mọi ứng dụng ẩn chỉ trong tích tắc.
- **Cơ chế ẩn toàn diện**: Biến mất hoàn toàn khỏi màn hình Desktop, Taskbar và Alt+Tab.
- **Quản lý âm thanh thông minh**: Mute process trực tiếp qua Core Audio API khi ẩn/thu nhỏ và tự unmute khi quay lại.
- **Lập lịch linh hoạt**: Chỉ kích hoạt tính năng trong khung giờ làm việc mong muốn.
- **Giao diện Windows 11 Fluent**: Hỗ trợ Dark Mode / Light Mode và chuyển đổi ngôn ngữ (Tiếng Việt / English).
- **100% Portable**: Dung lượng nhẹ, không cần cài đặt, không rác registry hệ thống.

---

## 📥 Tải về

- Tải bản nén mới nhất: **[AppHider.zip](https://github.com/pytago02/AppHider/raw/main/AppHider.zip)**
- Giải nén file `.zip` và chạy file `AppHider.exe`.

---

## 👨‍💻 Tác giả

© Kkmt
