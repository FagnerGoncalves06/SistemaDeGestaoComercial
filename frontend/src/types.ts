export type Perfil = 'Administrador' | 'Operador';
export interface Pagina<T> {
  itens: T[];
  paginaAtual: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}
export interface Endereco {
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  uf: string;
}
export interface Cliente {
  id: string;
  nome: string;
  cpf: string;
  email?: string;
  telefone: string;
  dataNascimento?: string;
  endereco: Endereco;
  ativo: boolean;
}
export interface Produto {
  id: string;
  codigo: string;
  nome: string;
  descricao?: string;
  precoCusto: number;
  precoVenda: number;
  quantidadeEstoque: number;
  estoqueMinimo: number;
  ativo: boolean;
}
export interface ItemVenda {
  produtoId: string;
  produto: string;
  quantidade: number;
  precoUnitario: number;
  desconto: number;
  total: number;
}
export interface Venda {
  id: string;
  numero: string;
  clienteId?: string;
  cliente?: string;
  dataVenda: string;
  subtotal: number;
  desconto: number;
  total: number;
  formaPagamento: string;
  situacao: string;
  itens: ItemVenda[];
}
export interface MovimentoEstoque {
  id: string;
  produtoId: string;
  produto: string;
  tipo: string;
  quantidade: number;
  anterior: number;
  posterior: number;
  data: string;
  usuario: string;
  observacao?: string;
}
export interface MovimentoFinanceiro {
  id: string;
  tipo: string;
  descricao: string;
  valor: number;
  data: string;
  vendaId?: string;
}
export interface Dashboard {
  faturamentoDia: number;
  faturamentoMes: number;
  despesasDia: number;
  despesasMes: number;
  estornosDia: number;
  estornosMes: number;
  saldoDia: number;
  saldoMes: number;
  vendasDia: number;
  vendasMes: number;
  ticketMedioDia: number;
  ticketMedioMes: number;
  estoqueBaixo: Produto[];
}
export interface Usuario {
  id: string;
  nome: string;
  email: string;
  perfil: Perfil;
  ativo: boolean;
}
