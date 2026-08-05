# Hướng dẫn sử dụng các tính năng mới

Mở website tại `http://localhost:8080`. Tài khoản Admin mặc định là `admin` / `Admin123@`.

## 1. Tạo tài khoản Staff hoặc Trainer

Chỉ Admin thực hiện được.

1. Đăng nhập bằng tài khoản Admin.
2. Chọn **Quản lý nhân viên** hoặc **Quản lý huấn luyện viên** ở sidebar.
3. Bấm **Tạo tài khoản**.
4. Kiểm tra Role đã được chọn đúng là **Staff** hoặc **Trainer**.
5. Nhập họ tên, username, mật khẩu, số điện thoại và email. Với Trainer có thể nhập thêm chuyên môn, chứng chỉ và ảnh chứng chỉ.
6. Bấm lưu, sau đó gửi username/mật khẩu cho nhân sự tương ứng.

Tài khoản mẫu để thử nhanh:

| Vai trò | Username | Password |
|---|---|---|
| Staff | `staffdemo` | `Staff123@` |
| Trainer | `trainerdemo` | `Trainer123@` |

## 2. Xem và tự đăng ký ca làm việc

Dành cho Staff và Trainer.

1. Đăng nhập bằng tài khoản Staff hoặc Trainer.
2. Sidebar chỉ hiển thị **Lịch làm việc của tôi**. Trang này liệt kê các ca hiện có của chính tài khoản đang đăng nhập.
3. Bấm **Đăng ký ca**.
4. Chọn phòng tập, ngày làm việc, giờ bắt đầu và giờ kết thúc.
5. Bấm **Đăng ký ca** để lưu.

Hệ thống chỉ nhận ca trong tương lai. Ca bị từ chối nếu giờ kết thúc không sau giờ bắt đầu, tài khoản đó đã có ca chồng giờ, hoặc phòng đã được một người khác sử dụng trong cùng khoảng thời gian. Hai ca liền nhau, ví dụ 08:00–10:00 và 10:00–12:00, được phép.

Admin vẫn có thể tạo/sửa lịch trong **Quản lý lịch tập**. Hệ thống áp dụng cùng quy tắc chống trùng cho cả Admin, Staff và Trainer.

## 3. Đặt phòng tập

### Hội viên tạo yêu cầu

1. Đăng nhập bằng tài khoản hội viên.
2. Mở menu tên tài khoản ở góc trên phải, chọn **Đặt phòng tập**.
3. Chọn phòng, thời gian bắt đầu/kết thúc và ghi chú nếu cần.
4. Bấm **Gửi yêu cầu**.
5. Vào **Lịch đặt phòng của tôi** để theo dõi trạng thái hoặc hủy lịch chưa bắt đầu.

Trạng thái gồm **Chờ duyệt**, **Đã duyệt**, **Từ chối** và **Đã hủy**.

### Staff/Admin xử lý yêu cầu

1. Đăng nhập bằng Staff hoặc Admin.
2. Chọn **Quản lý đặt phòng** ở sidebar.
3. Với yêu cầu **Chờ duyệt**, bấm **Duyệt** hoặc **Từ chối**.

Không thể duyệt hai booking cùng phòng có khoảng thời gian chồng nhau. Hội viên cũng không thể tạo booking trùng với một booking đã được duyệt.

## 4. Khi gặp lỗi quyền truy cập

Mỗi vai trò chỉ thấy các chức năng được cấp quyền. Nếu bị đưa về trang đăng nhập, hãy kiểm tra bạn đã đăng nhập đúng vai trò:

- **Admin**: quản lý toàn bộ hệ thống.
- **Staff**: quản lý hội viên, thanh toán, booking và lịch cá nhân.
- **Trainer**: lịch cá nhân và đăng ký ca.
- **Member**: hồ sơ, gói tập, thanh toán, lịch tập và booking của chính mình.
