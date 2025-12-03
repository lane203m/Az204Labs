using Azure;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Threading.Tasks;
// Create a unique name for the queue
// TODO: Replace the <YOUR-STORAGE-ACCT-NAME> placeholder 
string queueName = "myqueue-" + Guid.NewGuid().ToString();
string storageAccountName = "<YOUR-STORAGE-ACCT-NAME>";

// ADD CODE TO CREATE A QUEUE CLIENT AND CREATE A QUEUE

DefaultAzureCredentialOptions options = new()
{
    ExcludeEnvironmentCredential = true,
    ExcludeManagedIdentityCredential = true
};

QueueClient queueClient = new QueueClient(
    new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
    new DefaultAzureCredential(options));
queueClient.CreateIfNotExists();

if (queueClient.Exists())
{
  queueClient.SendMessage("Hello, World!");
  queueClient.SendMessage("Hello, World!2");
  SendReceipt receipt = queueClient.SendMessage("Hello, World!3");

  foreach (var msg in queueClient.PeekMessages(maxMessages: 10).Value)
  {
      Console.WriteLine($"Message: {msg.MessageText}");
  }



  // ADD CODE TO SEND AND LIST MESSAGES

  QueueMessage[] message = queueClient.ReceiveMessages().Value;
  // ADD CODE TO UPDATE A MESSAGE AND LIST MESSAGES

  Console.WriteLine($"Message before update: {message[0].MessageText}");
  Console.WriteLine($"Body before update: {message[0].Body.ToString()}");
  queueClient.UpdateMessage(message[0].MessageId,
      message[0].PopReceipt,
      "Updated Message",
      TimeSpan.FromSeconds(0));

  queueClient.UpdateMessage(receipt.MessageId,
      receipt.PopReceipt,
      "Updated Message for Receipt 3",
      TimeSpan.FromSeconds(0));

  foreach (var msg in queueClient.PeekMessages(maxMessages: 10).Value)
  {
      Console.WriteLine($"New Messages: {msg.MessageText}");
  }



  // ADD CODE TO DELETE MESSAGES AND THE QUEUE
  
  foreach (var msg in queueClient.ReceiveMessages(maxMessages: 10).Value)
  {
      Console.WriteLine($"Delete: {msg.MessageText}");
      queueClient.DeleteMessage(msg.MessageId, msg.PopReceipt);
  }
  
  
  queueClient.Delete();
}