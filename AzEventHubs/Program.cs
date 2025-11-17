using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Identity;
using System.Text;

// TO-DO: Replace YOUR_EVENT_HUB_NAMESPACE with your actual Event Hub namespace
string namespaceURL = "YOUR_EVENT_HUB_NAMESPACE.servicebus.windows.net";
string eventHubName = "demo-event-hub"; 

// Create a DefaultAzureCredentialOptions object to exclude certain credentials
DefaultAzureCredentialOptions options = new()
{
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential = true
};

// Number of events to be sent to the event hub
int numOfEvents = 3;

// CREATE A PRODUCER CLIENT AND SEND EVENTS
EventHubProducerClient producerClient = new EventHubProducerClient(namespaceURL, eventHubName, new DefaultAzureCredential(options));

    // Create a batch of events 
using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
var random = new Random();
for (int i = 1; i <= numOfEvents; i++)
{

  int randomNumber = random.Next(1, 101); // 1 to 100 inclusive
  string eventBody = $"Event {randomNumber}";
  if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(eventBody))))
  {
      // if it is too large for the batch
      throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
  }

}

// Use the producer client to send the batch of events to the event hub
try
{
    // Use the producer client to send the batch of events to the event hub
    await producerClient.SendAsync(eventBatch);

    Console.WriteLine($"A batch of {numOfEvents} events has been published.");
    Console.WriteLine("Press Enter to retrieve and print the events...");
    Console.ReadLine();
}
finally
{
    await producerClient.DisposeAsync();
}


// CREATE A CONSUMER CLIENT AND RECEIVE EVENTS



await using EventHubConsumerClient consumerClient = new EventHubConsumerClient(
    EventHubConsumerClient.DefaultConsumerGroupName,
    namespaceURL,
    eventHubName,
    new DefaultAzureCredential(options));

Console.Clear();
Console.WriteLine("Retrieving all events from the hub...");

long totalEventCount = 0;

string[] partitionIds = await consumerClient.GetPartitionIdsAsync();
foreach (string partitionId in partitionIds)
{
  PartitionProperties properties = await consumerClient.GetPartitionPropertiesAsync(partitionId);
  if(!properties.IsEmpty && properties.LastEnqueuedSequenceNumber >= properties.BeginningSequenceNumber)
  {
            totalEventCount += (properties.LastEnqueuedSequenceNumber - properties.BeginningSequenceNumber + 1);
  }
}

int retrievedCount = 0;
await foreach (PartitionEvent partitionEvent in consumerClient.ReadEventsAsync(startReadingAtEarliestEvent: true))
{
    if (partitionEvent.Data != null)
    {
        string body = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
        Console.WriteLine($"Retrieved event: {body}");
        retrievedCount++;
        if (retrievedCount >= totalEventCount)
        {
            Console.WriteLine("Done retrieving events. Press Enter to exit...");
            Console.ReadLine();
            return;
        }
    }
}