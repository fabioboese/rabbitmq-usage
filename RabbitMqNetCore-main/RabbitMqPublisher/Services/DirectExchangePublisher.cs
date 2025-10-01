using RabbitMQ.Client;
using RabbitMqDomain.Services;
using System;
using System.Text;
using System.Threading.Channels;

namespace RabbitMQPublisher.Services
{
    public class DirectExchangePublisher : IDisposable 
    {
        // Serviço auxiliar para gerenciar conexões com o RabbitMQ
        private readonly RabbitMqService _rabbitMQService;

        // Canal de comunicação com o RabbitMQ, usado para enviar mensagens
        private readonly IModel _rabbitMQchannel;

        // Conexão com o servidor RabbitMQ
        private readonly IConnection _rabbitMQconection;

        // Nome do "Exchange" (intermediário que roteia mensagens para filas)
        private readonly string _exchangeName = "DemoDirectExChange";

        // Nomes das filas que serão criadas
        private readonly string _queueOne = "queueOne";
        private readonly string _queueTwo = "queueTwo";
        private readonly string _queueThree = "queueThree";

        // Construtor: inicializa a conexão com o RabbitMQ e configura o Exchange e as filas
        public DirectExchangePublisher()
        {
            // Cria uma instância do serviço RabbitMqService para gerenciar conexões
            _rabbitMQService = new RabbitMqService();

            // Obtém uma conexão com o servidor RabbitMQ
            _rabbitMQconection = _rabbitMQService.GetRabbitMQConnection();

            // Cria um canal de comunicação dentro da conexão
            _rabbitMQchannel = _rabbitMQconection.CreateModel();

            // Declara um Exchange do tipo "Direct" (roteamento baseado em chaves específicas)
            _rabbitMQchannel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, false, false);

            // Declara três filas no RabbitMQ
            _rabbitMQchannel.QueueDeclare(_queueOne, false, false, false);           
            _rabbitMQchannel.QueueDeclare(_queueTwo, false, false, false);
            _rabbitMQchannel.QueueDeclare(_queueThree, false, false, false);

            // Associa cada fila ao Exchange com uma chave de roteamento específica
            // "log.error" -> Mensagens de erro
            _rabbitMQchannel.QueueBind(_queueOne, _exchangeName, "log.error");

            // "log.warning" -> Mensagens de aviso
            _rabbitMQchannel.QueueBind(_queueTwo, _exchangeName, "log.warning");

            // "log.info" -> Mensagens informativas
            _rabbitMQchannel.QueueBind(_queueThree, _exchangeName, "log.info");
        }

        // Método para publicar uma mensagem no Exchange
        public string PublishMessage(string _routing_key, string message)
        {        
            // Publica a mensagem no Exchange, especificando a chave de roteamento
            _rabbitMQchannel.BasicPublish(
                _exchangeName,               // Nome do Exchange
                _routing_key,                // Chave de roteamento (define para qual fila a mensagem será enviada)
                null,                        // Propriedades adicionais (não usadas aqui)
                Encoding.UTF8.GetBytes(message) // Mensagem convertida para bytes
            );

            // Retorna informações sobre a mensagem publicada
            return $"ExchangeName:{_exchangeName}, RoutingKey:{_routing_key}, Message:{message}";
        }

        // Método para liberar recursos (conexão e canal)
        public void Dispose()
        {
            _rabbitMQchannel?.Dispose(); // Fecha o canal
            _rabbitMQconection?.Dispose(); // Fecha a conexão
        }
    }    
}