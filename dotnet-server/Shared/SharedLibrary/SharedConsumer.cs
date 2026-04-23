using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SharedLibrary
{
    public abstract class SharedConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _dlqProducer;
        private readonly ILogger<SharedConsumer> _logger;
        private readonly string _kafkaGroupId;

        public SharedConsumer(string kafkaConnection, string kafkaGroupId, string kafkaSubscribeTopic, ILogger<SharedConsumer> logger)
        {
            _logger = logger;
            _kafkaGroupId = kafkaGroupId;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaConnection,
                GroupId = kafkaGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Ручной commit для гарантии
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _dlqProducer = new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = kafkaConnection
            }).Build();
            _consumer.Subscribe(kafkaSubscribeTopic);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => ProcessMessages(stoppingToken), stoppingToken);
        }
        private async Task ProcessMessages(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Асинхронный Consume (не блокирует)
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult != null && consumeResult.Message != null)
                    {
                        try
                        {
                            // Обработка сообщения
                            await ProcessMessageAsync(consumeResult, stoppingToken);
                            _consumer.Commit(consumeResult);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message from topic {Topic}", consumeResult.Topic);

                            var movedToDlq = await PublishToDlqAsync(consumeResult, ex, stoppingToken);
                            if (movedToDlq)
                            {
                                _consumer.Commit(consumeResult);
                            }
                            else
                            {
                                await Task.Delay(1000, stoppingToken);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _consumer.Close();
            _dlqProducer.Flush(TimeSpan.FromSeconds(5));
            _dlqProducer.Dispose();
        }

        private async Task<bool> PublishToDlqAsync(ConsumeResult<string, string> consumeResult, Exception ex, CancellationToken stoppingToken)
        {
            var dlqTopic = $"{consumeResult.Topic}.dlq";

            try
            {
                var headers = new Headers();
                if (consumeResult.Message.Headers != null)
                {
                    foreach (var header in consumeResult.Message.Headers)
                    {
                        headers.Add(header.Key, header.GetValueBytes());
                    }
                }

                headers.Add("dlq-source-topic", Encoding.UTF8.GetBytes(consumeResult.Topic));
                headers.Add("dlq-source-group", Encoding.UTF8.GetBytes(_kafkaGroupId));
                headers.Add("dlq-error", Encoding.UTF8.GetBytes(ex.Message));
                headers.Add("dlq-timestamp", BitConverter.GetBytes(DateTime.UtcNow.Ticks));

                var dlqMessage = new Message<string, string>
                {
                    Key = consumeResult.Message.Key,
                    Value = consumeResult.Message.Value,
                    Headers = headers
                };

                await _dlqProducer.ProduceAsync(dlqTopic, dlqMessage, stoppingToken);
                _logger.LogWarning("Message moved to DLQ topic {DlqTopic} from {SourceTopic}", dlqTopic, consumeResult.Topic);
                return true;
            }
            catch (Exception dlqEx)
            {
                _logger.LogError(dlqEx, "Failed to publish message to DLQ topic {DlqTopic}", dlqTopic);
                return false;
            }
        }
        protected abstract Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken);
    }
}
