using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Services.BackgroundWorker;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BackgroundWorkerController : ControllerBase, IDisposable
    {
        private readonly ILogger<DataGridController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBackgroundWorkers _backgroundWorkers;

        public BackgroundWorkerController(ILogger<DataGridController> logger,
            ApplicationDbContext context, IMapper mapper, IBackgroundWorkers backgroundWorkers)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _backgroundWorkers = backgroundWorkers;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private async Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, BackgroundTaskType type)
        {
            var result = await _backgroundWorkers.ActivateBackgroundWorkersAsync(activate, type);

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServicesStatusAsync(ApiCrudPayload<ServicesStatus> status)
        {
            try
            {
                _context.Entry(status.EntityValue!).State = EntityState.Modified;
                await SaveDbAsync(status.UserName);
                _context.Entry(status.EntityValue!).State = EntityState.Detached;
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        private async Task<SubmitResult> UpdateServicesAsync(ServicesStatus item, string? userName)
        {
            try
            {
                _context.Entry(item).State = EntityState.Modified;
                await SaveDbAsync(userName);
                _context.Entry(item).State = EntityState.Detached;
                return new SubmitResult { Success = true };
            }
            catch (Exception ex)
            {
                return new SubmitResult { Success = false, Message = ex.Message };
            }
        }

        private async Task<ServicesStatus> GetServiceStatusAsync()
        {
            return (await _context.ServicesStatus.OrderBy(a => a.Id).FirstOrDefaultAsync())!;
        }

        [HttpPost]
        public async Task<IActionResult> ActivateReportService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();
            item.ReportService = value.EntityValue!.Activate;
            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.Report);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> ActivateAlertService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();
            item.AlertService = value.EntityValue!.Activate;
            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.Alert);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> ActivateDataTransferService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();
            item.DataTransferService = value.EntityValue!.Activate;
            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.DataTransfer);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ActivateCleanerService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();

            item.CleanerService = value.EntityValue.Activate;

            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.Cleaner);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePerActivity(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            await _backgroundWorkers.SwitchBackgroundTasksPerActivityAsync(value.EntityValue!.Value, value.EntityValue.Activate);
            return Ok(new SubmitResult { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePerTask(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            await _backgroundWorkers.SwitchBackgroundTaskAsync(value.EntityValue!.Value, value.EntityValue.Activate);
            return Ok(new SubmitResult { Success = true });
        }

        [HttpPost]
        public IActionResult RunManually(ApiCrudPayload<RunTaskManually> value)
        {
            _backgroundWorkers.RunManuallyTask(value.EntityValue!.TaskHeaderId, value.UserName, value.EntityValue.Emails!, value.EntityValue.CustomQueryParameters!, value.EntityValue.GenerateFiles);
            return Ok(new SubmitResult { Success = true });
        }

        private async Task SaveDbAsync(string? userId = "system")
        {
            await _context.SaveChangesAsync(userId);
        }
    }
}
