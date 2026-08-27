import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { Navigate, NavLink, Route, Routes, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Package,
  Warehouse,
  ShoppingCart,
  Receipt,
  Wallet,
  CircleUserRound,
  LogOut,
} from 'lucide-react';
import type { Perfil } from './types';
import { api } from './lib/api';
import {
  DashboardPage,
  ClientesPage,
  ClienteFormPage,
  ClienteDetalhePage,
  ProdutosPage,
  ProdutoFormPage,
  EstoquePage,
  PdvPage,
  VendasPage,
  VendaDetalhePage,
  FinanceiroPage,
  UsuariosPage,
  LoginPage,
} from './pages';
interface Auth {
  autenticado: boolean;
  perfil: Perfil | null;
  entrar: (perfil: Perfil) => void;
  sair: () => void;
}
const AuthContext = createContext<Auth | null>(null);
export const useAuth = () => {
  const autenticacao = useContext(AuthContext);
  if (!autenticacao) throw new Error('Auth ausente');
  return autenticacao;
};
function Protegida({ children, admin = false }: { children: React.ReactNode; admin?: boolean }) {
  const autenticacao = useAuth();
  if (!autenticacao.autenticado) return <Navigate to="/login" replace />;
  if (admin && autenticacao.perfil !== 'Administrador') return <Navigate to="/clientes" replace />;
  return children;
}
const itens = [
  ['/dashboard', 'Dashboard', LayoutDashboard, true],
  ['/clientes', 'Clientes', Users, false],
  ['/produtos', 'Produtos', Package, false],
  ['/estoque', 'Estoque', Warehouse, true],
  ['/pdv', 'PDV', ShoppingCart, false],
  ['/vendas', 'Vendas', Receipt, false],
  ['/financeiro', 'Financeiro', Wallet, true],
  ['/usuarios', 'Usuários', CircleUserRound, true],
] as const;
function Layout() {
  const autenticacao = useAuth();
  return (
    <div className="min-h-screen md:flex">
      <aside className="bg-slate-900 p-4 text-white md:w-64">
        <h1 className="mb-6 text-xl font-bold">Gestão Comercial</h1>
        <nav className="space-y-1">
          {itens
            .filter((itemMenu) => !itemMenu[3] || autenticacao.perfil === 'Administrador')
            .map(([to, nome, Icon]) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-lg px-3 py-2 ${isActive ? 'bg-cyan-700' : 'hover:bg-slate-800'}`
                }
              >
                <Icon size={18} />
                {nome}
              </NavLink>
            ))}
        </nav>
        <button onClick={autenticacao.sair} className="mt-8 flex items-center gap-2 text-slate-300">
          <LogOut size={18} />
          Sair
        </button>
      </aside>
      <main className="min-w-0 flex-1 p-4 md:p-8">
        <Routes>
          <Route
            path="dashboard"
            element={
              <Protegida admin>
                <DashboardPage />
              </Protegida>
            }
          />
          <Route path="clientes" element={<ClientesPage />} />
          <Route path="clientes/novo" element={<ClienteFormPage />} />
          <Route path="clientes/:id" element={<ClienteDetalhePage />} />
          <Route path="clientes/:id/editar" element={<ClienteFormPage />} />
          <Route path="produtos" element={<ProdutosPage />} />
          <Route
            path="produtos/novo"
            element={
              <Protegida admin>
                <ProdutoFormPage />
              </Protegida>
            }
          />
          <Route
            path="produtos/:id/editar"
            element={
              <Protegida admin>
                <ProdutoFormPage />
              </Protegida>
            }
          />
          <Route
            path="estoque"
            element={
              <Protegida admin>
                <EstoquePage />
              </Protegida>
            }
          />
          <Route path="pdv" element={<PdvPage />} />
          <Route path="vendas" element={<VendasPage />} />
          <Route path="vendas/:id" element={<VendaDetalhePage />} />
          <Route
            path="financeiro"
            element={
              <Protegida admin>
                <FinanceiroPage />
              </Protegida>
            }
          />
          <Route
            path="usuarios"
            element={
              <Protegida admin>
                <UsuariosPage />
              </Protegida>
            }
          />
          <Route
            index
            element={<Navigate to={autenticacao.perfil === 'Administrador' ? '/dashboard' : '/clientes'} replace />}
          />
        </Routes>
      </main>
    </div>
  );
}
export function App() {
  const nav = useNavigate();
  const [perfil, setPerfil] = useState<Perfil | null>(null);
  const [verificandoSessao, setVerificandoSessao] = useState(true);
  useEffect(() => {
    api
      .get<{ perfil: Perfil }>('/auth/session')
      .then((resposta) => setPerfil(resposta.data.perfil))
      .catch(() => setPerfil(null))
      .finally(() => setVerificandoSessao(false));
  }, []);
  useEffect(() => {
    const encerrarSessao = () => {
      setPerfil(null);
      nav('/login');
    };
    window.addEventListener('auth:unauthorized', encerrarSessao);
    return () => window.removeEventListener('auth:unauthorized', encerrarSessao);
  }, [nav]);
  const valor = useMemo<Auth>(
    () => ({
      autenticado: perfil !== null,
      perfil,
      entrar: (novoPerfil) => {
        setPerfil(novoPerfil);
        nav('/');
      },
      sair: () => {
        void api.post('/auth/logout');
        setPerfil(null);
        nav('/login');
      },
    }),
    [perfil, nav],
  );
  if (verificandoSessao) return <div className="grid min-h-screen place-items-center">Carregando...</div>;
  return (
    <AuthContext.Provider value={valor}>
      <Routes>
        <Route path="/login" element={perfil ? <Navigate to="/" /> : <LoginPage />} />
        <Route
          path="/*"
          element={
            <Protegida>
              <Layout />
            </Protegida>
          }
        />
      </Routes>
    </AuthContext.Provider>
  );
}
