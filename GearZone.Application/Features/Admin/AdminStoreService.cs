using AutoMapper;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin;

public class AdminStoreService : IAdminStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public AdminStoreService(IStoreRepository storeRepository, IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
    {
        _storeRepository = storeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<PagedResult<StoreApplicationDto>> GetStoreApplicationsAsync(StoreApplicationQueryDto query)
    {
        var pagedStores = await _storeRepository.GetStoreApplicationsAsync(query);

        return new PagedResult<StoreApplicationDto>
        {
            Items = _mapper.Map<List<StoreApplicationDto>>(pagedStores.Items),
            TotalCount = pagedStores.TotalCount,
            PageNumber = pagedStores.PageNumber,
            PageSize = pagedStores.PageSize
        };
    }

    public async Task<StoreApplicationDto?> GetStoreApplicationByIdAsync(Guid storeId)
    {
        var store = await _storeRepository.GetStoreApplicationByIdAsync(storeId);

        if (store == null)
            return null;

        return _mapper.Map<StoreApplicationDto>(store);
    }

    public async Task<bool> ApproveStoreAsync(Guid storeId)
    {
        var store = await _storeRepository.GetStoreApplicationByIdAsync(storeId);
        if (store == null || store.Status != StoreStatus.Pending)
            return false;

        store.Status = StoreStatus.Approved;
        store.ApprovedAt = DateTime.UtcNow;
        store.UpdatedAt = DateTime.UtcNow;

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        if (store.OwnerUser != null && !string.IsNullOrWhiteSpace(store.OwnerUser.Email))
        {
            var subject = "Your Store Application has been Approved!";
            var body = $@"
                <h3>Congratulations {store.OwnerUser.FullName},</h3>
                <p>We are thrilled to inform you that your store application for <strong>{store.StoreName}</strong> has been <strong>approved</strong>.</p>
                <p>You can now log in to the Seller Dashboard and start managing your store.</p>
                <br/>
                <p>Best regards,<br/>The GearZone Team</p>
            ";
            await _emailService.SendAsync(store.OwnerUser.Email, subject, body);
        }

        return true;
    }

    public async Task<bool> RejectStoreAsync(Guid storeId, string reason)
    {
        var store = await _storeRepository.GetStoreApplicationByIdAsync(storeId);
        if (store == null || store.Status != StoreStatus.Pending)
            return false;

        store.Status = StoreStatus.Rejected;
        store.RejectReason = reason;
        store.UpdatedAt = DateTime.UtcNow;

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        if (store.OwnerUser != null && !string.IsNullOrWhiteSpace(store.OwnerUser.Email))
        {
            var subject = "Your Store Application Status";
            var body = $@"
                <h3>Dear {store.OwnerUser.FullName},</h3>
                <p>Thank you for applying to become a seller on GearZone with your store <strong>{store.StoreName}</strong>.</p>
                <p>After careful review, we regret to inform you that your application has been <strong>rejected</strong> at this time.</p>
                <p><strong>Reason for rejection:</strong><br/>{reason}</p>
                <br/>
                <p>If you have any questions or would like to submit additional information, please reply to this email.</p>
                <br/>
                <p>Best regards,<br/>The GearZone Team</p>
            ";
            await _emailService.SendAsync(store.OwnerUser.Email, subject, body);
        }

        return true;
    }

    public async Task<bool> RequestInfoAsync(Guid storeId, string note)
    {
        var store = await _storeRepository.GetStoreApplicationByIdAsync(storeId);
        if (store == null || store.Status != StoreStatus.Pending)
            return false;

        store.UpdatedAt = DateTime.UtcNow;

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        if (store.OwnerUser != null && !string.IsNullOrWhiteSpace(store.OwnerUser.Email))
        {
            var subject = "Action Required: Additional Information Needed for Your Store Application";
            var body = $@"
                <h3>Dear {store.OwnerUser.FullName},</h3>
                <p>We are reviewing your application to become a seller on GearZone with your store <strong>{store.StoreName}</strong>.</p>
                <p>To proceed with your application, we need some additional information or clarification:</p>
                <p><strong>Note from Admin:</strong><br/>{note}</p>
                <br/>
                <p>Please reply to this email with the requested information at your earliest convenience.</p>
                <br/>
                <p>Best regards,<br/>The GearZone Team</p>
            ";
            await _emailService.SendAsync(store.OwnerUser.Email, subject, body);
        }

        return true;
    }

    public async Task<StoreApplicationStatsDto> GetStoreApplicationStatsAsync()
    {
        return await _storeRepository.GetStoreApplicationStatsAsync();
    }
}
