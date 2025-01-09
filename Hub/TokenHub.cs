namespace WebEmailSendler
{
    public static class TokenHub
    {
        public static Dictionary<int, CancellationTokenSource> CancelTokenTasks = new Dictionary<int, CancellationTokenSource>();
        public static CancellationToken AddToken(int emailSendTaskId)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancelTokenTasks.Add(emailSendTaskId, cancelTokenSource);
            return cancelTokenSource.Token;
        }
        public static async Task CancelAsync(int emailSendTaskId)
        {
            var token = CancelTokenTasks.GetValueOrDefault(emailSendTaskId);
            if (token != null)
            {
                await token.CancelAsync();
            }
        }
    }
}
