using WebEmailSendler.Managers;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class SampleService
    {
        private readonly DataManager _dataManager;
        public SampleService(DataManager dataManager)
        { 
            _dataManager = dataManager; 
        }
        public async Task<Sample> CreateSample(Sample sample)
        {
            return await _dataManager.CreateSample(sample);
        }
        public async Task<Sample> CreateCopySample(Sample sample)
        {
            var originSample = await _dataManager.SampleById(sample.Id);
            if(originSample != null)
            {
                originSample.CreateDate = DateTimeOffset.UtcNow;
                originSample.ChangeDate = DateTimeOffset.UtcNow;
                originSample.Id = 0;
                originSample.Name = originSample.Name + " - копия";
                return await _dataManager.CreateSample(originSample);
            }
            else
            {
                sample.CreateDate = DateTimeOffset.UtcNow;
                sample.ChangeDate = DateTimeOffset.UtcNow;
                sample.Id = 0;
                sample.Name = sample.Name + " - копия";
                return await _dataManager.CreateSample(sample);
            }
        }

        public async Task<IList<Sample>> SampleList()
        {
            return await _dataManager.SampleList();
        }

        public async Task<Sample> SampleById(int sampleId)
        {
            return await _dataManager.SampleById(sampleId);
        }
        public async Task DeleteSample(int sampleId)
        {
            await _dataManager.DeleteSample(sampleId);
        }
        public async Task UpdateSample(Sample sample)
        {
            sample.ChangeDate = DateTime.UtcNow;
            await _dataManager.UpdateSample(sample);
        }
    }
}
