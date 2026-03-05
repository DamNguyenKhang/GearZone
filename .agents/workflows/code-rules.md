---
description: GearZone coding rules - áp dụng cho TẤT CẢ code mới trong dự án
---

// turbo-all

# GearZone Code Rules

Khi code cho dự án GearZone, LUÔN tuân thủ các quy tắc sau:

## Kiến trúc Clean Architecture 4 Layer
- **Domain** → Entities + Enums (KHÔNG phụ thuộc layer khác)
- **Application** → Services + DTOs + Interfaces (CHỈ phụ thuộc Domain)
- **Infrastructure** → Repositories + EF Core + External Services (implements Application)
- **Web** → Razor Pages + ViewModels (presentation)

## Luồng Code
```
PageModel → Service (interface) → Repository (interface) → EF Core → DB
```
- PageModel KHÔNG được gọi Repository trực tiếp
- Service KHÔNG được gọi DbContext trực tiếp
- LUÔN commit qua IUnitOfWork.SaveChangesAsync()

## Khi Thêm Tính Năng Mới, tạo theo thứ tự:
1. Domain: Entity (kế thừa `Entity<TKey>`) + Enum
2. Application: IRepository → IService → DTOs → Mappings → Service impl → DI
3. Infrastructure: Repository impl → Configuration → DbSet → DI
4. Web: ViewModels (có validation) → PageModel → Razor Page

## Quy tắc Quan Trọng:
- **DTO** (Application layer): POCO không validation, trong `Features/{Feature}/Dtos/`
- **ViewModel** (Web layer): CÓ DataAnnotations, trong `Pages/{Area}/{Feature}/Models/`
- **Soft Delete**: Dùng `IsDeleted` flag, KHÔNG xóa cứng
- **TempData**: `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]` cho toast
- **PRG Pattern**: Post thành công → RedirectToPage(), thất bại → return Page()
- **Projection**: Dùng `.Select()` thay vì `.Include()` cho list queries
- **AsNoTracking**: Luôn dùng cho read-only queries

## KHÔNG Duplicate Code:
- **Trước khi tạo mới** → LUÔN kiểm tra đã có sẵn chưa (DTO, partial, service method)
- **Tái sử dụng** partial views, DTOs, service methods hiện có thay vì tạo mới
- **Thêm method** vào interface/service/repo đã có thay vì tạo class mới
- **Dùng chung** _ProductCard, _ProductGridPartial, CatalogProductDto cho mọi nơi hiển thị sản phẩm

## Naming:
- Entity: PascalCase, singular (`Product`, `OrderItem`)
- Interface: `I` prefix (`IProductRepository`, `ICatalogService`)
- DTO: `{Entity}Dto`, `Create{Entity}Dto`, `{Entity}QueryDto`
- ViewModel: `{Action}{Entity}ViewModel`
- Service: `{Feature}Service`
- Repository: `{Entity}Repository`
