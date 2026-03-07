# 📋 Quy Trình Đăng Ký Cửa Hàng Trên GearZone

> Quy trình đăng ký 4 bước, gọn nhẹ nhưng đầy đủ thông tin cần thiết.

---

## 🗺️ Tổng Quan

```
Bước 1             Bước 2             Bước 3             Bước 4
Thông Tin       →  Xác Minh        →  Thanh Toán      →  Xem Lại
Cửa Hàng           Danh Tính          & Ngân Hàng        & Gửi Đơn
```

---

## 📌 Bước 1: Thông Tin Cửa Hàng

| Trường | Bắt buộc | Mô tả |
|--------|----------|-------|
| **Tên cửa hàng** | ✅ | Tên hiển thị trên GearZone |
| **Loại hình kinh doanh** | ✅ | Cá nhân / Hộ kinh doanh / Doanh nghiệp |
| **Số điện thoại** | ✅ | SĐT hỗ trợ khách hàng |
| **Email liên hệ** | ✅ | Email cho việc kinh doanh |
| **Địa chỉ** | ✅ | Địa chỉ kho / cửa hàng |
| **Tỉnh/Thành phố** | ✅ | Chọn từ danh sách |

> Dữ liệu lưu vào bảng `Store` (đã có sẵn đầy đủ các trường này).

---

## 📌 Bước 2: Xác Minh Danh Tính

| Trường | Bắt buộc | Mô tả |
|--------|----------|-------|
| **Họ và tên** | ✅ | Tên trên giấy tờ tuỳ thân |
| **Số CCCD** | ✅ | 12 chữ số |
| **Ngày cấp** | ✅ | Ngày cấp CCCD |
| **Nơi cấp** | ✅ | Cơ quan cấp |
| **Mã số thuế** | ✅ | MST cá nhân hoặc doanh nghiệp (10 hoặc 13 số) |

> Dữ liệu lưu vào `ApplicationUser` (IdentityNumber, IdentityIssuedDate, IdentityIssuedPlace — đã có sẵn) và `Store.TaxCode` (đã có sẵn).

---

## 📌 Bước 3: Thông Tin Thanh Toán

| Trường | Bắt buộc | Mô tả |
|--------|----------|-------|
| **Tên ngân hàng** | ✅ | Chọn từ danh sách (Vietcombank, BIDV, VPBank...) |
| **Số tài khoản** | ✅ | STK ngân hàng |
| **Tên chủ tài khoản** | ✅ | Tên trên tài khoản (nên khớp CCCD) |

> Dữ liệu lưu vào bảng `Store` (BankName, BankAccountNumber, BankAccountName — đã có sẵn).

---

## 📌 Bước 4: Xem Lại & Gửi Đơn

Hiển thị tóm tắt toàn bộ thông tin từ 3 bước trước:

- **Thông tin cửa hàng**: Tên, loại hình, SĐT, email, địa chỉ
- **Xác minh danh tính**: Họ tên, CCCD, MST
- **Thanh toán**: Ngân hàng, STK, chủ TK

Hành động:
- ✏️ **Quay lại sửa**: Click vào bất kỳ bước nào trên Stepper để chỉnh sửa.
- ☑️ **Đồng ý điều khoản**: Tick checkbox xác nhận.
- 📤 **Gửi đơn**: Submit → Trạng thái chuyển sang `Pending`.

---

## 🔄 Quy Trình Duyệt (Admin)

```
  Pending  ──→  Approved  (Duyệt → gán role "Store Owner" + gửi email)
     │
     ├──→  Rejected  (Từ chối + lý do → gửi email)
     │
     └──→  Request Info  (Yêu cầu bổ sung thông tin → gửi email)
                 │
                 └──→  Pending  (Seller bổ sung → quay lại chờ duyệt)
```

> Phần Admin Review **đã có sẵn** và hoạt động đầy đủ (Approve, Reject, RequestInfo + Email).

---

## 🏗️ Thay Đổi Cần Thực Hiện

### Không cần thay đổi gì ở Domain / Database

Tất cả các trường cần thiết **đã tồn tại sẵn** trong Entity:

- `Store`: StoreName, BusinessType, Phone, Email, AddressLine, Province, TaxCode, BankName, BankAccountNumber, BankAccountName, Status
- `ApplicationUser`: FullName, IdentityNumber, IdentityIssuedDate, IdentityIssuedPlace

### Chỉ cần thay đổi ở tầng Web

| Hạng mục | Mô tả |
|----------|-------|
| **Thêm `RegistrationStep`** vào `Store` entity | Để tracking bước hiện tại (int, default 1). Chỉ cần 1 cột mới duy nhất. |
| **Enum `StoreStatus`** | Thêm `Draft` (đơn đang điền dở, chưa submit) |
| **DTO `StoreRegistrationDto`** | Bổ sung các trường ngân hàng + danh tính |
| **Service `ISellerStoreService`** | Thêm `SaveStepAsync()` và `GetRegistrationProgressAsync()` |
| **UI `RegisterSeller.cshtml`** | Chuyển từ 1 form đơn → multi-step form với Stepper |

---

## 🎯 Thứ Tự Triển Khai

1. **Domain**: Thêm `RegistrationStep` vào `Store` + `Draft` vào `StoreStatus` → Migration.
2. **Application**: Cập nhật DTO + Service (lưu từng bước).
3. **Web**: Xây UI multi-step stepper (4 bước) + JavaScript xử lý chuyển bước.
4. **Testing**: Kiểm tra flow hoàn chỉnh từ đăng ký → Admin duyệt.

---

## 📊 So Sánh: Hiện Tại vs. Đề Xuất

| Hạng mục | Hiện tại | Đề xuất |
|----------|----------|---------|
| **Số bước** | 1 | 4 |
| **Thông tin thu thập** | Tên, MST, SĐT, Email, Địa chỉ | + Loại hình KD, CCCD, Ngân hàng |
| **Xác minh danh tính** | ❌ | ✅ (CCCD) |
| **Thông tin ngân hàng** | ❌ | ✅ |
| **Stepper UI** | ❌ | ✅ |
| **Lưu nháp giữa bước** | ❌ | ✅ |
| **Thay đổi DB** | — | Chỉ 1 cột mới + 1 enum value |

---

*Tài liệu cập nhật ngày 07/03/2026 cho dự án GearZone.*
