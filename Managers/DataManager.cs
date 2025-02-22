﻿using EFCore.BulkExtensions;
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
            var result = await _context.Samles.AsNoTracking()
                .Select(x => new Sample() { Id = x.Id, ChangeDate = x.ChangeDate, CreateDate = x.CreateDate, Name = x.Name, HtmlString = x.HtmlString, JsonString = "" })
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
            return result;
        }

        public async Task UpdateSample(Sample sample)
        {
            var upd = await _context.Samles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sample.Id);
            if (upd != null)
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
            var result = await _context.Samles.FirstAsync(x => x.Id == sampleId);
            return result;
        }

        public async Task<SendInfo> EmailSendTaskInfo(int sendTaskId)
        {
            SendInfo result = new SendInfo();
            result.MaxCount = await _context.EmailSendData.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId).CountAsync();
            result.BadSendCount = await _context.EmailSendData.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId && x.IsSuccess == false && x.ErrorMessage != null).CountAsync();
            result.SuccessSendCount = await _context.EmailSendData.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendTaskId && x.IsSuccess == true && x.ErrorMessage == null).CountAsync();
            return result;
        }

        public async Task<List<EmailSendTask>> EmailSendTaskList(SendTaskStatusEnum status, DateTime leftDate, DateTime rightDate)
        {
            var result = await _context.EmailSendTask
                .AsNoTracking()
                .Where(x => x.CreateDate >= DateTime.SpecifyKind(leftDate.Date, DateTimeKind.Utc)
                    && x.CreateDate <= DateTime.SpecifyKind(rightDate.Date, DateTimeKind.Utc).AddDays(1)
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

        public async Task CreateEmailSendResult(List<EmailSendData> emailSendResults)
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

        public async Task<List<EmailSendData>> GetEmailSendResult(int sendlerEmailTaskId)
        {
            var result = await _context.EmailSendData.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendlerEmailTaskId).ToListAsync();
            return result;
        }
        /// <summary>
        /// отдает список Email для отправки где статус != status
        /// </summary>
        /// <param name="sendlerEmailTaskId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<List<EmailSendData>> GetEmailSendResult(int sendlerEmailTaskId, bool status)
        {
            var result = await _context.EmailSendData.AsQueryable().AsNoTracking().Where(x => x.EmailSendTaskId == sendlerEmailTaskId && x.IsSuccess != status).ToListAsync();
            return result;
        }

        public async Task BulkUpdateEmailSendResult(List<EmailSendData> emailSendResults)
        {
            await _context.BulkUpdateAsync(emailSendResults, bulkConfig =>
            {
                bulkConfig.BatchSize = 5000;
                bulkConfig.BulkCopyTimeout = 600000;
            });
        }
        public void UpdateEmailSendResult(List<EmailSendData> emailSendResults)
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

        public void DeleteEmailSendTask(int sendEmailTaskId)
        {
            var upd = _context.EmailSendTask.FirstOrDefault(x => x.Id == sendEmailTaskId);
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

        public List<SearchEmailReport> SearchEmail(string email)
        {
            var result = _context.EmailSendData.AsNoTracking()
                .Include(x=>x.EmailSendTask)
                .Where(x => x.Email.Trim().ToLower() == email.Trim().ToLower())
                .GroupBy(esd => new { esd.EmailSendTask.Name, esd.Email, esd.EmailSendTask.CreateDate, esd.EmailSendTaskId })
                .Select(g => new SearchEmailReport
                {
                    TaskSendName = g.Key.Name,      // Имя рассылки
                    CreateDate = g.Key.CreateDate,
                    TaskSendId = g.Key.EmailSendTaskId,
                    Email = g.Key.Email,              // Адрес email
                    Count = g.Count()                 // Количество повторений
                })
                .ToList();
            return result;
        }

        public Part<EmailSendData> EmailResultPath(string? inputValue, int sendTaskId, int pageNumber, int pageSize, string? sortField, string? orderBy)
        {
            IQueryable<EmailSendData> data;
            if (inputValue?.Length > 1)
            {
                data =
                   _context.EmailSendData.AsNoTracking().AsQueryable()
                   .Where(x => x.EmailSendTaskId == sendTaskId && x.Email.Contains(inputValue))
                   .ToPageList(pageNumber, pageSize, sortField, orderBy);
            }
            else
            {
                data =
                   _context.EmailSendData.AsNoTracking().AsQueryable()
                   .Where(x => x.EmailSendTaskId == sendTaskId)
                   .ToPageList(pageNumber, pageSize, sortField, orderBy);
            }
            var count = _context.EmailSendData
                .AsNoTracking()
                .Where(x => x.EmailSendTaskId == sendTaskId)
                .Count();

            var resultPart = new Part<EmailSendData>()
            {
                TotalCount = count,
                Items = data.ToList()
            };
            return resultPart;
        }
    }
}
