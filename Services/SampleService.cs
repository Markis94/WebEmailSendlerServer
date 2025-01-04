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
