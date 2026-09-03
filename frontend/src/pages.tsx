import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { api, mensagemErro } from './lib/api';
import type {
  AlertaEstoque,
  Cliente,
  Dashboard,
  MovimentoEstoque,
  MovimentoFinanceiro,
  Pagina,
  Perfil,
  Produto,
  Usuario,
  Venda,
} from './types';
import { Button, Card, Empty, Input, Loading } from './components/ui';
import { useAuth } from './App';
const dinheiro = (valor: number) => valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
const data = (valor: string) => new Date(valor).toLocaleString('pt-BR');
function useValorComAtraso<T>(valor: T, atraso = 300) {
  const [valorAtrasado, setValorAtrasado] = useState(valor);
  useEffect(() => {
    const temporizador = window.setTimeout(() => setValorAtrasado(valor), atraso);
    return () => window.clearTimeout(temporizador);
  }, [atraso, valor]);
  return valorAtrasado;
}
function Titulo({ children, acao }: { children: React.ReactNode; acao?: React.ReactNode }) {
  return (
    <div className="mb-6 flex items-center justify-between">
      <h2 className="text-2xl font-bold">{children}</h2>
      {acao}
    </div>
  );
}
function useLista<T>(url: string) {
  const [dados, setDados] = useState<T[]>([]);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [paginaAtual, setPaginaAtual] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [totalItens, setTotalItens] = useState(0);
  useEffect(() => setPaginaAtual(1), [url]);
  const recarregar = useCallback(() => {
    setCarregando(true);
    setErro('');
    const separador = url.includes('?') ? '&' : '?';
    api
      .get<Pagina<T>>(`${url}${separador}pagina=${paginaAtual}`)
      .then((resposta) => {
        setDados(resposta.data.itens);
        setTotalPaginas(resposta.data.totalPaginas);
        setTotalItens(resposta.data.totalItens);
      })
      .catch((erro) => setErro(mensagemErro(erro)))
      .finally(() => setCarregando(false));
  }, [paginaAtual, url]);
  useEffect(() => recarregar(), [recarregar]);
  return { dados, erro, carregando, recarregar, paginaAtual, totalPaginas, totalItens, setPaginaAtual };
}

export function Paginador({
  paginaAtual,
  totalPaginas,
  totalItens,
  mudarPagina,
}: {
  paginaAtual: number;
  totalPaginas: number;
  totalItens: number;
  mudarPagina: (pagina: number) => void;
}) {
  if (totalPaginas <= 1)
    return totalItens > 0 ? <p className="mt-3 text-sm text-slate-500">{totalItens} registro(s)</p> : null;
  return (
    <nav className="mt-4 flex items-center justify-between gap-3" aria-label="Paginação">
      <Button type="button" disabled={paginaAtual <= 1} onClick={() => mudarPagina(paginaAtual - 1)}>
        Anterior
      </Button>
      <span className="text-sm text-slate-600">
        Página {paginaAtual} de {totalPaginas} — {totalItens} registro(s)
      </span>
      <Button type="button" disabled={paginaAtual >= totalPaginas} onClick={() => mudarPagina(paginaAtual + 1)}>
        Próxima
      </Button>
    </nav>
  );
}

const loginSchema = z.object({
  email: z.string().trim().min(1, 'Campo obrigatório.').pipe(z.email('Email inválido.')),
  senha: z.string().min(1, 'Campo obrigatório.').min(8, 'A senha deve ter ao menos 8 caracteres.'),
});
type Login = z.infer<typeof loginSchema>;
export function LoginPage() {
  const autenticacao = useAuth();
  const [erro, setErro] = useState('');
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<Login>({ resolver: zodResolver(loginSchema) });
  const enviar = async (credenciais: Login) => {
    try {
      const resposta = await api.post<{ perfil: Perfil }>('/auth/login', credenciais);
      autenticacao.entrar(resposta.data.perfil);
    } catch (erro) {
      setErro(mensagemErro(erro));
    }
  };
  return (
    <main className="grid min-h-screen place-items-center bg-slate-900 p-4">
      <Card className="w-full max-w-md">
        <h1 className="mb-1 text-2xl font-bold">Gestão Comercial</h1>
        <p className="mb-6 text-slate-500">Entre para acessar o sistema</p>
        {erro && <p className="error mb-4">{erro}</p>}
        <form onSubmit={handleSubmit(enviar)} className="space-y-4">
          <div>
            <label htmlFor="login-email">
              Email <span className="text-red-600">*</span>
            </label>
            <Input id="login-email" type="email" autoComplete="username" {...register('email')} />
            {errors.email && <small className="block text-red-600">{errors.email.message}</small>}
          </div>
          <div>
            <label htmlFor="login-senha">
              Senha <span className="text-red-600">*</span>
            </label>
            <Input id="login-senha" type="password" autoComplete="current-password" {...register('senha')} />
            {errors.senha && <small className="block text-red-600">{errors.senha.message}</small>}
          </div>
          <Button disabled={isSubmitting} className="w-full">
            Entrar
          </Button>
        </form>
      </Card>
    </main>
  );
}

