using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Interface for the messenger service that enables communication between ViewModels.
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Registers a recipient for a specific message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to register for.</typeparam>
        /// <param name="recipient">The recipient instance.</param>
        /// <param name="action">The action to execute when a message is received.</param>
        void Register<TMessage>(object recipient, Action<TMessage> action);

        /// <summary>
        /// Unregisters a recipient from receiving messages.
        /// </summary>
        /// <param name="recipient">The recipient to unregister.</param>
        void Unregister(object recipient);

        /// <summary>
        /// Unregisters a recipient from receiving a specific message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to unregister from.</typeparam>
        /// <param name="recipient">The recipient to unregister.</param>
        void Unregister<TMessage>(object recipient);

        /// <summary>
        /// Sends a message to registered recipients.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        void Send<TMessage>(TMessage message);

        /// <summary>
        /// Sends a message to registered recipients asynchronously.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of the messenger service that enables communication between ViewModels.
    /// </summary>
    public class Messenger : IMessenger
    {
        private readonly ILogger<Messenger> _logger;
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, List<Delegate>>> _recipients = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Messenger"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public Messenger(ILogger<Messenger> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            var messageType = typeof(TMessage);
            
            var messageRecipients = _recipients.GetOrAdd(messageType, _ => new ConcurrentDictionary<object, List<Delegate>>());
            var recipientActions = messageRecipients.GetOrAdd(recipient, _ => new List<Delegate>());
            
            lock (recipientActions)
            {
                recipientActions.Add(action);
            }
            
            _logger.LogDebug("Registered {Recipient} for message type {MessageType}", recipient.GetType().Name, messageType.Name);
        }

        /// <inheritdoc/>
        public void Unregister(object recipient)
        {
            foreach (var messageType in _recipients.Keys)
            {
                if (_recipients.TryGetValue(messageType, out var messageRecipients))
                {
                    messageRecipients.TryRemove(recipient, out _);
                    _logger.LogDebug("Unregistered {Recipient} from all message types", recipient.GetType().Name);
                }
            }
        }

        /// <inheritdoc/>
        public void Unregister<TMessage>(object recipient)
        {
            var messageType = typeof(TMessage);
            
            if (_recipients.TryGetValue(messageType, out var messageRecipients))
            {
                messageRecipients.TryRemove(recipient, out _);
                _logger.LogDebug("Unregistered {Recipient} from message type {MessageType}", recipient.GetType().Name, messageType.Name);
            }
        }

        /// <inheritdoc/>
        public void Send<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);
            
            if (!_recipients.TryGetValue(messageType, out var messageRecipients))
            {
                _logger.LogDebug("No recipients registered for message type {MessageType}", messageType.Name);
                return;
            }
            
            foreach (var kvp in messageRecipients)
            {
                var recipient = kvp.Key;
                var actions = kvp.Value;
                
                foreach (var action in actions.ToList())
                {
                    try
                    {
                        ((Action<TMessage>)action)(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error delivering message of type {MessageType} to {Recipient}", 
                            messageType.Name, recipient.GetType().Name);
                    }
                }
            }
            
            _logger.LogDebug("Sent message of type {MessageType} to {RecipientCount} recipients", 
                messageType.Name, messageRecipients.Count);
        }

        /// <inheritdoc/>
        public async Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            var messageType = typeof(TMessage);
            
            if (!_recipients.TryGetValue(messageType, out var messageRecipients))
            {
                _logger.LogDebug("No recipients registered for message type {MessageType}", messageType.Name);
                return;
            }
            
            var tasks = new List<Task>();
            
            foreach (var kvp in messageRecipients)
            {
                var recipient = kvp.Key;
                var actions = kvp.Value;
                
                foreach (var action in actions.ToList())
                {
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            ((Action<TMessage>)action)(message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error delivering message of type {MessageType} to {Recipient}", 
                                messageType.Name, recipient.GetType().Name);
                        }
                    }, cancellationToken));
                }
            }
            
            await Task.WhenAll(tasks);
            
            _logger.LogDebug("Sent message of type {MessageType} to {RecipientCount} recipients asynchronously", 
                messageType.Name, messageRecipients.Count);
        }
    }
}