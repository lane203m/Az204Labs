using Azure.Messaging.ServiceBus;
using Azure.Identity;
using System.Timers;


// TODO: Replace <YOUR-NAMESPACE> with your Service Bus namespace
string svcbusNameSpace = "<namespace>.servicebus.windows.net";
string queueName = "myQueue";


// ADD CODE TO CREATE A SERVICE BUS CLIENT
DefaultAzureCredentialOptions options = new()
{
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential = true
};
ServiceBusClient client = new ServiceBusClient(svcbusNameSpace, new DefaultAzureCredential(options));

// ADD CODE TO SEND MESSAGES TO THE QUEUE

ServiceBusSender sender = client.CreateSender(queueName);
using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

for (int i = 1; i <= 3; i++)
{
  if(!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}"))){
            throw new Exception($"The message {i} is too large to fit in the batch.");
  }
}

try
{
    await sender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of 3 messages has been published to the queue.");
}
finally
{
    await sender.DisposeAsync();
}

// ADD CODE TO PROCESS MESSAGES FROM THE QUEUE

ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
const int idleTimeoutMs = 3000;
System.Timers.Timer idleTimer = new(idleTimeoutMs);
idleTimer.Elapsed += async (s, e) =>
{
    Console.WriteLine($"No messages received for {idleTimeoutMs / 1000} seconds. Stopping processor...");
    await processor.StopProcessingAsync();
};

try
{
  processor.ProcessMessageAsync += MessageHandler;
  processor.ProcessErrorAsync += ErrorHandler;
  idleTimer.Start();
  await processor.StartProcessingAsync();
  while (processor.IsProcessing)
  {
    await Task.Delay(500);
  }
  idleTimer.Stop();


} finally
{
  await processor.DisposeAsync();
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received message: {body}");
    idleTimer.Stop();
    idleTimer.Start();
    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
  Console.WriteLine(args.Exception.ToString());
  return Task.CompletedTask;
}

// Dispose client after use
await client.DisposeAsync();
