using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CampanhasAPI.Messaging;

/// <summary>
/// Uma conexão por processo (Singleton), aberta sob demanda. Canais são baratos;
/// conexões não — o padrão da Fase 3 mantinha a conexão aberta pelo mesmo motivo.
/// </summary>
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
            _conexao = await factory.CreateConnectionAsync("CampanhasAPI", ct);
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