export function DashboardPage() {
  const [dashboard, setDashboard] = useState<Dashboard>();
  const [erro, setErro] = useState('');
  useEffect(() => {
    api
      .get<Dashboard>('/dashboard')
      .then((resposta) => setDashboard(resposta.data))
      .catch((erro) => setErro(mensagemErro(erro)));
  }, []);
  if (erro) return <p className="error">{erro}</p>;
  if (!dashboard) return <Loading />;
  const cards = [
    ['Faturamento hoje', dinheiro(dashboard.faturamentoDia)],
    ['Faturamento mês', dinheiro(dashboard.faturamentoMes)],
    ['Despesas hoje', dinheiro(dashboard.despesasDia)],
    ['Despesas mês', dinheiro(dashboard.despesasMes)],
    ['Estornos hoje', dinheiro(dashboard.estornosDia)],
    ['Estornos mês', dinheiro(dashboard.estornosMes)],
    ['Saldo hoje', dinheiro(dashboard.saldoDia)],
    ['Saldo mês', dinheiro(dashboard.saldoMes)],
    ['Vendas hoje', String(dashboard.vendasDia)],
    ['Ticket médio mês', dinheiro(dashboard.ticketMedioMes)],
  ];
  return (
    <>
      <Titulo>Dashboard</Titulo>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {cards.map((itemCard) => (
          <Card key={itemCard[0]}>
            <p className="text-sm text-slate-500">{itemCard[0]}</p>
            <strong className="text-2xl">{itemCard[1]}</strong>
          </Card>
        ))}
      </div>
      <Card className="mt-6">
        <h3 className="mb-3 font-bold">Estoque baixo</h3>
        {dashboard.estoqueBaixo.length === 0 ? (
          <Empty>Nenhum produto com estoque baixo.</Empty>
        ) : (
          <TabelaProdutos itens={dashboard.estoqueBaixo} />
        )}
      </Card>
    </>
  );
}

