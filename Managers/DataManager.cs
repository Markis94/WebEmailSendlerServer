using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using WebEmailSendler.Context;
using WebEmailSendler.Enums;
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

        public async Task<Sample> CreateSample(Sample samle)
        {
            await _context.Samles.AddAsync(samle);
            await _context.SaveChangesAsync();
            return samle;
        }

        public async Task<IList<Sample>> SampleList()
        {
            var result = await _context.Samles.AsNoTracking().OrderByDescending(x => x.CreateDate).ToListAsync();
            return result;
        }

        public async Task UpdateSample(Sample sample)
        {
            var upd = await _context.Samles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sample.Id);
            if (sample != null)
            {
                _context.Samles.Update(sample);
                _context.SaveChanges();
            }
        }

        public async Task DeleteSample(int sampleId)
        {
            var sample = await _context.Samles.FirstOrDefaultAsync(x => x.Id == sampleId);
            if (sample != null)
            {
                _context.Samles.Remove(sample);
                _context.SaveChanges();
            }
        }

        public async Task<Sample> SampleById(int sampleId)
        {
            var result = await _context.Samles.FirstAsync(x=>x.Id == sampleId);
            return result;
        }

        public async Task<EmailSendInfo> EmailSendTaskInfo(int sendTaskId)
        {
            EmailSendInfo result = new EmailSendInfo();
            result.MaxCount = await _context.EmailSendResults.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId).CountAsync();
            result.BadSendCount = await _context.EmailSendResults.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId && x.IsSuccess == false && x.ErrorMessage != null).CountAsync();
            result.SendCount = await _context.EmailSendResults.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId && x.IsSuccess == true && x.ErrorMessage == null).CountAsync();
            return result;
        }

        public async Task<List<EmailSendTask>> EmailSendTaskList(SendTaskStatusEnum status, DateTime leftDate, DateTime rightDate)
        {
            var result = await _context.EmailSendTask
                .AsNoTracking()
                .Where(x => x.CreateDate > DateTime.SpecifyKind(leftDate.Date, DateTimeKind.Utc)
                    && x.CreateDate < DateTime.SpecifyKind(rightDate.Date, DateTimeKind.Utc)
                    && x.SendTaskStatus == status.ToString())
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

        public async Task BulkUpdateEmailSendResult(List<EmailSendResult> emailSendResults)
        {
            await _context.BulkUpdateAsync(emailSendResults, bulkConfig =>
            {
                bulkConfig.BatchSize = 5000;
                bulkConfig.BulkCopyTimeout = 600000;
            });
        }
        public void UpdateEmailSendResult(List<EmailSendResult> emailSendResults)
        {
            _context.UpdateRange(emailSendResults);
            _context.SaveChanges();
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
                upd.SendTaskStatus = SendTaskStatusEnum.deleted.ToString();
                UpdateEmailSendTask(upd);
                if (upd != null)
                {
                    _context.EmailSendTask.Remove(upd);
                    _context.SaveChanges();
                }
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
