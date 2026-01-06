using AutoMapper;
using Microsoft.Extensions.Logging;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;
using norviguet_control_fletes_api.Repositories;

namespace norviguet_control_fletes_api.Services
{
    public class CarrierService : ICarrierService
    {
        private readonly ICarrierRepository _carrierRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CarrierService> _logger;

        public CarrierService(
            ICarrierRepository carrierRepository, 
            IMapper mapper, 
            ILogger<CarrierService> logger)
        {
            _carrierRepository = carrierRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<CarrierDto>>> GetAllCarriersAsync()
        {
            try
            {
                var carriers = await _carrierRepository.GetAllAsync();
                var result = _mapper.Map<List<CarrierDto>>(carriers);
                return Result<List<CarrierDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all carriers");
                return Result<List<CarrierDto>>.Failure("Error retrieving carriers", "GET_ALL_ERROR");
            }
        }

        public async Task<Result<CarrierDto>> GetCarrierByIdAsync(int id)
        {
            try
            {
                var carrier = await _carrierRepository.GetByIdAsync(id);
                
                if (carrier == null)
                {
                    _logger.LogWarning("Carrier with ID {CarrierId} not found", id);
                    return Result<CarrierDto>.Failure("Carrier not found", "NOT_FOUND");
                }

                var result = _mapper.Map<CarrierDto>(carrier);
                return Result<CarrierDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving carrier with ID {CarrierId}", id);
                return Result<CarrierDto>.Failure("Error retrieving carrier", "GET_ERROR");
            }
        }

        public async Task<Result<CarrierDto>> CreateCarrierAsync(CreateCarrierDto dto)
        {
            try
            {
                var carrier = _mapper.Map<Carrier>(dto);
                await _carrierRepository.AddAsync(carrier);
                await _carrierRepository.SaveChangesAsync();

                var result = _mapper.Map<CarrierDto>(carrier);
                _logger.LogInformation("Carrier created with ID {CarrierId}", carrier.Id);
                
                return Result<CarrierDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating carrier");
                return Result<CarrierDto>.Failure("Error creating carrier", "CREATE_ERROR");
            }
        }

        public async Task<Result<CarrierDto>> UpdateCarrierAsync(int id, UpdateCarrierDto dto)
        {
            try
            {
                var carrier = await _carrierRepository.GetByIdAsync(id);
                
                if (carrier == null)
                {
                    _logger.LogWarning("Carrier with ID {CarrierId} not found for update", id);
                    return Result<CarrierDto>.Failure("Carrier not found", "NOT_FOUND");
                }

                _mapper.Map(dto, carrier);
                await _carrierRepository.UpdateAsync(carrier);
                await _carrierRepository.SaveChangesAsync();

                var result = _mapper.Map<CarrierDto>(carrier);
                _logger.LogInformation("Carrier with ID {CarrierId} updated", id);
                
                return Result<CarrierDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating carrier with ID {CarrierId}", id);
                return Result<CarrierDto>.Failure("Error updating carrier", "UPDATE_ERROR");
            }
        }

        public async Task<Result<bool>> DeleteCarrierAsync(int id)
        {
            try
            {
                var carrier = await _carrierRepository.GetByIdWithRelationsAsync(id);
                
                if (carrier == null)
                {
                    _logger.LogWarning("Carrier with ID {CarrierId} not found for deletion", id);
                    return Result<bool>.Failure("Carrier not found", "NOT_FOUND");
                }

                if (HasAssociatedRecords(carrier))
                {
                    _logger.LogWarning("Cannot delete carrier with ID {CarrierId} due to associated records", id);
                    return Result<bool>.Failure(
                        "Carrier cannot be deleted because it has associated records", 
                        "ASSOCIATED_RECORDS");
                }

                await _carrierRepository.DeleteAsync(carrier);
                await _carrierRepository.SaveChangesAsync();
                
                _logger.LogInformation("Carrier with ID {CarrierId} deleted", id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting carrier with ID {CarrierId}", id);
                return Result<bool>.Failure("Error deleting carrier", "DELETE_ERROR");
            }
        }

        public async Task<Result<bool>> DeleteCarriersBulkAsync(List<int> ids)
        {
            try
            {
                var carriers = await _carrierRepository.GetByIdsWithRelationsAsync(ids);
                
                if (carriers.Count == 0)
                {
                    _logger.LogWarning("No carriers found for bulk deletion");
                    return Result<bool>.Failure("No carriers found", "NOT_FOUND");
                }

                var carriersWithAssociations = carriers.Where(HasAssociatedRecords).ToList();
                
                if (carriersWithAssociations.Any())
                {
                    _logger.LogWarning("Cannot delete {Count} carriers due to associated records", 
                        carriersWithAssociations.Count);
                    return Result<bool>.Failure(
                        "Some carriers could not be deleted because they have associated records", 
                        "ASSOCIATED_RECORDS");
                }

                await _carrierRepository.DeleteRangeAsync(carriers);
                await _carrierRepository.SaveChangesAsync();
                
                _logger.LogInformation("Bulk deleted {Count} carriers", carriers.Count);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk delete carriers");
                return Result<bool>.Failure("Error deleting carriers", "BULK_DELETE_ERROR");
            }
        }

        public async Task<Result<List<CarrierDto>>> GetCarriersWithoutInvoicesByOrderIdAsync(int orderId)
        {
            try
            {
                var carriers = await _carrierRepository.GetCarriersWithoutInvoicesByOrderIdAsync(orderId);
                var result = _mapper.Map<List<CarrierDto>>(carriers);
                return Result<List<CarrierDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving carriers without invoices for order {OrderId}", orderId);
                return Result<List<CarrierDto>>.Failure("Error retrieving carriers", "GET_ERROR");
            }
        }

        public async Task<Result<List<CarrierDto>>> GetCarriersWithoutPaymentOrdersByOrderIdAsync(int orderId)
        {
            try
            {
                var carriers = await _carrierRepository.GetCarriersWithoutPaymentOrdersByOrderIdAsync(orderId);
                var result = _mapper.Map<List<CarrierDto>>(carriers);
                return Result<List<CarrierDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving carriers without payment orders for order {OrderId}", orderId);
                return Result<List<CarrierDto>>.Failure("Error retrieving carriers", "GET_ERROR");
            }
        }

        private static bool HasAssociatedRecords(Carrier carrier)
        {
            return (carrier.DeliveryNotes?.Any() == true) ||
                   (carrier.PaymentOrders?.Any() == true) ||
                   (carrier.Invoices?.Any() == true);
        }
    }
}