export function ClientesPage() {
  const [busca, setBusca] = useState('');
  const buscaAtrasada = useValorComAtraso(busca);
  const url = useMemo(() => `/clientes?busca=${encodeURIComponent(buscaAtrasada)}`, [buscaAtrasada]);
  const listaClientes = useLista<Cliente>(url);
  return (
    <>
      <Titulo
        acao={
          <Link className="btn" to="/clientes/novo">
            Novo cliente
          </Link>
        }
      >
        Clientes
      </Titulo>
      <Input
        placeholder="Buscar por nome, CPF ou telefone"
        value={busca}
        onChange={(evento) => setBusca(evento.target.value)}
      />
      {listaClientes.erro && <p className="error mt-4">{listaClientes.erro}</p>}
      {listaClientes.carregando ? (
        <Loading />
      ) : (
        <Card className="mt-4 overflow-x-auto">
          {listaClientes.dados.length === 0 ? (
            <Empty>Nenhum cliente encontrado.</Empty>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>CPF</th>
                  <th>Telefone</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {listaClientes.dados.map((item) => (
                  <tr key={item.id}>
                    <td>{item.nome}</td>
                    <td>{item.cpf}</td>
                    <td>{item.telefone}</td>
                    <td>
                      <Link className="text-cyan-700" to={`/clientes/${item.id}`}>
                        Detalhes
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </Card>
      )}
      <Paginador
        paginaAtual={listaClientes.paginaAtual}
        totalPaginas={listaClientes.totalPaginas}
        totalItens={listaClientes.totalItens}
        mudarPagina={listaClientes.setPaginaAtual}
      />
    </>
  );
}
const formatarCep = (valor: string) => {
  const digitos = valor.replace(/\D/g, '').slice(0, 8);
  return digitos.length > 5 ? `${digitos.slice(0, 5)}-${digitos.slice(5)}` : digitos;
};

const clienteSchema = z.object({
  nome: z.string().trim().min(1, 'Campo obrigatório.').min(2, 'Informe ao menos 2 caracteres.'),
  cpf: z.string().trim().min(1, 'Campo obrigatório.').min(11, 'Informe um CPF válido.'),
  email: z.union([z.email(), z.literal('')]),
  telefone: z.string().trim().min(1, 'Campo obrigatório.'),
  dataNascimento: z.string(),
  cep: z
    .string()
    .min(1, 'Campo obrigatório.')
    .regex(/^\d{5}-?\d{3}$/, 'Informe um CEP válido com 8 números.'),
  logradouro: z.string().trim().min(1, 'Campo obrigatório.'),
  numero: z.string().trim().min(1, 'Campo obrigatório.'),
  complemento: z.string(),
  bairro: z.string().trim().min(1, 'Campo obrigatório.'),
  cidade: z.string().trim().min(1, 'Campo obrigatório.'),
  uf: z.string().trim().min(1, 'Campo obrigatório.').length(2, 'Informe uma UF válida.'),
});
type ClienteForm = z.infer<typeof clienteSchema>;
export function ClienteFormPage() {
  const { id } = useParams();
  const nav = useNavigate();
  const [erro, setErro] = useState('');
  const [cepAviso, setCepAviso] = useState('');
  const {
    register,
    handleSubmit,
    reset,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<ClienteForm>({
    resolver: zodResolver(clienteSchema),
    defaultValues: {
      email: '',
      telefone: '',
      dataNascimento: '',
      complemento: '',
    },
  });
  useEffect(() => {
    if (id)
      api.get<Cliente>(`/clientes/${id}`).then(({ data: cliente }) =>
        reset({
          nome: cliente.nome,
          cpf: cliente.cpf,
          email: cliente.email ?? '',
          telefone: cliente.telefone,
          dataNascimento: cliente.dataNascimento ?? '',
          ...cliente.endereco,
          cep: formatarCep(cliente.endereco.cep),
        }),
      );
  }, [id, reset]);
  const campoCep = register('cep');
  const buscarCep = async (valor: string) => {
    const cepLimpo = valor.replace(/\D/g, '');
    if (cepLimpo.length !== 8) {
      setCepAviso(cepLimpo.length === 0 ? 'Campo obrigatório.' : 'Informe um CEP válido com 8 números.');
      return;
    }

    setCepAviso('');
    try {
      const resposta = await api.get<{
        logradouro: string;
        complemento?: string;
        bairro: string;
        cidade: string;
        uf: string;
      }>(`/cep/${cepLimpo}`);
      (['logradouro', 'complemento', 'bairro', 'cidade', 'uf'] as const).forEach((campoEndereco) =>
        setValue(campoEndereco, resposta.data[campoEndereco] ?? ''),
      );
    } catch (erro) {
      setCepAviso(`${mensagemErro(erro)} Você pode preencher o endereço manualmente.`);
    }
  };
  const enviar = async (clienteForm: ClienteForm) => {
    const body = {
      nome: clienteForm.nome,
      cpf: clienteForm.cpf,
      email: clienteForm.email || null,
      telefone: clienteForm.telefone,
      dataNascimento: clienteForm.dataNascimento || null,
      endereco: {
        cep: clienteForm.cep,
        logradouro: clienteForm.logradouro,
        numero: clienteForm.numero,
        complemento: clienteForm.complemento || null,
        bairro: clienteForm.bairro,
        cidade: clienteForm.cidade,
        uf: clienteForm.uf,
      },
    };
    try {
      if (id) await api.put(`/clientes/${id}`, body);
      else await api.post('/clientes', body);
      nav('/clientes');
    } catch (erro) {
      setErro(mensagemErro(erro));
    }
  };
  return (
    <>
      <Titulo>{id ? 'Editar cliente' : 'Novo cliente'}</Titulo>
      {erro && <p className="error mb-4">{erro}</p>}
      <Card>
        <form onSubmit={handleSubmit(enviar)} className="grid gap-4 md:grid-cols-2">
          <Campo label="Nome" obrigatorio erro={errors.nome?.message}>
            <Input {...register('nome')} />
          </Campo>
          <Campo label="CPF" obrigatorio erro={errors.cpf?.message}>
            <Input disabled={Boolean(id)} {...register('cpf')} />
          </Campo>
          <Campo label="Email">
            <Input {...register('email')} />
          </Campo>
          <Campo label="Telefone" obrigatorio erro={errors.telefone?.message}>
            <Input {...register('telefone')} />
          </Campo>
          <Campo label="Nascimento">
            <Input type="date" {...register('dataNascimento')} />
          </Campo>
          <div />
          <Campo label="CEP" obrigatorio erro={cepAviso || errors.cep?.message}>
            <Input
              {...campoCep}
              inputMode="numeric"
              maxLength={9}
              placeholder="00000-000"
              aria-invalid={Boolean(cepAviso || errors.cep)}
              onChange={(evento) => {
                evento.target.value = formatarCep(evento.target.value);
                campoCep.onChange(evento);
                setCepAviso('');
              }}
              onBlur={(evento) => {
                campoCep.onBlur(evento);
                void buscarCep(evento.target.value);
              }}
            />
          </Campo>
          <Campo label="Logradouro" obrigatorio erro={errors.logradouro?.message}>
            <Input {...register('logradouro')} />
          </Campo>
          <Campo label="Número" obrigatorio erro={errors.numero?.message}>
            <Input {...register('numero')} />
          </Campo>
          <Campo label="Complemento">
            <Input {...register('complemento')} />
          </Campo>
          <Campo label="Bairro" obrigatorio erro={errors.bairro?.message}>
            <Input {...register('bairro')} />
          </Campo>
          <Campo label="Cidade" obrigatorio erro={errors.cidade?.message}>
            <Input {...register('cidade')} />
          </Campo>
          <Campo label="UF" obrigatorio erro={errors.uf?.message}>
            <Input maxLength={2} {...register('uf')} />
          </Campo>
          <div className="md:col-span-2">
            <Button disabled={isSubmitting}>Salvar</Button>
          </div>
        </form>
      </Card>
    </>
  );
}
export function ClienteDetalhePage() {
  const { id } = useParams();
  const [cliente, setCliente] = useState<Cliente>();
  const vendasCliente = useLista<Venda>(`/clientes/${id}/compras`);
  useEffect(() => {
    api.get<Cliente>(`/clientes/${id}`).then((resposta) => setCliente(resposta.data));
  }, [id]);
  if (!cliente) return <Loading />;
  return (
    <>
      <Titulo
        acao={
          <Link className="btn-secondary" to={`/clientes/${id}/editar`}>
            Editar
          </Link>
        }
      >
        {cliente.nome}
      </Titulo>
      <Card>
        <p>
          {cliente.cpf} · {cliente.email ?? 'Sem email'} · {cliente.telefone}
        </p>
        <p>
          {cliente.endereco.logradouro}, {cliente.endereco.numero} — {cliente.endereco.cidade}/{cliente.endereco.uf}
        </p>
      </Card>
      <Card className="mt-4">
        <h3 className="font-bold">Histórico de compras</h3>
        {vendasCliente.dados.length === 0 ? <Empty>Sem compras.</Empty> : <TabelaVendas itens={vendasCliente.dados} />}
      </Card>
      <Paginador
        paginaAtual={vendasCliente.paginaAtual}
        totalPaginas={vendasCliente.totalPaginas}
        totalItens={vendasCliente.totalItens}
        mudarPagina={vendasCliente.setPaginaAtual}
      />
    </>
  );
}

export function ProdutosPage() {
  const autenticacao = useAuth();
  const [busca, setBusca] = useState('');
  const buscaAtrasada = useValorComAtraso(busca);
  const listaProdutos = useLista<Produto>(`/produtos?busca=${encodeURIComponent(buscaAtrasada)}`);
  return (
    <>
      <Titulo
        acao={
          autenticacao.perfil === 'Administrador' ? (
            <Link className="btn" to="/produtos/novo">
              Novo produto
            </Link>
          ) : undefined
        }
      >
        Produtos
      </Titulo>
      <Input
        placeholder="Buscar por código ou nome"
        value={busca}
        onChange={(evento) => setBusca(evento.target.value)}
      />
      {listaProdutos.carregando ? (
        <Loading />
      ) : (
        <Card className="mt-4 overflow-x-auto">
          {listaProdutos.dados.length ? <TabelaProdutos itens={listaProdutos.dados} /> : <Empty>Nenhum produto.</Empty>}
        </Card>
      )}
      <Paginador
        paginaAtual={listaProdutos.paginaAtual}
        totalPaginas={listaProdutos.totalPaginas}
        totalItens={listaProdutos.totalItens}
        mudarPagina={listaProdutos.setPaginaAtual}
      />
    </>
  );
}
const produtoSchema = z.object({
  codigo: z.string().trim().min(1, 'Campo obrigatório.'),
  nome: z.string().trim().min(1, 'Campo obrigatório.').min(2, 'Informe ao menos 2 caracteres.'),
  descricao: z.string(),
  precoCusto: z.number({ error: 'Campo obrigatório.' }).min(0, 'O valor não pode ser negativo.'),
  precoVenda: z.number({ error: 'Campo obrigatório.' }).min(0, 'O valor não pode ser negativo.'),
  estoqueMinimo: z.number({ error: 'Campo obrigatório.' }).int().min(0, 'O valor não pode ser negativo.'),
});
type ProdutoForm = z.infer<typeof produtoSchema>;
export function ProdutoFormPage() {
  const { id } = useParams();
  const nav = useNavigate();
  const [erro, setErro] = useState('');
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProdutoForm>({
    resolver: zodResolver(produtoSchema),
    defaultValues: {
      codigo: '',
      nome: '',
      descricao: '',
      precoCusto: 0,
      precoVenda: 0,
      estoqueMinimo: 0,
    },
  });
  useEffect(() => {
    if (id) api.get<Produto>(`/produtos/${id}`).then((resposta) => reset(resposta.data));
  }, [id, reset]);
  const enviar = async (produtoForm: ProdutoForm) => {
    try {
      if (id) await api.put(`/produtos/${id}`, produtoForm);
      else await api.post('/produtos', produtoForm);
      nav('/produtos');
    } catch (erro) {
      setErro(mensagemErro(erro));
    }
  };
  return (
    <>
      <Titulo>{id ? 'Editar produto' : 'Novo produto'}</Titulo>
      {erro && <p className="error">{erro}</p>}
      <Card>
        <form onSubmit={handleSubmit(enviar)} className="grid gap-4 md:grid-cols-2">
          <Campo label="Código" obrigatorio erro={errors.codigo?.message}>
            <Input disabled={Boolean(id)} {...register('codigo')} />
          </Campo>
          <Campo label="Nome" obrigatorio erro={errors.nome?.message}>
            <Input {...register('nome')} />
          </Campo>
          <Campo label="Descrição">
            <Input {...register('descricao')} />
          </Campo>
          <Campo label="Preço custo" obrigatorio erro={errors.precoCusto?.message}>
            <Input type="number" step="0.01" {...register('precoCusto', { valueAsNumber: true })} />
          </Campo>
          <Campo label="Preço venda" obrigatorio erro={errors.precoVenda?.message}>
            <Input type="number" step="0.01" {...register('precoVenda', { valueAsNumber: true })} />
          </Campo>
          <Campo label="Estoque mínimo" obrigatorio erro={errors.estoqueMinimo?.message}>
            <Input type="number" {...register('estoqueMinimo', { valueAsNumber: true })} />
          </Campo>
          <Button>Salvar</Button>
        </form>
      </Card>
    </>
  );
}

export function EstoquePage() {
  const produtos = useLista<Produto>('/produtos?tamanhoPagina=100');
  const movimentos = useLista<MovimentoEstoque>('/estoque/movimentacoes?tamanhoPagina=100');
  const alertas = useLista<AlertaEstoque>('/estoque/alertas?tamanhoPagina=20');
  const [produtoId, setProduto] = useState('');
  const [quantidade, setQuantidade] = useState(1);
  const [tipo, setTipo] = useState('Entrada');
  const [observacao, setObservacao] = useState('');
  const [erro, setErro] = useState('');
  const [tentouEnviar, setTentouEnviar] = useState(false);
  const enviar = async () => {
    setTentouEnviar(true);
    if (!produtoId || quantidade <= 0) return;
    try {
      await api.post('/estoque/movimentacoes', {
        produtoId,
        quantidade: quantidade,
        tipo,
        observacao: observacao,
      });
      movimentos.recarregar();
      produtos.recarregar();
      setTentouEnviar(false);
    } catch (erro) {
      setErro(mensagemErro(erro));
    }
  };
  return (
    <>
      <Titulo>Controle de estoque</Titulo>
      {erro && <p className="error">{erro}</p>}
      <Card>
        <div className="grid items-start gap-3 md:grid-cols-4">
          <Campo label="Produto" obrigatorio erro={tentouEnviar && !produtoId ? 'Campo obrigatório.' : undefined}>
            <select value={produtoId} onChange={(evento) => setProduto(evento.target.value)}>
              <option value="">Selecione</option>
              {produtos.dados.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nome}
                </option>
              ))}
            </select>
          </Campo>
          <Campo
            label="Quantidade"
            obrigatorio
            erro={tentouEnviar && quantidade <= 0 ? 'Campo obrigatório.' : undefined}
          >
            <Input
              type="number"
              min={1}
              value={quantidade}
              onChange={(evento) => setQuantidade(Number(evento.target.value))}
            />
          </Campo>
          <Campo label="Tipo" obrigatorio>
            <select value={tipo} onChange={(evento) => setTipo(evento.target.value)}>
              <option>Entrada</option>
              <option>Ajuste</option>
            </select>
          </Campo>
          <Campo label="Observação">
            <Input value={observacao} onChange={(evento) => setObservacao(evento.target.value)} />
          </Campo>
          <Button onClick={enviar}>Registrar</Button>
        </div>
      </Card>
      <Card className="mt-4 overflow-x-auto">
        <h3 className="mb-3 font-bold">Alertas de estoque baixo ({alertas.totalItens})</h3>
        {alertas.dados.length === 0 ? (
          <Empty>Nenhum alerta de estoque.</Empty>
        ) : (
          <table>
            <thead><tr><th>Data</th><th>Produto</th><th>Estoque</th><th>Mínimo</th><th>Venda</th><th>Status</th></tr></thead>
            <tbody>{alertas.dados.map((alerta) => (
              <tr key={alerta.id}>
                <td>{data(alerta.createdAt)}</td><td>{alerta.produto}</td><td>{alerta.quantidadeAtual}</td>
                <td>{alerta.estoqueMinimo}</td><td>{alerta.numeroVenda}</td>
                <td>{alerta.visualizado ? 'Visualizado' : <Button type="button" onClick={async () => {
                  await api.put(`/estoque/alertas/${alerta.id}/visualizar`); alertas.recarregar();
                }}>Marcar como visto</Button>}</td>
              </tr>
            ))}</tbody>
          </table>
        )}
      </Card>
      <Card className="mt-4 overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Data</th>
              <th>Produto</th>
              <th>Tipo</th>
              <th>Quantidade</th>
              <th>Saldo</th>
            </tr>
          </thead>
          <tbody>
            {movimentos.dados.map((item) => (
              <tr key={item.id}>
                <td>{data(item.data)}</td>
                <td>{item.produto}</td>
                <td>{item.tipo}</td>
                <td>{item.quantidade}</td>
                <td>
                  {item.anterior} → {item.posterior}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>
      <Paginador
        paginaAtual={movimentos.paginaAtual}
        totalPaginas={movimentos.totalPaginas}
        totalItens={movimentos.totalItens}
        mudarPagina={movimentos.setPaginaAtual}
      />
    </>
  );
}

export function PdvPage() {
  const [buscaProduto, setBuscaProduto] = useState('');
  const buscaProdutoAtrasada = useValorComAtraso(buscaProduto);
  const produtos = useLista<Produto>(`/produtos?busca=${encodeURIComponent(buscaProdutoAtrasada)}`);
  const clientes = useLista<Cliente>('/clientes?tamanhoPagina=100');
  const [cliente, setCliente] = useState('');
  const [pag, setPag] = useState('Pix');
  const [carrinho, setCarrinho] = useState<Record<string, { produto: Produto; quantidade: number }>>({});
  const [msg, setMsg] = useState('');
  const [finalizando, setFinalizando] = useState(false);
  const chaveIdempotencia = useRef(crypto.randomUUID());
  const itens = Object.values(carrinho).map((itemCarrinho) => ({
    ...itemCarrinho.produto,
    quantidade: itemCarrinho.quantidade,
  }));
  const total = itens.reduce(
    (totalAcumulado, itemCarrinho) => totalAcumulado + itemCarrinho.precoVenda * itemCarrinho.quantidade,
    0,
  );
  const finalizar = async () => {
    if (finalizando) return;
    setFinalizando(true);
    try {
      const resposta = await api.post<Venda>(
        '/vendas',
        {
          clienteId: cliente || null,
          desconto: 0,
          formaPagamento: pag,
          itens: itens.map((item) => ({
            produtoId: item.id,
            quantidade: item.quantidade,
            desconto: 0,
          })),
        },
        { headers: { 'Idempotency-Key': chaveIdempotencia.current } },
      );
      setCarrinho({});
      chaveIdempotencia.current = crypto.randomUUID();
      setMsg(`Venda ${resposta.data.numero} concluída.`);
    } catch (erro) {
      setMsg(mensagemErro(erro));
    } finally {
      setFinalizando(false);
    }
  };
  return (
    <>
      <Titulo>Frente de caixa</Titulo>
      {msg && <p className={msg.includes('concluída') ? 'success' : 'error'}>{msg}</p>}
      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <h3 className="mb-3 font-bold">Produtos</h3>
          <Input
            className="mb-3"
            placeholder="Buscar produto"
            value={buscaProduto}
            onChange={(evento) => setBuscaProduto(evento.target.value)}
          />
          {produtos.dados.map((item) => (
            <button
              key={item.id}
              onClick={() =>
                setCarrinho((carrinhoAtual) => ({
                  ...carrinhoAtual,
                  [item.id]: {
                    produto: item,
                    quantidade: (carrinhoAtual[item.id]?.quantidade ?? 0) + 1,
                  },
                }))
              }
              className="mb-2 flex w-full justify-between rounded border p-3 text-left"
            >
              <span>
                {item.nome}
                <small className="block text-slate-500">Estoque {item.quantidadeEstoque}</small>
              </span>
              <strong>{dinheiro(item.precoVenda)}</strong>
            </button>
          ))}
          <Paginador
            paginaAtual={produtos.paginaAtual}
            totalPaginas={produtos.totalPaginas}
            totalItens={produtos.totalItens}
            mudarPagina={produtos.setPaginaAtual}
          />
        </Card>
        <Card>
          <h3 className="mb-3 font-bold">Carrinho</h3>
          {itens.map((item) => (
            <div key={item.id} className="flex items-center justify-between border-b py-2">
              <span>{item.nome}</span>
              <Input
                className="w-20"
                type="number"
                min={0}
                value={item.quantidade}
                onChange={(evento) =>
                  setCarrinho((carrinhoAtual) => ({
                    ...carrinhoAtual,
                    [item.id]: { produto: item, quantidade: Number(evento.target.value) },
                  }))
                }
              />
              <span>{dinheiro(item.precoVenda * item.quantidade)}</span>
            </div>
          ))}
          <select className="mt-4" value={cliente} onChange={(evento) => setCliente(evento.target.value)}>
            <option value="">Consumidor não identificado</option>
            {clientes.dados.map((item) => (
              <option key={item.id} value={item.id}>
                {item.nome}
              </option>
            ))}
          </select>
          <select className="mt-3" value={pag} onChange={(evento) => setPag(evento.target.value)}>
            <option>Dinheiro</option>
            <option>Pix</option>
            <option>CartaoDebito</option>
            <option>CartaoCredito</option>
          </select>
          <div className="my-5 flex justify-between text-xl font-bold">
            <span>Total</span>
            <span>{dinheiro(total)}</span>
          </div>
          <Button disabled={!itens.length || finalizando} onClick={finalizar} className="w-full">
            {finalizando ? 'Finalizando...' : 'Finalizar venda'}
          </Button>
        </Card>
      </div>
    </>
  );
}

export function VendasPage() {
  const listaVendas = useLista<Venda>('/vendas');
  return (
    <>
      <Titulo>Vendas</Titulo>
      <Card className="overflow-x-auto">
        {listaVendas.carregando ? <Loading /> : <TabelaVendas itens={listaVendas.dados} />}
      </Card>
      <Paginador
        paginaAtual={listaVendas.paginaAtual}
        totalPaginas={listaVendas.totalPaginas}
        totalItens={listaVendas.totalItens}
        mudarPagina={listaVendas.setPaginaAtual}
      />
    </>
  );
}
export function VendaDetalhePage() {
  const { id } = useParams();
  const [venda, setVenda] = useState<Venda>();
  const [erro, setErro] = useState('');
  const autenticacao = useAuth();
  const carregar = () =>
    api
      .get<Venda>(`/vendas/${id}`)
      .then((resposta) => setVenda(resposta.data))
      .catch((erro) => setErro(mensagemErro(erro)));
  useEffect(() => {
    api
      .get<Venda>(`/vendas/${id}`)
      .then((resposta) => setVenda(resposta.data))
      .catch((erro) => setErro(mensagemErro(erro)));
  }, [id]);
  if (erro) return <p className="error">{erro}</p>;
  if (!venda) return <Loading />;
  const cancelar = async () => {
    if (confirm('Confirma o cancelamento desta venda?')) {
      try {
        await api.post(`/vendas/${id}/cancelar`);
        await carregar();
      } catch (erro) {
        setErro(mensagemErro(erro));
      }
    }
  };
  return (
    <>
      <Titulo
        acao={
          autenticacao.perfil === 'Administrador' && venda.situacao === 'Concluida' ? (
            <Button onClick={cancelar}>Cancelar</Button>
          ) : undefined
        }
      >
        Venda {venda.numero}
      </Titulo>
      <Card>
        <p>
          {data(venda.dataVenda)} · {venda.cliente ?? 'Consumidor não identificado'} · {venda.formaPagamento} ·{' '}
          {venda.situacao}
        </p>
        <table className="mt-4">
          <thead>
            <tr>
              <th>Produto</th>
              <th>Qtd.</th>
              <th>Unitário</th>
              <th>Total</th>
            </tr>
          </thead>
          <tbody>
            {venda.itens.map((item) => (
              <tr key={item.produtoId}>
                <td>{item.produto}</td>
                <td>{item.quantidade}</td>
                <td>{dinheiro(item.precoUnitario)}</td>
                <td>{dinheiro(item.total)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <p className="mt-4 text-right text-xl font-bold">Total {dinheiro(venda.total)}</p>
      </Card>
    </>
  );
}

export function FinanceiroPage() {
  const listaFinanceiro = useLista<MovimentoFinanceiro>('/financeiro');
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState(0);
  const [erro, setErro] = useState('');
  const [tentouEnviar, setTentouEnviar] = useState(false);
  const criar = async () => {
    setTentouEnviar(true);
    if (!descricao.trim() || valor <= 0) return;
    try {
      await api.post('/financeiro/despesas', { descricao: descricao, valor: valor });
      setDescricao('');
      setValor(0);
      setTentouEnviar(false);
      listaFinanceiro.recarregar();
    } catch (erro) {
      setErro(mensagemErro(erro));
    }
  };
  return (
    <>
      <Titulo>Financeiro</Titulo>
      {erro && <p className="error">{erro}</p>}
      <Card>
        <div className="grid items-start gap-3 md:grid-cols-[2fr_1fr_auto]">
          <Campo
            label="Descrição da despesa"
            obrigatorio
            erro={tentouEnviar && !descricao.trim() ? 'Campo obrigatório.' : undefined}
          >
            <Input value={descricao} onChange={(evento) => setDescricao(evento.target.value)} />
          </Campo>
          <Campo label="Valor" obrigatorio erro={tentouEnviar && valor <= 0 ? 'Campo obrigatório.' : undefined}>
            <Input
              type="number"
              min="0.01"
              step="0.01"
              value={valor}
              onChange={(evento) => setValor(Number(evento.target.value))}
            />
          </Campo>
          <Button onClick={criar}>Registrar despesa</Button>
        </div>
      </Card>
      <Card className="mt-4">
        <table>
          <thead>
            <tr>
              <th>Data</th>
              <th>Tipo</th>
              <th>Descrição</th>
              <th>Valor</th>
            </tr>
          </thead>
          <tbody>
            {listaFinanceiro.dados.map((item) => (
              <tr key={item.id}>
                <td>{data(item.data)}</td>
                <td>{item.tipo}</td>
                <td>{item.descricao}</td>
                <td>{dinheiro(item.valor)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>
      <Paginador
        paginaAtual={listaFinanceiro.paginaAtual}
        totalPaginas={listaFinanceiro.totalPaginas}
        totalItens={listaFinanceiro.totalItens}
        mudarPagina={listaFinanceiro.setPaginaAtual}
      />
    </>
  );
}
export function UsuariosPage() {
  const listaUsuarios = useLista<Usuario>('/usuarios');
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [perfil, setPerfil] = useState<Perfil>('Operador');
  const [erro, setErro] = useState('');
  const [tentouEnviar, setTentouEnviar] = useState(false);
  const criar = async () => {
    setTentouEnviar(true);
    if (!nome.trim() || !email.trim() || !senha) return;
    try {
      await api.post('/usuarios', { nome: nome, email, senha, perfil });
      setNome('');
      setEmail('');
      setSenha('');
      setTentouEnviar(false);
      listaUsuarios.recarregar();
    } catch (erro) {
      setErro(mensagemErro(erro));
    }
  };
  return (
    <>
      <Titulo>Usuários</Titulo>
      {erro && <p className="error">{erro}</p>}
      <Card>
        <div className="grid items-start gap-3 md:grid-cols-5">
          <Campo label="Nome" obrigatorio erro={tentouEnviar && !nome.trim() ? 'Campo obrigatório.' : undefined}>
            <Input value={nome} onChange={(evento) => setNome(evento.target.value)} />
          </Campo>
          <Campo label="Email" obrigatorio erro={tentouEnviar && !email.trim() ? 'Campo obrigatório.' : undefined}>
            <Input type="email" value={email} onChange={(evento) => setEmail(evento.target.value)} />
          </Campo>
          <Campo label="Senha" obrigatorio erro={tentouEnviar && !senha ? 'Campo obrigatório.' : undefined}>
            <Input type="password" value={senha} onChange={(evento) => setSenha(evento.target.value)} />
          </Campo>
          <Campo label="Perfil" obrigatorio>
            <select value={perfil} onChange={(evento) => setPerfil(evento.target.value as Perfil)}>
              <option>Operador</option>
              <option>Administrador</option>
            </select>
          </Campo>
          <Button onClick={criar}>Criar</Button>
        </div>
      </Card>
      <Card className="mt-4">
        <table>
          <thead>
            <tr>
              <th>Nome</th>
              <th>Email</th>
              <th>Perfil</th>
              <th>Ativo</th>
            </tr>
          </thead>
          <tbody>
            {listaUsuarios.dados.map((item) => (
              <tr key={item.id}>
                <td>{item.nome}</td>
                <td>{item.email}</td>
                <td>{item.perfil}</td>
                <td>{item.ativo ? 'Sim' : 'Não'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>
      <Paginador
        paginaAtual={listaUsuarios.paginaAtual}
        totalPaginas={listaUsuarios.totalPaginas}
        totalItens={listaUsuarios.totalItens}
        mudarPagina={listaUsuarios.setPaginaAtual}
      />
    </>
  );
}

function Campo({
  label,
  children,
  obrigatorio = false,
  erro,
}: {
  label: string;
  children: React.ReactNode;
  obrigatorio?: boolean;
  erro?: string;
}) {
  return (
    <div>
      <label>
        {label}
        {obrigatorio && (
          <span className="ml-1 text-red-600" aria-hidden="true">
            *
          </span>
        )}
      </label>
      {children}
      {erro && (
        <small className="block text-red-600" role="alert">
          {erro}
        </small>
      )}
    </div>
  );
}
function TabelaProdutos({ itens }: { itens: Produto[] }) {
  return (
    <table>
      <thead>
        <tr>
          <th>Código</th>
          <th>Nome</th>
          <th>Preço</th>
          <th>Estoque</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {itens.map((item) => (
          <tr key={item.id}>
            <td>{item.codigo}</td>
            <td>{item.nome}</td>
            <td>{dinheiro(item.precoVenda)}</td>
            <td className={item.quantidadeEstoque <= item.estoqueMinimo ? 'font-bold text-red-600' : ''}>
              {item.quantidadeEstoque}
            </td>
            <td>
              <Link className="text-cyan-700" to={`/produtos/${item.id}/editar`}>
                Editar
              </Link>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
function TabelaVendas({ itens }: { itens: Venda[] }) {
  return itens.length === 0 ? (
    <Empty>Nenhuma venda.</Empty>
  ) : (
    <table>
      <thead>
        <tr>
          <th>Número</th>
          <th>Data</th>
          <th>Cliente</th>
          <th>Total</th>
          <th>Situação</th>
        </tr>
      </thead>
      <tbody>
        {itens.map((item) => (
          <tr key={item.id}>
            <td>
              <Link className="text-cyan-700" to={`/vendas/${item.id}`}>
                {item.numero}
              </Link>
            </td>
            <td>{data(item.dataVenda)}</td>
            <td>{item.cliente ?? '—'}</td>
            <td>{dinheiro(item.total)}</td>
            <td>{item.situacao}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
