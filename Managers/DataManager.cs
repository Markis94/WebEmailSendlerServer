using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using WebEmailSendler.Context;
using WebEmailSendler.Models;

namespace WebEmailSendler.Managers
{
    public class DataManager
    {
        private readonly MyDbContext _context;
        public DataManager(MyDbContext context)
        {
            _context = context;
        }

        public async Task<EmailSendInfo> EmailSendTaskInfo(int sendTaskId)
        {
            EmailSendInfo result = new EmailSendInfo();
            result.MaxCount = await _context.EmailSendResults.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId).CountAsync();
            result.BadSendCount = await _context.EmailSendResults.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId && x.IsSuccess == false).CountAsync();
            result.SendCount = await _context.EmailSendResults.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId && x.IsSuccess == true && x.ErrorMessage == null).CountAsync();
            return result;
        }

        public async Task<List<EmailSendTask>> EmailSendTaskList(SendTaskStatusEnum status)
        {
            var result = await _context.EmailSendTask
                .AsNoTracking()
                .Where(x => x.SendTaskStatus == status.ToString())
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
            return result;
        }

        public async Task<List<EmailSendTask>> EmailSendTaskList()
        {
            var result = await _context.EmailSendTask.OrderByDescending(x => x.CreateDate).ToListAsync();
            return result;
        }

        public async Task<int> CreateEmailSendTask(EmailSendTask emailSendTask)
        {
            await _context.EmailSendTask.AddAsync(emailSendTask);
            await _context.SaveChangesAsync();
            return emailSendTask.Id;
        }

        public async Task CreateEmailSendResult(List<EmailSendResult> emailSendResults)
        {
            await _context.BulkInsertAsync(emailSendResults, bulkConfig =>
            {
                bulkConfig.BatchSize = 5000;
                bulkConfig.BulkCopyTimeout = 600000;
            });
        }

        public async Task<EmailSendTask?> GetEmailSendTask(int sendlerEmailTaskId)
        {
            var result = await _context.EmailSendTask.FirstOrDefaultAsync(x => x.Id == sendlerEmailTaskId);
            return result;
        }

        public async Task<List<EmailSendResult>> GetEmailSendResult(int sendlerEmailTaskId)
        {
            var result = await _context.EmailSendResults.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendlerEmailTaskId && x.IsSuccess != true).ToListAsync();
            return result;
        }

        public async Task UpdateEmailSendResult(List<EmailSendResult> emailSendResults)
        {
            await _context.BulkUpdateAsync(emailSendResults, bulkConfig =>
            {
                bulkConfig.BatchSize = 5000;
                bulkConfig.BulkCopyTimeout = 600000;
            });
        }

        public async Task UpdateEmailSendTaskJobId(string jobId, int sendEmailTaskId)
        {
            var upd = await _context.EmailSendTask.FirstOrDefaultAsync(x => x.Id == sendEmailTaskId);
            if (upd != null)
            {
                upd.JobId = jobId;
                await _context.SaveChangesAsync();
            }
        }

        public void UpdateEmailSendTask(EmailSendTask emailSendTask)
        {
            _context.EmailSendTask.Update(emailSendTask);
            _context.SaveChanges();
        }

        public async Task DeleteEmailSendTask(int sendEmailTaskId)
        {
            var upd = await _context.EmailSendTask.FirstOrDefaultAsync(x => x.Id == sendEmailTaskId);
            if (upd != null)
            {
                upd.SendTaskStatus = "deleted";
                UpdateEmailSendTask(upd);
                if (upd != null)
                {
                    _context.EmailSendTask.Remove(upd);
                    _context.SaveChanges();
                }
            }
        }

        public async Task CancelEmailSendTask(string jobId)
        {
            var upd = await _context.EmailSendTask.FirstOrDefaultAsync(x => x.JobId == jobId);
            if (upd != null)
            {
                upd.SendTaskStatus = SendTaskStatusEnum.complete.ToString();
                UpdateEmailSendTask(upd);
            }
        }

        public Part<EmailSendResult> EmailResultPath(string? inputValue, int sendTaskId, int pageNumber, int pageSize, string? sortField, string? orderBy)
        {
            IQueryable<EmailSendResult> data;
            if (inputValue?.Length > 1)
            {
                data =
                   _context.EmailSendResults.AsNoTracking().AsQueryable()
                   .Where(x => x.EmailSendTaskId == sendTaskId && x.Email.Contains(inputValue))
                   .ToPageList(pageNumber, pageSize, sortField, orderBy);
            }
            else
            {
                data =
                   _context.EmailSendResults.AsNoTracking().AsQueryable()
                   .Where(x => x.EmailSendTaskId == sendTaskId)
                   .ToPageList(pageNumber, pageSize, sortField, orderBy);
            }
            var count = _context.EmailSendResults
                .AsNoTracking()
                .Where(x => x.EmailSendTaskId == sendTaskId)
                .Count();

            var resultPart = new Part<EmailSendResult>()
            {
                TotalCount = count,
                Items = data.ToList()
            };
            return resultPart;
        }
    }
}
