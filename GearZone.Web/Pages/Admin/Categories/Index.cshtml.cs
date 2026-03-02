using AutoMapper;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Web.Pages.Admin.Categories.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Categories
{
    public class IndexModel : PageModel
    {
        private readonly IAdminCategoryService _adminCategoryService;
        private readonly IMapper _mapper;

        public IndexModel(IAdminCategoryService adminCategoryService, IMapper mapper)
        {
            _adminCategoryService = adminCategoryService;
            _mapper = mapper;
        }

        [BindProperty(SupportsGet = true)]
        public CategoryQueryDto Query { get; set; } = new CategoryQueryDto();

        [BindProperty]
        public CreateCategoryViewModel CreateCategoryRequest { get; set; } = new CreateCategoryViewModel();

        [BindProperty]
        public EditCategoryViewModel EditCategoryRequest { get; set; } = new EditCategoryViewModel();

        public PagedResult<CategoryDto> Categories { get; set; } = new PagedResult<CategoryDto>();
        public List<CategoryDto> AllCategories { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var otherKeys = ModelState.Keys
                .Where(k => !k.StartsWith(nameof(CreateCategoryRequest) + ".") && k != nameof(CreateCategoryRequest))
                .ToList();
            foreach (var key in otherKeys)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var category = _mapper.Map<Category>(CreateCategoryRequest);
            var result = await _adminCategoryService.CreateCategoryAsync(category);

            if (result)
            {
                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = "Failed to create category.";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var otherKeys = ModelState.Keys
                .Where(k => !k.StartsWith(nameof(EditCategoryRequest) + ".") && k != nameof(EditCategoryRequest))
                .ToList();
            foreach (var key in otherKeys)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Validation failed: " + string.Join(" ", errors);
                await LoadDataAsync();
                return Page();
            }

            var dto = _mapper.Map<EditCategoryDto>(EditCategoryRequest);
            var result = await _adminCategoryService.UpdateCategoryAsync(dto);

            if (result)
            {
                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = "Failed to update category.";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _adminCategoryService.SoftDeleteCategoryAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete category.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetCategoryDetailsAsync(int id)
        {
            var category = await _adminCategoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var viewModel = _mapper.Map<EditCategoryViewModel>(category);
            return new JsonResult(viewModel);
        }

        private async Task LoadDataAsync()
        {
            if (Query.PageNumber < 1) Query.PageNumber = 1;
            if (Query.PageSize < 1) Query.PageSize = 10;

            Categories = await _adminCategoryService.GetPaginatedCategoriesAsync(Query);
            AllCategories = await _adminCategoryService.GetAllCategoriesListAsync();
        }
    }
}
