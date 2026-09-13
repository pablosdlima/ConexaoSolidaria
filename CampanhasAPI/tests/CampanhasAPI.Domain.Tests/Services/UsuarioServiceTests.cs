using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using CampanhasAPI.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using static CampanhasAPI.Domain.Tests.Fixtures;

namespace CampanhasAPI.Domain.Tests.Services;

public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly ISenhaHasher _hasher = Substitute.For<ISenhaHasher>();
    private readonly UsuarioService _sut;

    public UsuarioServiceTests()
    {
        _hasher.Hash(Arg.Any<string>()).Returns(s => "hash:" + s.Arg<string>());
        _sut = new UsuarioService(_repo, _hasher, NullLogger<UsuarioService>.Instance);
    }

    [Fact]
    public async Task CadastrarDoador_ComDadosNovos_PersisteComSenhaHasheada()
    {
        var u = await _sut.CadastrarDoadorAsync("Maria", "maria@x.com", CpfValido, "Senha@123");

        Assert.Equal("hash:Senha@123", u.SenhaHash);      // RF-3: nunca texto puro
        await _repo.Received(1).AdicionarAsync(Arg.Is<Usuario>(x => x.Email == "maria@x.com"), Arg.Any<CancellationToken>());
    }

    // RF-3: e-mail unico no banco
    [Fact]
    public async Task CadastrarDoador_ComEmailJaCadastrado_LancaConflitoENaoPersiste()
    {
        _repo.EmailJaCadastradoAsync("maria@x.com", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflitoException>(() =>
            _sut.CadastrarDoadorAsync("Maria", "maria@x.com", CpfValido, "Senha@123"));

        await _repo.DidNotReceive().AdicionarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CadastrarDoador_ComCpfJaCadastrado_LancaConflito()
    {
        _repo.CpfJaCadastradoAsync("11144477735", Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<ConflitoException>(() =>
            _sut.CadastrarDoadorAsync("Maria", "maria@x.com", CpfValido, "Senha@123"));

        Assert.Contains("CPF", ex.Message);
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task ValidarLogin_ComSenhaCorreta_RetornaUsuario()
    {
        var doador = Doador();
        _repo.ObterPorEmailAsync(doador.Email, Arg.Any<CancellationToken>()).Returns(doador);
        _hasher.Verificar("Senha@123", doador.SenhaHash).Returns(true);

        var u = await _sut.ValidarLoginAsync(doador.Email, "Senha@123");

        Assert.Same(doador, u);
    }

    [Fact]
    public async Task ValidarLogin_ComSenhaErrada_Lanca401()
    {
        var doador = Doador();
        _repo.ObterPorEmailAsync(doador.Email, Arg.Any<CancellationToken>()).Returns(doador);
        _hasher.Verificar(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var ex = await Assert.ThrowsAsync<AutenticacaoException>(() => _sut.ValidarLoginAsync(doador.Email, "errada"));
        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task ValidarLogin_ComEmailInexistente_MesmaMensagemDeSenhaErrada()
    {
        _repo.ObterPorEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var ex = await Assert.ThrowsAsync<AutenticacaoException>(() => _sut.ValidarLoginAsync("nao@existe.com", "x"));

        Assert.Equal("E-mail ou senha inválidos.", ex.Message);   // nao revela quais e-mails existem
        _hasher.DidNotReceive().Verificar(Arg.Any<string>(), Arg.Any<string>());
    }
}
