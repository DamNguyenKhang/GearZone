using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.User
{
    [Authorize]
    public class RegisterSellerModel : PageModel
    {
        private readonly ISellerStoreService _sellerStoreService;
        private readonly IAuthService _authService;

        public RegisterSellerModel(ISellerStoreService sellerStoreService, IAuthService authService)
        {
            _sellerStoreService = sellerStoreService;
            _authService = authService;
        }

        [BindProperty]
        public Step1Dto Step1Input { get; set; } = new();

        [BindProperty]
        public Step2Dto Step2Input { get; set; } = new();

        [BindProperty]
        public Step3Dto Step3Input { get; set; } = new();

        public int CurrentStep { get; set; } = 1;
        public Guid? StoreId { get; set; }
        public RegistrationProgressDto? Progress { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            // Check if user already has an approved/pending store
            var existingStore = await _sellerStoreService.GetStoreByOwnerIdAsync(user.Id);
            if (existingStore != null && existingStore.Status != StoreStatus.Draft)
            {
                if (existingStore.Status == StoreStatus.Approved)
                    return RedirectToPage("/StoreOwner/Dashboard");
                if (existingStore.Status == StoreStatus.Pending)
                {
                    TempData["InfoMessage"] = "Đơn đăng ký của bạn đang chờ duyệt.";
                    return RedirectToPage("/Public/User/Profile");
                }
            }

            // Load draft progress
            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress != null)
            {
                CurrentStep = Progress.CurrentStep;
                StoreId = Progress.StoreId;
                Step1Input = Progress.Step1;
                Step2Input = Progress.Step2;
                Step3Input = Progress.Step3;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostStep1Async()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            try
            {
                var storeId = await _sellerStoreService.SaveStep1Async(user.Id, Step1Input);
                return RedirectToPage(new { step = 2 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 1;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStep2Async()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress?.StoreId == null) return RedirectToPage();

            try
            {
                await _sellerStoreService.SaveStep2Async(Progress.StoreId.Value, user.Id, Step2Input);
                return RedirectToPage(new { step = 3 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 2;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStep3Async()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress?.StoreId == null) return RedirectToPage();

            try
            {
                await _sellerStoreService.SaveStep3Async(Progress.StoreId.Value, Step3Input);
                return RedirectToPage(new { step = 4 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 3;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Public/Auth/Login");

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress?.StoreId == null) return RedirectToPage();

            try
            {
                await _sellerStoreService.SubmitRegistrationAsync(Progress.StoreId.Value, user.Id);
                TempData["SuccessMessage"] = "Đơn đăng ký của bạn đã được gửi thành công! Chúng tôi sẽ xem xét và phản hồi sớm nhất.";
                return RedirectToPage("/Public/User/Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CurrentStep = 4;
                return Page();
            }
        }

        public async Task OnGetLoadStepAsync(int step)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null) return;

            Progress = await _sellerStoreService.GetRegistrationProgressAsync(user.Id);
            if (Progress != null)
            {
                StoreId = Progress.StoreId;
                Step1Input = Progress.Step1;
                Step2Input = Progress.Step2;
                Step3Input = Progress.Step3;
            }
            CurrentStep = step;
        }
    }
}
