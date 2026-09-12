using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DoacoesWorker.Messaging;

/// <summary>Uma conexao por processo (Singleton), aberta sob demanda.</summary>
public sealed class RabbitMQConexao(IOptions<RabbitMQOptions> options) : IAsyncDisposable
{
    private readonly RabbitMQOptions _opt = options.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _conexao;

    public async Task<IConnection> ObterAsync(CancellationToken ct = default)
    {
        if (_conexao is { IsOpen: true }) return _conexao;

        await _lock.WaitAsync(ct);
        try
        {
            if (_conexao is { IsOpen: true }) return _conexao;

            var factory = new ConnectionFactory
            {
                HostName = _opt.Host,
                Port = _opt.Port,
                UserName = _opt.Username,
                Password = _opt.Password,
                VirtualHost = _opt.VirtualHost
            };
            _conexao = await factory.CreateConnectionAsync("DoacoesWorker", ct);
            return _conexao;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_conexao is not null) await _conexao.DisposeAsync();
        _lock.Dispose();
    }
}
